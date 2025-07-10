using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class FRAutoMessageSender
	{
		protected FRAutoMessageSender(ICommonLogger logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		readonly ICommonLogger logger;

		protected virtual int BatchSize => 100;

		protected abstract IEnumerable<ZGuid> GetCandidateCusEntryHeaderPKsPerBranch(BusinessObjectFactory factory);

		protected abstract ZString MessageType { get; }

		protected abstract bool ReadyToSend(CusEntryHeader entry);

		protected abstract bool RunInFirstActiveBranch { get; }

		public void Process(GlbCompany company)
		{
			foreach (var branch in company.ActiveBranches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					if (IsAutomationTurnedOn())
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Start to run {0} in {1}.", ProcessDescription, CurrentContext));
						Process();
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Finished running {0} in {1}.", ProcessDescription, CurrentContext));
					}
					else
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0} is not enabled in {1}.", ProcessDescription, CurrentContext));
					}
				}

				if (RunInFirstActiveBranch)
				{
					break;
				}
			}
		}

		protected abstract bool IsAutomationTurnedOn();

		protected void Process()
		{
			var factory = new BusinessObjectFactory();

			var entryHeaderPKs = GetCandidateCusEntryHeaderPKsPerBranch(factory).ToArray();
			if (entryHeaderPKs.Any())
			{
				foreach (var batchedPKs in entryHeaderPKs.Batch(BatchSize))
				{
					factory = factory ?? new BusinessObjectFactory();

					var entries = factory.Load<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.PK, batchedPKs));
					var entriesReadyToSend = entries.Where(ReadyToSend).OrderBy(x => x.CH_BGMReference).ToArray();

					if (entriesReadyToSend.Any())
					{
						ProcessEntries(entriesReadyToSend);
					}
					else
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "No entries ready to send in {0}.", CurrentContext));
					}

					factory = null;
				}
			}
			else
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "No candidate entries in {0}.", CurrentContext));
			}
		}

		public void ProcessEntries(CusEntryHeader[] entries)
		{
			var successfulEntries = new ZStringBuilder();

			foreach (var entry in entries)
			{
				try
				{
					DoExtendThingsBeforeSendingMessage(entry);
					var errorCollector = new ErrorCollector();
					var result = ZString.Empty;
					if (IsUCC6)
					{
						var messageObject = new DeltaIEJobDeclarationMessageSendingObject(entry)
						{
							MessageType = MessageType
						};

						result = new DeltaIEMessageSender(messageObject, errorCollector).Send();
					}
					else
					{
						var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry)
						{
							MessageType = MessageType
						};

						result = new DeltaGMessageSender(messageObject, errorCollector).Send();
					}

					if (errorCollector.ErrorCount == 0)
					{
						successfulEntries.Append(entry.CH_BGMReference);
					}
					else
					{
						logger.LogFormat(LogType.Error, (NoResString)"{0} for Customs Entry {1}", result, entry.CH_BGMReference);
					}

					logger.BumpSectionProgress();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					logger.Log(LogType.Error, "DELTA Auto Sending failed.", ex);
				}
			}

			if (!successfulEntries.IsEmpty)
			{
				logger.LogFormat(LogType.Information, (NoResString)"{0} have been sent for Customs Entries {1}", ProcessDescription, successfulEntries.ToStringWithDelimiterBetweenAppends(","));
			}
		}

		protected virtual bool IsUCC6 => false;

		protected virtual void DoExtendThingsBeforeSendingMessage(CusEntryHeader entry)
		{
		}

		string ProcessDescription => string.Format(CultureInfo.InvariantCulture, (NoResString)"Automated Delta {0} messages", MessageType);

		string CurrentContext
		{
			get
			{
				var context = string.Format(CultureInfo.InvariantCulture, (NoResString)"Company {0}", GlbCompany.CurrentCompany.GC_Code);
				if (!RunInFirstActiveBranch)
				{
					context += string.Format(CultureInfo.InvariantCulture, (NoResString)" Branch {0}", GlbBranch.CurrentBranch.GB_Code);
				}
				return context;
			}
		}
	}
}
