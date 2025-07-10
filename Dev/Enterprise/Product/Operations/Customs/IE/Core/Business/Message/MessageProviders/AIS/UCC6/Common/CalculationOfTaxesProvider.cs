using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class CalculationOfTaxesProvider : ICalculationOfTaxes
	{
		public CalculationOfTaxesProvider(EntryLineWrapper entryLineWrapper)
		{
			entryLine = entryLineWrapper.EntryLine;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
		}

		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine invoiceLine;

		public string Preference => invoiceLine.JI_PrimaryPreference;

		public decimal TotalDutiesAndTaxesAmount => entryLine.TotalDutyAndTaxesAmount;

		public IReadOnlyCollection<IDutiesAndTaxes> DutiesAndTaxes => dutiesAndTaxes ?? (dutiesAndTaxes = GetDutiesAndTaxes().ToArray());
		IReadOnlyCollection<IDutiesAndTaxes> dutiesAndTaxes;

		IReadOnlyCollection<IDutiesAndTaxes> GetDutiesAndTaxes() => entryLine.Fees.OfType<CusEntryLineFee>()
			.Select((x, i) => new DutiesandTaxesProvider(i + 1, x))
			.ToArray();
	}
}
