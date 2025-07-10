#define CODE_ANALYSIS

using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration.CalculateTaxForCharge
{
	public interface ICalculateOSSellTaxForCharge
	{
		[SuppressMessage("Microsoft.Design", "CA1045: Do not pass types by reference")]
		[SuppressMessage("Microsoft.Design", "CA1007: Use generics where appropriate")]
		ZDecimal CalculateOSSellGSTAmount(LineDetails line, ChargeSellDetails charge, ref object cache);
	}

	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct LineDetails
	{
		public ZString Type;
		public ZString CurrencyCode;
		public ZDecimal LocalTaxAmount;
		public ZDecimal OSAmount;
	}

	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct ChargeSellDetails
	{
		public ZString InvoiceType;
		public ZDecimal SellExRate;
		public ZString SellCurrencyCode;
		public ZString SellInvoiceCurrencyCode;
		public ZString LocalCurrencyCode;
		public ZDecimal SellOSAmount;
		public ZDecimal LocalSellAmount;
		public ZGuid SellTaxRatePK;
		public ZDecimal? TaxRate;
		public ZDecimal? EffectiveExtraTaxRate;
		public ZGuid ChargeCodePK;
		public ZGuid CompanyPK;
		public ZString CountryCode;
	}
}
