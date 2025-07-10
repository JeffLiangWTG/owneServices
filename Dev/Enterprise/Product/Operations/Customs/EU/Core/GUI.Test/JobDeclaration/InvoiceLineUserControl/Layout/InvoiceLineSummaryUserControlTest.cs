using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class InvoiceLineSummaryUserControlTest : TestCaseWithFactory
	{
		public void TestJGSTVATDeferredConvertToLocalCurrencyControl()
		{
			using var control = new InvoiceLineSummaryUserControl();
			CombineAssertions("GSTVATDeferredConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(InvoiceLineSummaryUserControl.GSTVATDeferredConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(JobComInvoiceLine.JI_Calc_GSTVATDeferred))
					.WithBindToUnit(nameof(JobComInvoiceLine.JI_RX_LocalCurrency))
					.WithCaption("Def. VAT")
				);
			});
		}

		public void TestStatisticalValueConvertToLocalCurrencyControl()
		{
			using var control = new InvoiceLineSummaryUserControl();
			CombineAssertions("StatisticalValueConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(InvoiceLineSummaryUserControl.StatisticalValueConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(JobComInvoiceLine.JI_Calc_StatisticalValue))
					.WithBindToUnit(nameof(JobComInvoiceLine.JI_RX_LocalCurrency))
					.WithCaption("Stat. Value")
				);
			});
		}

		public void TestValueForVatConvertToLocalCurrencyControl()
		{
			using var control = new InvoiceLineSummaryUserControl();
			CombineAssertions("ValueForVatConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(InvoiceLineSummaryUserControl.ValueForVatConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(JobComInvoiceLine.JI_Calc_ValueForVat))
					.WithBindToUnit(nameof(JobComInvoiceLine.JI_RX_LocalCurrency))
					.WithCaption("VAT Value")
				);
			});
		}

		public void TestCustomsValueConvertToLocalCurrencyControl()
		{
			using var control = new InvoiceLineSummaryUserControl();
			CombineAssertions("CustomsValueConvertToLocalCurrencyControl assertions", () =>
			{
				control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(InvoiceLineSummaryUserControl.CustomsValueConvertToLocalCurrencyControl), x => x
					.WithBindToAmount(nameof(JobComInvoiceLine.JI_CustomsValue))
					.WithBindToUnit(nameof(JobComInvoiceLine.JI_RX_LocalCurrency))
					.WithCaption("Customs Value")
				);
			});
		}
	}
}
