using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eHubOutboundServiceTaskJob : OutboundEDIInterchangesServiceTaskJob
	{
#if DEBUG
		public
#endif
		eHubOutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier)
			: base(serviceTaskSupport, notifier)
		{
		}

		public eHubOutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

#if DEBUG
		public
#endif
		eHubOutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory, IZQueryFactory queryFactory, IDynamicBusinessObjectCollectionFactory dynamicBizoFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory, queryFactory, dynamicBizoFactory)
		{
		}

		protected override string PendingItemsIndex => EDIInterchangeSchema.Constants.Indexes.NR_RX__EI_IsActive_EI_ReceiveTransmit_EI_Status_EI_To_EI_GB_HQU;

		public bool ProcessSystemInterchanges { get; set; }

		internal override string MutexPrefix
		{
			get { return "EHO" + (ProcessSystemInterchanges ? "SYS" : "OTH"); }
		}

		protected override string GetBranchRecipientPairsExtraCondition
		{
			get { return string.Format((NoResString)"{0} {1} @SystemInterchangeType", EDIInterchange.Schema.EI_InterchangeType, ProcessSystemInterchanges ? "=" : "<>"); }
		}

		protected override void GetBranchRecipientPairsExtraParameters(ZSqlParameterCollection collection)
		{
			collection.Add(ZSqlParameter.New("@SystemInterchangeType", EDIInterchangeTypeList.Codes.SYS, EDIInterchangeSchema.EI_InterchangeType));
		}

		protected override void GetPendingItemsAddExtraFilters(ZQuery pendingItemsQuery)
		{
			pendingItemsQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, ProcessSystemInterchanges ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, EDIInterchangeTypeList.Codes.SYS);
		}

		internal override void CallAdapterToSendMessages(IeHubAdapter adapter)
		{
			LockInterchanges(candidatesInOutbox.Select(i => i.FullItem));
			base.CallAdapterToSendMessages(adapter);
		}

		void LockInterchanges(IEnumerable<EDIInterchange> items)
		{
			lockedInterchangePairs.Clear();
			foreach (var item in items)
			{
				NotifyVerbose(Res.GetString("B2035B3C-C7D7-4CDC-BDAE-540AA516AAF1", "Acquiring update lock for eHub Tracking ID: {0}", item.EI_SessionGUID));
				if (DbConnection.TryGetLock(MutexConstants.MessageMutexPrefix + item.EI_SessionGUID, InterchangeLockTimeout, out var mutex))
				{
					NotifyVerbose(Res.GetString("FC1BAF3F-8BC5-41EE-A282-509A4E73859D", "Update lock successfully acquired for eHub Tracking ID: {0}", item.EI_SessionGUID));
					lockedInterchangePairs.Add(item.PK, (item, mutex));
				}
				else
				{
					var key = (NoResString)"Outbound EDI Interchange Lock Timeout";
					ErrorReporter.ReportOnce(key, string.Format(Culture.Invariant, "{0} for eHub Tracking ID: {1}", key, item.EI_SessionGUID));
				}
			}
		}

		protected override void FailInterchanges(IEnumerable<EDIInterchange> items)
		{
			try
			{
				LockInterchanges(items);
				base.FailInterchanges(lockedInterchangePairs.Select(t => t.Value.Item1).Where(i => i != null));
			}
			finally
			{
				lockedInterchangePairs.ForEach(p => p.Value.Item2.Dispose());
				lockedInterchangePairs.Clear();
			}
		}

		protected override void FinalizeSend(IeHubAdapter adapter)
		{
			base.FinalizeSend(adapter);
			lockedInterchangePairs.ForEach(p => p.Value.Item2.Dispose());
			lockedInterchangePairs.Clear();
		}

		protected override void FinalizeInterchange(EDIInterchange interchange)
		{
			if (!lockedInterchangePairs.ContainsKey(interchange.PK))
			{
				return;
			}

			base.FinalizeInterchange(interchange);
		}

		protected internal override string InterchangeQueuedStatus
		{
			get { return EDIInterchange.Status.eHubQueued; }
		}

		internal override string InterchangeSuccessStatus
		{
			get { return EDIInterchange.Status.eHubPending; }
		}

		protected override BillingDataSource BillingDataSourceCode
		{
			get { return BillingDataSource.eHubOutbound; }
		}

		protected override BillingInterfaceName BillingInterfaceName
		{
			get { return BillingInterfaceName.eHubOutbound; }
		}

		internal override int AdapterOutboxCountLimit
		{
			get { return eHubMessagingRegistry.Instance.eHubOutboundSendLimitsRule.Value.SendCountLimit; }
		}

		internal override int AdapterOutboxSizeLimitInBytes
		{
			get { return 1024 * eHubMessagingRegistry.Instance.eHubOutboundSendLimitsRule.Value.SendSizeLimit; }
		}

		internal override int PendingItemsBatchSize => eHubMessagingRegistry.Instance.eHubOutboundPendingItemsBatchSize.Value;
		internal override int PendingItemsSearchLimit => eHubMessagingRegistry.Instance.eHubOutboundPendingItemsSearchLimit.Value;

		internal Dictionary<ZGuid, (EDIInterchange, SqlApplicationLock)> lockedInterchangePairs = new Dictionary<ZGuid, (EDIInterchange, SqlApplicationLock)>();
	}
}
