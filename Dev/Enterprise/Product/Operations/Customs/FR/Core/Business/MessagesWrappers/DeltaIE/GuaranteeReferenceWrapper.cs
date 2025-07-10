using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GuaranteeReferenceWrapper : IGuaranteeReference
	{
		GuaranteeReferenceWrapper(GuaranteeForEntryInstruction guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}
		readonly GuaranteeForEntryInstruction guarantee;

		public string AccessCode => accessCode ?? (accessCode = guarantee.PW_Password);
		string accessCode;

		public double AmountToBeCovered => (double)guarantee.PW_BondAmount;

		public string CcQualifier => null;

		public string CurrencyCode => currencyCode ?? (currencyCode = guarantee.PW_RX_NKCurrency);
		string currencyCode;

		public ICustomsOfficeOfGuarantee CustomsOfficeOfGuarantee => customsOfficeOfGuarantee ?? (customsOfficeOfGuarantee = CustomsOfficeOfGuaranteeWrapper.New(guarantee));
		ICustomsOfficeOfGuarantee customsOfficeOfGuarantee;

		public string Grn => grn ?? (grn = guarantee.PW_BondNumber);
		string grn;

		public string OtherGuaranteeReference => otherGuaranteeReference ?? (otherGuaranteeReference = guarantee.PW_BondNumber2);
		string otherGuaranteeReference;

		public static GuaranteeReferenceWrapper New(GuaranteeForEntryInstruction guarantee) => guarantee == null ? null : new GuaranteeReferenceWrapper(guarantee);
	}
}
