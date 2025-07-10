using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class AdjustmentNoteForm : BaseInvoicingForm
	{
		#region Controls

		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;
		#endregion

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AdjustmentNoteForm()
		{
		}

		public AdjustmentNoteForm(InvoicingBase businessEntity)
			: base(businessEntity)
		{
		}

		#region Form Overrides

		protected override string IncompleteFormCaption
		{
			get { return BusinessEntity is APAdjustmentNote ? ResString.GetMultilingualString("AdjustmentNoteForm|D41E6AC0-5A1F-4699-B91F-2426E27792CC", "Incomplete AP Adjustment Note") : ResString.GetMultilingualString("AdjustmentNoteForm|3FB1D3BA-62FC-4f6c-98C4-877D0795B913", "Incomplete AR Adjustment Note"); }
		}

		protected override string UnapprovedFormCaption
		{
			get { return BusinessEntity is APAdjustmentNote ? ResString.GetMultilingualString("AdjustmentNoteForm|B3B36064-C8D1-4A30-963C-1AF85C985328", "Unapproved AP Adjustment Note") : ResString.GetMultilingualString("AdjustmentNoteForm|191EE74D-436F-403D-80F1-2AEB71FD1027", "Unapproved AR Adjustment Note"); }
		}

		protected override string NormalFormCaption
		{
			get { return BusinessEntity is APAdjustmentNote ? ResString.GetMultilingualString("AdjustmentNoteForm|6837B365-848F-40DC-8BA3-955737533C3C", "AP Adjustment Note") : ResString.GetMultilingualString("AdjustmentNoteForm|CBC344A7-3FD9-47C3-95DC-CE768B8831B7", "AR Adjustment Note"); }
		}

		protected override string CaptionForInsertingIntoLabels
		{
			get { return Res.GetString("AdjustmentNoteForm|686a3b2f-f91a-4411-bd54-6de74ec1e695", "Adj. Note"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return false; }
		}

		protected override void Delete()
		{
			base.Delete();

			if (LastSaveSuccessful)
			{
				PromptForPrintAdjustmentNote();
			}
		}

		#region Handle Correct Controller ID

		protected override bool ShouldReopenWithCorrectControllerID(ControllerID correctID)
		{
			return ControllerID == ControllerIDs.APIncompleteAdjustmentNote && correctID == ControllerIDs.APAdjustmentNote;
		}

		#endregion

		#endregion

		#region Implementation

		#region Validate and Save - Print Invoice

		protected override ContinueWithSave ValidateAndSave()
		{
			bool isNew = !((BusinessObject)BusinessEntity).IsInDatabase;
			ContinueWithSave saveResult = base.ValidateAndSave();
			if (isNew && saveResult == ContinueWithSave.Yes)
			{
				PromptForPrintAdjustmentNote();
			}
			return saveResult;
		}

		void PromptForPrintAdjustmentNote()
		{
			CreditNotePrinter printer = new CreditNotePrinter();
			printer.PrintTransaction(Invoice, this, InvoicePrintContext.DontCare, true);
		}

		#endregion

		#endregion

		#region Dispose
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
		void LineChargesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.LineChargesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.LineChargesGrid.ResumeLayout(false);
			this.LineChargesGrid.PerformLayout();
		}
	}
}

