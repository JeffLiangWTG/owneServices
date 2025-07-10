using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		public MiscOptionsControlBag() : base()
		{
			DefermentAccountNumberDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.DefermentAccountNumberDropEdit));
			VATDeferTypeDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATDeferTypeDropEdit));
			VatCanaDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VatCanaDropEdit));
			VATDeferNumberTextBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATDeferNumberTextBox));
			ChargePaymentOrDestinationIDsDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.ChargePaymentOrDestinationIDsDropEdit));
			CustomsGuaranteeNumberDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.CustomsGuaranteeNumberDropEdit));
			PaymentSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentSeparatorUserControl));
			SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.SupportingInformationUserControl));
		}

		public ControlReference DefermentAccountNumberDropEdit { get; }

		public ControlReference VATDeferTypeDropEdit { get; }

		public ControlReference VatCanaDropEdit { get; }

		public ControlReference VATDeferNumberTextBox { get; }

		public ControlReference ChargePaymentOrDestinationIDsDropEdit { get; }

		public ControlReference CustomsGuaranteeNumberDropEdit { get; }

		public ControlReference PaymentSeparatorUserControl { get; }

		public ControlReference SupportingInformationUserControl { get; }

		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
	}
}
