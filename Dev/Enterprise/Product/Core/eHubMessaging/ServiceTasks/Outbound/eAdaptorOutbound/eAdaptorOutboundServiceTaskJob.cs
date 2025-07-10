using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eAdaptorOutboundServiceTaskJob : OutboundEDIInterchangesServiceTaskJob
	{
#if DEBUG
		public
#endif
		eAdaptorOutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier)
			: base(serviceTaskSupport, notifier, new eAdaptorFactory())
		{
		}

		public eAdaptorOutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

#if DEBUG
		public
#endif
		eAdaptorOutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory, IZQueryFactory queryFactory, IDynamicBusinessObjectCollectionFactory dynamicBizoFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory, queryFactory, dynamicBizoFactory)
		{
		}

		protected override void ValidateDynamicProperties()
		{
			if (DynamicServerAddress == null && IsAddressInvalid(ServerAddress))
			{
				throw new UriFormatException("The eAdaptor Outbound Server Address is invalid");
			}
		}

		static bool IsAddressInvalid(string url)
		{
			return string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute);
		}

		protected internal override IEnumerable<SchemaColumn> ExtraColumnsToBatchPendingInterchangesBy
		{
			get
			{
				yield return EDIInterchangeSchema.EI_ECC_CommunicationPartyConfig;
			}
		}

		protected override string DynamicServerAddress
		{
			get
			{
				var communicationPartyConfig = GetCommunicationPartyConfig();
				if (communicationPartyConfig != null)
				{
					var address = communicationPartyConfig.ECC_Endpoint;
					if (IsAddressInvalid(address))
					{
						throw new UriFormatException($"The eAdaptor end point of Communication Party Config: {communicationPartyConfig?.Party?.ECP_Name} is invalid");
					}
					return address;
				}

				return null;
			}
		}

		protected override bool ShouldHoldLocksUntilAllBatchesAreProcessed => false;

		protected override IAdaptorFactory CreateDynamicAdaptorFactory()
		{
			var communicationPartyConfig = GetCommunicationPartyConfig();
			if (communicationPartyConfig != null)
			{
				return new eAdaptorDynamicFactory(communicationPartyConfig);
			}
			return null;
		}

		IEDICommunicationPartyConfig GetCommunicationPartyConfig()
		{
			if (TryGetCurrentBatchCommunicationConfigPK(out var communicationPartyConfigPk))
			{
				var cache = ObjectFactory.Get<ICommunicationPartyConfigCache>();
				if (cache.TryGetOutboundCommunicationPartyConfig(communicationPartyConfigPk, out var communicationPartyConfig))
				{
					return communicationPartyConfig;
				}
				throw new InvalidOperationException($"EDICommunicationPartyConfig could not be found but the PK [{communicationPartyConfigPk}] was present.");
			}
			return null;
		}

		bool TryGetCurrentBatchCommunicationConfigPK(out ZGuid communicationPartyConfigPk)
		{
			communicationPartyConfigPk = default;
			if (CurrentBatchCriteria != null && CurrentBatchCriteria.TryGetValue(EDIInterchangeSchema.EI_ECC_CommunicationPartyConfig.Name, out var config) && config is ZGuid configPk && configPk != ZGuid.Empty)
			{
				communicationPartyConfigPk = configPk;
				return true;
			}
			return false;
		}

		protected override string PendingItemsIndex => EDIInterchangeSchema.Constants.Indexes.NR_RX__EI_ReceiveTransmit_EI_Status_EI_SystemCreateTimeUtc_AQU;

		protected override bool CompanyShouldBeServiced(GlbCompany company)
		{
			return true;
		}

		protected override string ServerDescription
		{
			get { return Res.GetString("60264E4D-3311-4F7E-80BB-511248C6D326", "eAdaptor SDK Server"); }
		}

		internal override string MutexPrefix
		{
			get { return "EDP"; }
		}

		protected internal override string InterchangeQueuedStatus
		{
			get { return EDIInterchange.Status.eAdaptorQueued; }
		}

		internal override string InterchangeSuccessStatus
		{
			get { return EDIInterchange.Status.Sent; }
		}

		protected override BillingDataSource BillingDataSourceCode
		{
			get { return BillingDataSource.eAdaptorOutbound; }
		}

		protected override BillingInterfaceName BillingInterfaceName
		{
			get { return Enterprise.Billing.Integration.BillingInterfaceName.eAdaptorOutbound; }
		}

		internal override int AdapterOutboxCountLimit
		{
			get
			{
				return eAdaptorRegistry.Instance.eAdaptorOutboundSendLimitsRule.Value.SendCountLimit;
			}
		}

		internal override int AdapterOutboxSizeLimitInBytes
		{
			get
			{
				return 1024 * eAdaptorRegistry.Instance.eAdaptorOutboundSendLimitsRule.Value.SendSizeLimit;
			}
		}

		internal override IReadOnlyCollection<LightweightOutboundInterchangeCandidate> CheckFeatureControl(IReadOnlyCollection<LightweightOutboundInterchangeCandidate> items)
		{
			if (TryGetCurrentBatchCommunicationConfigPK(out _))
			{
				if (ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) == null)
				{
					Notifier.AddError(Res.GetString("2917FBBE-D381-4184-B930-7FC679FBA909", "eAdaptorNext features are deactivated, please contact WiseTech Global for further information."));
					FailInterchanges(items.Select(i => i.FullItem));
					return Array.Empty<LightweightOutboundInterchangeCandidate>();
				}
			}
			return items;
		}

		protected override void FailInterchanges(IEnumerable<EDIInterchange> items)
		{
			foreach (var interchange in items)
			{
				new MessageStatusManager(interchange).Update(EDIInterchangeStatusList.Codes.Failed, EDIInterchangeStatusList.Codes.Failed);
			}
			var interchangeNums = string.Join(",", items.Select(a => a.EI_InterchangeNum));

			FactoryProvider.SaveCurrentAndCreateNew();
			Notifier.AddWarning(Res.GetString("f46d00dd-9031-46dd-8e7d-c788d0016b93", "Interchange(s) Rejected ({0}).", interchangeNums));
		}

		internal override int PendingItemsBatchSize => eAdaptorRegistry.Instance.eAdaptorOutboundPendingItemsBatchSize.Value;
		internal override int PendingItemsSearchLimit => eAdaptorRegistry.Instance.eAdaptorOutboundPendingItemsSearchLimit.Value;
	}
}
