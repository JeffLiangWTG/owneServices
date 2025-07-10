using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillFetchStrategy : ManifestBase.AsycudaBillFetchStrategy
	{
		public AsycudaBillFetchStrategy(AsycudaBill bill)
			: base(bill)
		{
		}

		protected new AsycudaBill BusinessObject => (AsycudaBill)base.BusinessObject;

		protected override void AddCommonFetchHints()
		{
			base.AddCommonFetchHints();
			Factory.AddFetchHint(typeof(ABLEntryNum), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
		}

		protected override void AddFetchHintsForLoadChildEditableObjectsCore()
		{
			base.AddFetchHintsForLoadChildEditableObjectsCore();
			var businessObjectPK = BusinessObject.PK;
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, businessObjectPK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, businessObjectPK);
			if (ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(BusinessObject, Factory))
			{
				Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, businessObjectPK);
			}
		}

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			var businessObjectPK = BusinessObject.PK;
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, businessObjectPK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, businessObjectPK);
			Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, businessObjectPK);
			Factory.AddFetchHint(ProcessJobTriggerLinkSchema.P9L_ParentId, businessObjectPK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, businessObjectPK);
		}

		protected override IEnumerable<BusinessObject> LoadChildrenForDelete() => base.LoadChildrenForDelete().Union(BusinessObject.CustomsEntryNumbers);

		protected override void AddFetchHintsForValidateCore()
		{
			base.AddFetchHintsForValidateCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.ABL_OA_Shipper);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.ABL_OA_Consignee);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.ABL_OA_NotifyParty);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.ABL_OA_Forwarder);

			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.ABL_RX_NKCustomsValueCurrency);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.ABL_RX_NKFreightValueCurrency);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.ABL_RX_NKInsuranceValueCurrency);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.ABL_RX_NKTransportValueCurrency);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.DiscountValueCurrency);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.OtherChargesValueCurrency);

			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.ABL_RL_NKFinalDestination);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.ABL_RL_NKOrigin);
		}

		protected override IEnumerable<BusinessObject> LoadChildrenForValidate() => base.LoadChildrenForValidate()
			.Union(BusinessObject.CustomsEntryNumbers)
			.Union(new BusinessObject[] { BusinessObject.Consignee, BusinessObject.Forwarder, BusinessObject.NotifyParty, BusinessObject.Shipper });

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AsycudaBill.Schema.CustomsEntryNumber:
					case AsycudaBill.Schema.CustomsEntryNumberType:
					case AsycudaBill.Schema.RegistrationNumber:
						Factory.AddFetchHint(typeof(ABLEntryNum), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
