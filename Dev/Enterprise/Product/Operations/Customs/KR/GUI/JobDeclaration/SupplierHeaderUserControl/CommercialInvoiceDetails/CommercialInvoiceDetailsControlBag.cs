using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CommercialInvoiceDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new CommercialInvoiceDetailsUserControl();

		[ThreadStatic]
		static CommercialInvoiceDetailsControlBag instance;

		public static CommercialInvoiceDetailsControlBag Instance => instance ?? (instance = new CommercialInvoiceDetailsControlBag());

		CommercialInvoiceDetailsControlBag()
		{
			IncoTermCodeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.IncoTermCodeDropEdit));
			NoOfPacksCalcDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.NoOfPacksCalcDropEdit));
			PaymentTermsCodeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.PaymentTermsCodeDropEdit));
			LetterOfCreditNumberTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.LetterOfCreditNumberTextBox));
			NoteTextLongTextControl = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.NoteTextLongTextControl));

			DRWApplicantDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.DRWApplicantDropEdit));
			SupportingDocumentTypeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.SupportingDocumentTypeDropEdit));
			SupportingDocumentNoTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.SupportingDocumentNoTextBox));
			InboundDateEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InboundDateEdit));
			ValuationCodeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.ValuationCodeDropEdit));
		}

		public ControlReference IncoTermCodeDropEdit { get; }
		public ControlReference NoOfPacksCalcDropEdit { get; }
		public ControlReference PaymentTermsCodeDropEdit { get; }
		public ControlReference LetterOfCreditNumberTextBox { get; }
		public ControlReference NoteTextLongTextControl { get; }

		public ControlReference DRWApplicantDropEdit { get; }
		public ControlReference SupportingDocumentTypeDropEdit { get; }
		public ControlReference SupportingDocumentNoTextBox { get; }
		public ControlReference InboundDateEdit { get; }

		public ControlReference ValuationCodeDropEdit { get; }
	}
}
