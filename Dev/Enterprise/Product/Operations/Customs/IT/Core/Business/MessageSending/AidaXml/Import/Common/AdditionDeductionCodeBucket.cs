using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class AdditionDeductionCodeBucket
{
	public AdditionDeductionCodeBucket(string deductionCode)
	{
		Code = Argument.NotNullOrEmpty(deductionCode, nameof(Code));
	}

	public string Code { get; }

	public void AddCharge(IInvoiceLineChargeWrapper invoiceLineCharge)
	{
		Argument.NotNull(invoiceLineCharge, nameof(invoiceLineCharge));
		charges.Add(invoiceLineCharge);
	}

	public decimal CalculateAmount()
	{
		var amount = charges
			.GroupBy(c => c.LineId)
			.Sum(c => CalculateAmountForLine(c.ToArray()));
		return decimal.Round(amount, 2);
	}

	#region Implementation

	decimal CalculateAmountForLine(IInvoiceLineChargeWrapper[] lineCharges)
	{
		var chargesByCurrency = lineCharges
			.GroupBy(l => l.Currency?.Code ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			.Select(g => (g.Key, g.ToArray()))
			.ToCollection();

		return CalculateAmountByCurrency(chargesByCurrency);
	}

	decimal CalculateAmountByCurrency(IReadOnlyCollection<(string, IInvoiceLineChargeWrapper[])> chargesByCurrency)
	{
		var amount = 0m;
		foreach (var (currency, lineCharges) in chargesByCurrency)
		{
			amount += currency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
				? lineCharges.Sum(c => c.Amount)
				: SumAndConvertAmount(lineCharges);
		}

		return amount;
	}

	decimal SumAndConvertAmount(IInvoiceLineChargeWrapper[] invoiceLineChargeWrappers)
	{
		var (currencyConverter, currency) = invoiceLineChargeWrappers.Select(l => (l.CurrencyConverter, l.Currency))
			.FirstOrDefault(c => c.Currency != null && c.CurrencyConverter != null);

		var totalAmount = invoiceLineChargeWrappers.Sum(c => c.Amount);

		if (currencyConverter == null || currency == null)
		{
			return totalAmount;
		}

		return currencyConverter.ConvertRounded(new Money(totalAmount, currency), GlbCompany.CurrentCompany.LocalCurrency).Amount;
	}

	#endregion

	readonly List<IInvoiceLineChargeWrapper> charges = new List<IInvoiceLineChargeWrapper>();
}
