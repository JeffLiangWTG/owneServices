using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015GuaranteeReference : IIE013AndIIE015GuaranteeReference
	{
		public IE013AndIE015GuaranteeReference(NctsGuarantee guarantee)
		{
			this.guarantee = guarantee;
		}
		readonly NctsGuarantee guarantee;

		public string Grn => guarantee.PW_BondNumber;

		public string AccessCode => guarantee.PW_Password;

		public decimal AmountToBeCovered => guarantee.PW_BondAmount;

		public string Currency => Core.Constants.CurrencyCodes.EuropeanUnion;
	}
}
