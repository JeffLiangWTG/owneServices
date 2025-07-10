using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Plugin
{
	public sealed class InvoicePaymentControlBag : ControlBag
	{
		InvoicePaymentControlBag()
		{
			CommercialPaymentCodeDropEdit = RegisterControl(nameof(InvoicePaymentCountrySpecificUserControl.CommercialPaymentCodeDropEdit));
			PaymentAmountCalcEdit = RegisterControl(nameof(InvoicePaymentCountrySpecificUserControl.PaymentAmountCalcEdit));
			PaymentNoTextBox = RegisterControl(nameof(InvoicePaymentCountrySpecificUserControl.PaymentNoTextBox));
			PaymentDateEdit = RegisterControl(nameof(InvoicePaymentCountrySpecificUserControl.PaymentDateEdit));
		}

		public static InvoicePaymentControlBag Instance => instance ?? (instance = new InvoicePaymentControlBag());

		[ThreadStatic]
		static InvoicePaymentControlBag instance;

		protected override Control CreateTemplate() => new InvoicePaymentCountrySpecificUserControl();

		public ControlReference CommercialPaymentCodeDropEdit { get; }
		public ControlReference PaymentAmountCalcEdit { get; }
		public ControlReference PaymentNoTextBox { get; }
		public ControlReference PaymentDateEdit { get; }
	}
}
