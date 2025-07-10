using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class CustomsValuationProvider : ICustomsValuation
	{
		public CustomsValuationProvider(EntryLineWrapper entryLineWrapper)
		{
			entryLine = entryLineWrapper.EntryLine;
			randomInvoiceLine = entryLineWrapper.RandomInvoiceLine;
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine randomInvoiceLine;

		public string ValuationMethod => randomInvoiceLine.JI_ValuationCode;

		public IReadOnlyCollection<IAdditionsAndDeductions> AdditionsAndDeductions => additionsAndDeductions ??= AISMessageProviderHelper.GetAdditionsAndDeductions(entryLine.InvoiceLines.Cast<JobComInvoiceLine>());
		IReadOnlyCollection<IAdditionsAndDeductions> additionsAndDeductions;

		public decimal ItemAmount => randomInvoiceLine.JI_LinePriceInLocalCurrency;

		public string Preference => randomInvoiceLine.JI_PrimaryPreference;

		public decimal PostalvalueAmount => 0m;

		public string PostalvalueCurrency => null;
	}
}
