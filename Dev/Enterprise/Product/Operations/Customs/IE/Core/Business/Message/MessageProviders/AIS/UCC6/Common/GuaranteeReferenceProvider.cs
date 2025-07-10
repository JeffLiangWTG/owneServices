using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class GuaranteeReferenceProvider : IGuaranteeReference
	{
		public GuaranteeReferenceProvider(GuaranteeForEntryInstruction guarantee, ZShort sequenceNumber)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));

			this.sequenceNumber = sequenceNumber.ToString();
		}
		readonly GuaranteeForEntryInstruction guarantee;
		readonly string sequenceNumber;

		public string AccessCode => guarantee.PW_Password;

		public string CurrencyCode => guarantee.PW_RX_NKCurrency;

		public decimal AmountToBeCovered => guarantee.PW_BondAmount;

		public string OtherGuaranteeReference => guarantee.PW_GuaranteeDescription;

		public string CustomsOfficeOfGuarantee => guarantee.PW_BondFiledPort;

		public string Grn => guarantee.PW_BondNumber;

		public string CcQualifier => guarantee.PW_RN_NKCountryOfIssue;

		public string SequenceNumber => sequenceNumber;
	}
}
