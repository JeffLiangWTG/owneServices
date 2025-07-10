using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CalculationOfTaxesProvider : ICalculationOfTaxes
{
	public CalculationOfTaxesProvider(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	readonly CusEntryLine entryLine;

	public IReadOnlyCollection<IDutiesAndTaxes> DutiesAndTaxes => dutiesAndTaxes ?? (dutiesAndTaxes =
		entryLine.Fees.Cast<CusEntryLineFee>()
			.Select((x, i) => new DutiesAndTaxesProvider(x, i + 1)).ToArray<IDutiesAndTaxes>());
	IReadOnlyCollection<IDutiesAndTaxes> dutiesAndTaxes;

	public decimal TotalDutiesAndTaxesAmount => entryLine.Fees.Count > 0 ? entryLine.Fees.Cast<CusEntryLineFee>().Sum(fee => fee.CF_ChargeAmount) : 0m;

	public string Preference => entryLine.InvoiceLines.Count > 0 ? entryLine.RandomLine.JI_PrimaryPreference : null;
}
