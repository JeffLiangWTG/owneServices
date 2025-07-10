using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CustomsValuationProvider : ICustomsValuation
{
	public CustomsValuationProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(JobComInvoiceLine));
		this.entryLine = invoiceLine.CusEntryLine;
	}

	protected readonly JobComInvoiceLine invoiceLine;
	protected readonly EU.Business.Declaration.CusEntryLine entryLine;

	public IReadOnlyCollection<IItemAdditionsAndDeductionsType> AdditionsAndDeductions => additionsAndDeductions ?? (additionsAndDeductions = GetAdditionsAndDeductions());
	IReadOnlyCollection<IItemAdditionsAndDeductionsType> additionsAndDeductions;

	IReadOnlyCollection<IItemAdditionsAndDeductionsType> GetAdditionsAndDeductions()
	{
		var apportionedCharges = new Dictionary<string, decimal>();
		foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
		{
			foreach (var apportionedCharge in invoiceLine.ApportionedCharges)
			{
				var dictionaryCharge = apportionedCharges.GetOrAdd(apportionedCharge.J7_ChargeType);
				apportionedCharges[apportionedCharge.J7_ChargeType] = dictionaryCharge + apportionedCharge.MoneyInLocalCurrency.Amount;
			}

			foreach (var charge in invoiceLine.Charges)
			{
				var dictionaryCharge = apportionedCharges.GetOrAdd(charge.J7_ChargeType);
				apportionedCharges[charge.J7_ChargeType] = dictionaryCharge + charge.MoneyInInvoiceCurrency.Amount;
			}
		}

		return apportionedCharges.Select((y, index) => new AdditionsAndDeductionsProvider(index + 1, y.Key, y.Value)).ToArray();
	}

	public string ValuationMethod => invoiceLine.JI_ValuationCode;
}
