using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public static class MexicoEInvoicingTestHelper
	{
		public static TaxID CreateTaxID(string taxCode, decimal rate, string taxTypeCode, string extraTaxTypeCode = "", decimal extraTaxRate = 0)
		{
			var taxType = new CodeDescriptionPair() { Code = taxTypeCode };
			var extraTaxType = new CodeDescriptionPair() { Code = extraTaxTypeCode };

			return new TaxID()
			{
				TaxCode = taxCode,
				TaxRate = rate,
				TaxType = taxType,
				ExtraTaxType = extraTaxType,
				ExtraTaxRate = extraTaxRate
			};
		}

		public static PostingJournal CreatePostingJournal(TaxID taxRate, decimal? oSAmount, decimal? oSGSTVATAmount, decimal? oSExtraVATAmount = 0)
		{
			return new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Sequence = 1,
				OSAmount = oSAmount,
				OSGSTVATAmount = oSGSTVATAmount,
				OSExtraVATAmount = oSExtraVATAmount,
				VATTaxID = taxRate
			};
		}
	}
}
