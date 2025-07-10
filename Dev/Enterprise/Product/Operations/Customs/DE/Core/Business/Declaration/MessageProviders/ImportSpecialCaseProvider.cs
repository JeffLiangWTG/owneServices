using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class ImportSpecialCaseProvider : IImportSpecialCase
	{
		public ImportSpecialCaseProvider(JobComInvoiceLineTax tax)
		{
			this.tax = tax;
		}
		protected readonly JobComInvoiceLineTax tax;

		public string Group => tax.JLT_Type;

		public string ApplicationType => tax.JLT_MethodOfCalculation;

		public decimal RateOrAmountOrFactor => tax.JLT_Rate.FormatDecimal(5);
	}
}
