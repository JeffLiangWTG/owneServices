using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLineChargesUserControl : Customs.GUI.InvoiceLineChargesUserControl
	{
		public InvoiceLineChargesUserControl()
		{
			using (ChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ChargesGrid.ColumnStyles.Add(EUCustomsSupplierHeaderUserControl.CreateIsStatisticalValueApplicableColumn());
				ChargesGrid.ColumnStyles.Add(EUCustomsSupplierHeaderUserControl.CreateFixedRateColumn());
				ChargesGrid.ColumnStyles.Add(EUCustomsSupplierHeaderUserControl.CreateExchangeRateColumn());
				ChargesGrid.ReOrderColumns(
					[
						InvoiceLineCharge.Schema.J7_ChargeType,
						InvoiceLineCharge.Schema.ChargeCodeDescription,
						InvoiceLineCharge.Schema.J7_Amount,
						InvoiceLineCharge.Schema.J7_RX_NKCurrency,
						InvoiceLineCharge.Schema.J7_IsDutiable,
						InvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable,
						InvoiceLineCharge.Schema.J7_IsGSTApplicable,
						InvoiceLineCharge.Schema.J7_Percentage,
						InvoiceLineCharge.Schema.J7_IsIncludedInITOT,
						InvoiceLineCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
						JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						InvoiceLineCharge.Schema.J7_ExchangeRate
					]);
			}
			using (ApportionedChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ApportionedChargesGrid.ColumnStyles.Add(EUCustomsSupplierHeaderUserControl.CreateIsStatisticalValueApplicableColumn());
				ApportionedChargesGrid.ColumnStyles.Add(EUCustomsSupplierHeaderUserControl.CreateFixedRateColumn());
				ApportionedChargesGrid.ColumnStyles.Add(EUCustomsSupplierHeaderUserControl.CreateExchangeRateColumn());
				ApportionedChargesGrid.ReOrderColumns(
					[
						InvoiceLineCharge.Schema.J7_ChargeType,
						InvoiceLineCharge.Schema.ChargeCodeDescription,
						InvoiceLineCharge.Schema.J7_Amount,
						InvoiceLineCharge.Schema.J7_RX_NKCurrency,
						InvoiceLineCharge.Schema.J7_IsDutiable,
						InvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable,
						InvoiceLineCharge.Schema.J7_IsGSTApplicable,
						InvoiceLineCharge.Schema.J7_IsIncludedInITOT,
						InvoiceLineCharge.Schema.J7_FullOrPartialApportionment,
						JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						InvoiceLineCharge.Schema.J7_ExchangeRate
					]);
			}
		}
	}
}
