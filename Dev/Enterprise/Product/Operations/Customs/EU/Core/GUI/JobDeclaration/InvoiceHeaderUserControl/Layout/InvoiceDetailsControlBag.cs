using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class InvoiceDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new InvoiceDetailsUserControl();

		[ThreadStatic]
		static InvoiceDetailsControlBag instance;
		public static InvoiceDetailsControlBag Instance => instance ?? (instance = new InvoiceDetailsControlBag());

		InvoiceDetailsControlBag()
		{
			AgreedPlaceCodeFindBox = RegisterControl(nameof(InvoiceDetailsUserControl.AgreedPlaceCodeFindBox));
			TransportChargesMethodOfPaymentDropEdit = RegisterControl(nameof(InvoiceDetailsUserControl.TransportChargesMethodOfPaymentDropEdit));
			IncoTermsAgreedPlaceLongTextControl = RegisterControl(nameof(InvoiceDetailsUserControl.IncoTermsAgreedPlaceLongTextControl));
		}

		public ControlReference AgreedPlaceCodeFindBox { get; }
		public ControlReference TransportChargesMethodOfPaymentDropEdit { get; }
		public ControlReference IncoTermsAgreedPlaceLongTextControl { get; }
	}
}
