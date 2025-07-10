using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public sealed class InvoiceControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new InvoiceLayoutsUserControl();

		public static InvoiceControlBag Instance => instance ?? (instance = new InvoiceControlBag());

		[ThreadStatic]
		static InvoiceControlBag instance;

		InvoiceControlBag()
		{
			InvoiceDateDateEdit = RegisterControl(nameof(InvoiceLayoutsUserControl.InvoiceDateDateEdit));
			SequenceTextBox = RegisterControl(nameof(InvoiceLayoutsUserControl.SequenceTextBox));
			SupplierCodeFindBox = RegisterControl(nameof(InvoiceLayoutsUserControl.SupplierCodeFindBox));
			PreferenceAgreementDropEdit = RegisterControl(nameof(InvoiceLayoutsUserControl.PreferenceAgreementDropEdit));
			PaymentTermsDropEdit = RegisterControl(nameof(InvoiceLayoutsUserControl.PaymentTermsDropEdit));
			IncoTermsWithCountryCodeUserControl = RegisterControl(nameof(InvoiceLayoutsUserControl.IncoTermsWithCountryCodeUserControl));
		}

		public ControlReference InvoiceDateDateEdit { get; }
		public ControlReference SequenceTextBox { get; }
		public ControlReference SupplierCodeFindBox { get; }
		public ControlReference PreferenceAgreementDropEdit { get; }
		public ControlReference PaymentTermsDropEdit { get; }
		public ControlReference IncoTermsWithCountryCodeUserControl { get; }
	}
}
