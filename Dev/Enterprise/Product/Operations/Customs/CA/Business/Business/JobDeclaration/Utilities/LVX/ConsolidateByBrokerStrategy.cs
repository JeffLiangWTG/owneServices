using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateByBrokerStrategy : ConsolidationStrategy
	{
		#region Constructors

		public ConsolidateByBrokerStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

#if DEBUG
		public ConsolidateByBrokerStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
			: base(wrapper, hasAcknowledged)
		{
		}
#endif

		#endregion

		#region IConsolidationStrategy Members

		public override void AddMatchingFilter(ZQuery query)
		{
			if (HasAcknowledged)
			{
				var broker = Wrapper.Broker;
				if (!broker.IsEmpty)
				{
					query.AddToFilter(JobDeclarationSchema.JE_GS_NKCusAgent, broker);
				}
			}
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			var broker = Wrapper.Broker;
			return !HasAcknowledged || declaration.JE_GS_NKCusAgent.IsEmpty || broker.IsEmpty || declaration.JE_GS_NKCusAgent == broker;
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			if (HasAcknowledged && declaration.JE_GS_NKCusAgent.IsEmpty)
			{
				var broker = Wrapper.Broker;
				if (!broker.IsEmpty)
				{
					declaration.JE_GS_NKCusAgent = broker;
				}
			}
		}

		#endregion

		#region GetEffectiveSetting

		protected override bool GetSettingFromRegistry()
		{
			return CACustomsDataRegistry.Instance.ConsolidateByBroker.GetFallBackValueAtAllLevels(Wrapper.CompanyPK.ToGuid(), Wrapper.BranchPK.ToGuid(), Guid.Empty);
		}

		protected override bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo.ZO_IsConsolidateByBroker;
		}

		#endregion
	}
}
