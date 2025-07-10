using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GuaranteeReferenceWrapper : IGuaranteeReference
	{
		GuaranteeReferenceWrapper(EU.NCTS.Business.NctsGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}

		readonly EU.NCTS.Business.NctsGuarantee guarantee;

		public static GuaranteeReferenceWrapper New(EU.NCTS.Business.NctsGuarantee guarantee) => guarantee == null ? null : new GuaranteeReferenceWrapper(guarantee);

		public string Grn => grn ?? (grn = guarantee.CusGuarantee?.GetApplicationSpecificReference(OrgCusAccountDeltaTTypeList.Codes.TR) ?? guarantee.PW_BondNumber);
		string grn;

		public string AccessCode => accessCode ?? (accessCode = guarantee.PW_Password);
		string accessCode;

		public decimal? AmountToBeCovered => amountToBeCovered ?? (amountToBeCovered = guarantee.PW_BondAmount);
		decimal? amountToBeCovered;

		public string Currency => currency ?? (currency = guarantee.PW_RX_NKCurrency);
		string currency;
	}
}
