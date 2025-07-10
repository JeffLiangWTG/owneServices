using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Plugin
{
	public sealed class InvoiceLinePaymentControlBag : ControlBag
	{
		InvoiceLinePaymentControlBag()
		{
			CommercialPaymentCodeDropEdit = RegisterControl(nameof(InvoiceLinePaymentCountrySpecificUserControl.CommercialPaymentCodeDropEdit));
			PaymentAmountCalcEdit = RegisterControl(nameof(InvoiceLinePaymentCountrySpecificUserControl.PaymentAmountCalcEdit));
			PaymentNoTextBox = RegisterControl(nameof(InvoiceLinePaymentCountrySpecificUserControl.PaymentNoTextBox));
			PaymentDateEdit = RegisterControl(nameof(InvoiceLinePaymentCountrySpecificUserControl.PaymentDateEdit));
		}

		public static InvoiceLinePaymentControlBag Instance => instance ?? (instance = new InvoiceLinePaymentControlBag());

		[ThreadStatic]
		static InvoiceLinePaymentControlBag instance;

		protected override Control CreateTemplate() => new InvoiceLinePaymentCountrySpecificUserControl();

		public ControlReference CommercialPaymentCodeDropEdit { get; }
		public ControlReference PaymentAmountCalcEdit { get; }
		public ControlReference PaymentNoTextBox { get; }
		public ControlReference PaymentDateEdit { get; }
	}
}
