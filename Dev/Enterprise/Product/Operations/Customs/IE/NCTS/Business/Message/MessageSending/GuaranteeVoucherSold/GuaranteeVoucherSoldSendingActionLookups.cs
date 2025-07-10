using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeVoucherSoldSendingActionLookups : ZLookups
	{
		public GuaranteeVoucherSoldSendingActionLookups(GuaranteeVoucherSoldSendingAction sendingAction) : base(sendingAction) { }

		protected new GuaranteeVoucherSoldSendingAction Parent => (GuaranteeVoucherSoldSendingAction)base.Parent;

		public OrgHeaderCollection HolderOfTransitProcedure => new OrgHeaderCollection(Factory);

		public CustomsOfficeCodeCollection CustomsOfficeOfGuarantee
		{
			get
			{
				var countryCode = Parent.Header?.Branch?.GB_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return EUCustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, countryCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			}
		}
	}
}
