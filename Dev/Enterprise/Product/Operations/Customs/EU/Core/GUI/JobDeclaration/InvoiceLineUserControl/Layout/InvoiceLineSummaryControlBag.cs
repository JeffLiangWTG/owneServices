using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class InvoiceLineSummaryControlBag : ControlBag
	{
		public static InvoiceLineSummaryControlBag Instance => invoiceLineDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<InvoiceLineSummaryControlBag> invoiceLineDetailsControlBag = new Lazy<InvoiceLineSummaryControlBag>(() => new InvoiceLineSummaryControlBag());

		protected override Control CreateTemplate() => new InvoiceLineSummaryUserControl();

		InvoiceLineSummaryControlBag()
		{
			GSTVATDeferredConvertToLocalCurrencyControl = RegisterControl(nameof(InvoiceLineSummaryUserControl.GSTVATDeferredConvertToLocalCurrencyControl));
			StatisticalValueConvertToLocalCurrencyControl = RegisterControl(nameof(InvoiceLineSummaryUserControl.StatisticalValueConvertToLocalCurrencyControl));
			ValueForVatConvertToLocalCurrencyControl = RegisterControl(nameof(InvoiceLineSummaryUserControl.ValueForVatConvertToLocalCurrencyControl));
			CustomsValueConvertToLocalCurrencyControl = RegisterControl(nameof(InvoiceLineSummaryUserControl.CustomsValueConvertToLocalCurrencyControl));
		}

		public ControlReference GSTVATDeferredConvertToLocalCurrencyControl { get; }
		public ControlReference StatisticalValueConvertToLocalCurrencyControl { get; }
		public ControlReference ValueForVatConvertToLocalCurrencyControl { get; }
		public ControlReference CustomsValueConvertToLocalCurrencyControl { get; }
	}
}
