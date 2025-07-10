using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public abstract class BaseOutgoingMessageProcessor
	{
		protected BaseOutgoingMessageProcessor(LoggingInformation logger)
		{
			Logger = Argument.NotNull(logger, "logger");
		}

		public readonly LoggingInformation Logger;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public void ProcessMessage(CancellationToken token)
		{
			var messagePKsNotSaved = new Dictionary<ZGuid, byte>();

			while (true)
			{
				token.ThrowIfCancellationRequested();

				var factory = CreateFactory();
				var readyMessages = new NonDependentEDIMessageCollection(factory);
				readyMessages.Load(GetMessagesToProcessQuery(true));
				var queuedMessagesToProcessCount = readyMessages.Count;
				if (queuedMessagesToProcessCount == 0)
				{
					break;
				}

				var errorInPacking = PreProcessMessages(readyMessages);
				if (errorInPacking.IsEmpty)
				{
					HandleBeforeMessageProcessing(readyMessages, factory);
				}
				var messagePKsFailSaving3Times = new List<ZGuid>();
				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true, false);
					readyMessages.Cast<EDIMessage>().Select(x => x.PK).ForEach(x => messagePKsNotSaved.Remove(x));
				}
				catch (ZSaveException)
				{
					readyMessages.Cast<EDIMessage>().Select(x => x.PK).ForEach(x =>
					{
						if (messagePKsNotSaved.TryGetValue(x, out var value))
						{
							if (++value == 3)
							{
								messagePKsNotSaved.Remove(x);
								messagePKsFailSaving3Times.Add(x);
							}
							else
							{
								messagePKsNotSaved[x] = value;
							}
						}
						else
						{
							value = 1;
							messagePKsNotSaved.Add(x, value);
						}
					});
				}
				finally
				{
					if (errorInPacking.IsEmpty)
					{
						if (readyMessages.Count > 0)
						{
							Logger.Log(readyMessages.Count.ToString(CultureInfo.InvariantCulture) + " message(s) have been processed.");
						}
					}
					else
					{
						Logger.LogWarning(errorInPacking);
					}
				}

				MarkMessagesFailToSave(messagePKsFailSaving3Times);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Factory name for debugging")]
		protected virtual string FactoryFailSavingNameForDebugging => "Outgoing Message Fail Saving";

		void MarkMessagesFailToSave(List<ZGuid> messagePKsFailSaving3Times)
		{
			if (messagePKsFailSaving3Times.Count > 0)
			{
				var factory = new BusinessObjectFactory();
				factory.NameForDebugging = FactoryFailSavingNameForDebugging;
				factory.RefreshEnabled = false;
				foreach (var message in factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.PK, messagePKsFailSaving3Times)))
				{
					message.EM_Status = EDIMessage.Status.Failed;
					message.Notes.AddNew(true, InterchangeProviderBase.ProcessingLogDescription, Res.GetString("{CAFB7CB6-4AF1-45F4-86B8-DA3A02CAE644}", "Message fail to saving 3 times."));
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true, false);
				messagePKsFailSaving3Times.Clear();
			}
		}

		internal ZQuery GetMessagesToProcessQuery(bool includeBlob)
		{
			var filter = GetNewFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, true);

			var validTransmitDateMessageFilter = new ZQuery(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.Equal, null);
			validTransmitDateMessageFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, GetCurrentTime());
			filter.AddToFilter(validTransmitDateMessageFilter);

			filter.AddToFilter(MessageFilter);

			if (IsBranchFilter)
			{
				filter.AddToFilter(ValidBranchesForMessageFilter);
			}

			filter.MaximumRows = MaximumRows;

			if (includeBlob)
			{
				filter.IncludeBlob(EDIMessageSchema.EM_MessageText);
				filter.IncludeBlob(EDIMessageSchema.EM_MessageNText);
			}

			filter.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + "," + EDIMessage.Schema.EM_MessageNum;
			filter.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum));

			return filter;
		}

		ZDateTime GetCurrentTime() => ZDateTime.UtcNow;

		protected virtual ZQuery GetNewFilter()
		{
			return new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Factory name for debugging")]
		protected virtual string FactoryNameForDebugging => "Outgoing Message Processing";
		internal virtual BusinessObjectFactory CreateFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.NameForDebugging = FactoryNameForDebugging;
			factory.RefreshEnabled = false;
			return factory;
		}

		protected virtual bool IsBranchFilter => true;

		protected virtual void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory)
		{
		}

		protected abstract ZQuery MessageFilter { get; }

		protected virtual int MaximumRows => Env.Registry.MessagesPerInterchange;

		protected virtual ZString PreProcessMessages(NonDependentEDIMessageCollection readyMessages) => ZString.Empty;

		ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				var key = GlbCompany.CurrentCompany.PK;

				if (!BranchesForMessageFilterList.TryGetValue(key, out var result))
				{
					var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
					branchQuery.AddToFilter(GlbBranchSchema.GB_GC, key);

					result = new ZDBOnlyQuery(typeof(EDIMessage));
					((ZDBOnlyQuery)result).AddSubQuery(EDIMessageSchema.EM_GB, branchQuery, JoinCondition.And);

					BranchesForMessageFilterList.Add(key, result);
				}

				return result;
			}
		}

		Dictionary<ZGuid, ZQuery> BranchesForMessageFilterList
		{
			get { return branchesForMessageFilterList ?? (branchesForMessageFilterList = new Dictionary<ZGuid, ZQuery>()); }
		}
		Dictionary<ZGuid, ZQuery> branchesForMessageFilterList;
	}
}
