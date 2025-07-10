using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class InvoiceLineCopyDocumentControlBag : ControlBag
	{
		InvoiceLineCopyDocumentControlBag()
		{
			InvoiceNumberDropEditGroupBox = RegisterControl(nameof(InvoiceLineCopyDocumentsUserControl.InvoiceNumberDropEditGroupBox));
		}

		public ControlReference InvoiceNumberDropEditGroupBox { get; }

		public static InvoiceLineCopyDocumentControlBag Instance => invoiceLineCopyDocumentControlBag.Value;

		protected override Control CreateTemplate() => new InvoiceLineCopyDocumentsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<InvoiceLineCopyDocumentControlBag> invoiceLineCopyDocumentControlBag = new(() => new InvoiceLineCopyDocumentControlBag());
	}
}
