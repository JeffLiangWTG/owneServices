using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business
{
	public class CreateIndividualLVSShipmentsStrategy : ConsolidationStrategy
	{
		#region Constructors

		public CreateIndividualLVSShipmentsStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

#if DEBUG
		public CreateIndividualLVSShipmentsStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
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
				query.AddToFilter(ZQuery.NoResultQuery);
			}
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			return false;
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			if (HasAcknowledged)
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
				declaration.JE_GB = Wrapper.BranchPK;
				declaration.LVXInvoiceHeader.JZ_OH_Buyer = Wrapper.ImporterPK;
				declaration.LVXInvoiceHeader.CA_PortOfClearance = Wrapper.PortOfClearanceCode;
				declaration.JE_EntryAuthorisationDate = Wrapper.EntryAuthorisationDate;
				declaration.JE_GS_NKCusAgent = Wrapper.Broker;
			}
		}

		#endregion

		#region GetEffectiveSetting

		protected override bool GetSettingFromRegistry()
		{
			return CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.GetFallBackValueAtAllLevels(Wrapper.CompanyPK.ToGuid(), Wrapper.BranchPK.ToGuid(), Guid.Empty);
		}

		protected override bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo.ZO_IsCreateIndividualLVS;
		}

		#endregion
	}
}
