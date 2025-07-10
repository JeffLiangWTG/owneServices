using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage
{
	public static class EInvoicingPenaltyTaxInfoFormFactory
	{
		public static IEInvoicingPenaltyTaxInfoProvider CreateEInvoicingPenaltyInfoFormProvider(string countryCode)
		{
			switch (countryCode)
			{
				case CountryCodes.KoreaSouth:
					return new KoreaSouthEInvoicingPenaltyTaxInfoFormProvider();
				default:
					return null;
			}
		}
	}
}
