using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeAccessCodesSendingObjectLookups : ZLookups
	{
		public GuaranteeAccessCodesSendingObjectLookups(GuaranteeAccessCodesSendingObject sendingObject) : base(sendingObject) { }

		protected new GuaranteeAccessCodesSendingObject Parent => (GuaranteeAccessCodesSendingObject)base.Parent;

		public CustomsOfficeCodeCollection OfficeCodeList
		{
			get
			{
				var countryCode = Parent.CusGuaranteeHeader.CPH_RN_NKCountryCode;
				return EUCustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, countryCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			}
		}
	}
}
