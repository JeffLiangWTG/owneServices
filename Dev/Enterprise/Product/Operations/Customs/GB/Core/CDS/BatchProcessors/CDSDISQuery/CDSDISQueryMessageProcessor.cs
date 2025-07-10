using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.CDS.Messaging.MessageManagers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDISQueryMessageProcessor
	{
		public CDSDISQueryMessageProcessor(LoggingInformation logger)
		{
			Logger = Argument.NotNull(logger, "logger");
			notificationCollection = new Customs.Business.MessageSendingNotificationCollection();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public void ProcessMessage(CancellationToken token)
		{
			notificationCollection.Clear();
			var messagePKsNotSaved = new Dictionary<ZGuid, byte>();

			while (true)
			{
				token.ThrowIfCancellationRequested();

				var factory = CreateFactory();
				using (factory.AddDisposableService())
				{
					var readyMessages = new NonDependentEDIMessageCollection(factory);
					readyMessages.Load(GetMessagesToProcessQuery(true));

					var queuedMessagesToProcessCount = readyMessages.Count;
					if (queuedMessagesToProcessCount == 0)
					{
						break;
					}

					foreach (CDSDISQueryMessage message in readyMessages)
					{
						var universalEvent = message.GetUniversalEvent();
						var deliveredSuccesfully = false;

						if (universalEvent != null)
						{
							deliveredSuccesfully = CDSQuerySendingHelper.Deliver(factory, universalEvent, null, EHubID, notificationCollection, message);
						}

						if (!deliveredSuccesfully)
						{
							message.EM_Status = EDIMessageStatusList.Codes.Failed;
							factory.Save();

							Logger.Log(Integration.LogType.Information, ZString.Format("Message {0] has failed to be packaged into an interchange and has been marked as failed", message.EM_MessageNum));
						}
						else
						{
							Logger.Log(Integration.LogType.Information, ZString.Format("Message {0} has been packaged into interchange {1} and queued for sending via eHub", message.EM_MessageNum, message.Interchange.EI_InterchangeNum));
						}
					}
				}
			}
		}

		protected string FactoryNameForDebugging => "CDS DIS Query Message Processing"; // Factory name for debugging
		internal BusinessObjectFactory CreateFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.NameForDebugging = FactoryNameForDebugging;
			factory.RefreshEnabled = false;
			return factory;
		}

		protected ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCDSDISQuery);
			result.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			return result;
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

			filter.AddToFilter(ValidBranchesForMessageFilter);

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

		int MaximumRows => Env.Registry.MessagesPerInterchange;

		ZDateTime GetCurrentTime() => ZDateTime.Now;

		ZQuery GetNewFilter()
		{
			return new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
		}

		ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				if (!BranchesForMessageFilterList.TryGetValue(GlbCompany.CurrentCompany.PK, out var result))
				{
					result = new ZQuery(EDIMessageSchema.EM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
					BranchesForMessageFilterList.Add(GlbCompany.CurrentCompany.PK, result);
				}
				return result;
			}
		}

		Dictionary<ZGuid, ZQuery> BranchesForMessageFilterList
		{
			get { return branchesForMessageFilterList ?? (branchesForMessageFilterList = new Dictionary<ZGuid, ZQuery>()); }
		}
		Dictionary<ZGuid, ZQuery> branchesForMessageFilterList;

		public readonly LoggingInformation Logger;
		readonly Customs.Business.MessageSendingNotificationCollection notificationCollection;
		const string EHubID = "GBCustoms";
	}
}
