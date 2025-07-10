using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CommercialInvoiceDetailsUserControl
	{

		private void InitializeComponent()
		{
			this.IncoTermCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentTermsCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LetterOfCreditNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoteTextLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.InboundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingDocumentNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DRWApplicantDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupportingDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ValuationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncoTermCodeDropEdit.SuspendLayout();
			this.PaymentTermsCodeDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.NoteTextLongTextControl.SuspendLayout();
			this.InboundDateEdit.SuspendLayout();
			this.DRWApplicantDropEdit.SuspendLayout();
			this.SupportingDocumentTypeDropEdit.SuspendLayout();
			this.ValuationCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
			//
			// IncoTermCodeDropEdit
			//
			this.IncoTermCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermCodeDropEdit, "JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_IncoTerm)));
			this.IncoTermCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 37, true);
			this.IncoTermCodeDropEdit.Name = "IncoTermCodeDropEdit";
			this.IncoTermCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.IncoTermCodeDropEdit.TabIndex = 0;
			//
			// PaymentTermsCodeDropEdit
			//
			this.PaymentTermsCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTermsCodeDropEdit, "JZ_PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_PaymentTerms)));
			this.PaymentTermsCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 62, true);
			this.PaymentTermsCodeDropEdit.Name = "PaymentTermsCodeDropEdit";
			this.PaymentTermsCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.PaymentTermsCodeDropEdit.TabIndex = 3;
			//
			// LetterOfCreditNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.LetterOfCreditNumberTextBox, "JZ_LetterOfCreditNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_LetterOfCreditNumber)));
			this.LetterOfCreditNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 112, true);
			this.LetterOfCreditNumberTextBox.Name = "LetterOfCreditNumberTextBox";
			this.LetterOfCreditNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.LetterOfCreditNumberTextBox.TabIndex = 4;
			//
			// NoOfPacksCalcDropEdit
			//
			this.NoOfPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).NoOfPacksPackType)));
			this.NoOfPacksCalcDropEdit.BindToAmount = "JZ_NoOfPacks";
			this.NoOfPacksCalcDropEdit.BindToUnit = "NoOfPacksPackType";
			this.NoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 87, true);
			this.NoOfPacksCalcDropEdit.Name = "NoOfPacksCalcDropEdit";
			this.NoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.NoOfPacksCalcDropEdit.TabIndex = 5;
			//
			// NoteTextLongTextControl
			//
			this.NoteTextLongTextControl.AllowDrop = true;
			this.NoteTextLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NoteTextLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 137, true);
			this.NoteTextLongTextControl.Name = "NoteTextLongTextControl";
			this.NoteTextLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.NoteTextLongTextControl.TabIndex = 6;
			//
			// InboundDateEdit
			//
			this.InboundDateEdit.AllowDrop = true;
			this.InboundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InboundDateEdit, "JZ_InboundDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InboundDate)));
			this.InboundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 233, true);
			this.InboundDateEdit.Name = "InboundDateEdit";
			this.InboundDateEdit.TabIndex = 7;
			//
			// SupportingDocumentNoTextBox
			//
			this.BindingSource.SetBindingMember(this.SupportingDocumentNoTextBox, "SupportingDocumentReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).SupportingDocumentReferenceNumber)));
			this.SupportingDocumentNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 210, true);
			this.SupportingDocumentNoTextBox.Name = "SupportingDocumentNoTextBox";
			this.SupportingDocumentNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.SupportingDocumentNoTextBox.TabIndex = 8;
			//
			// DRWApplicantDropEdit
			//
			this.DRWApplicantDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DRWApplicantDropEdit, "JZ_DRWApplicantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_DRWApplicantType)));
			this.DRWApplicantDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 161, true);
			this.DRWApplicantDropEdit.Name = "DRWApplicantDropEdit";
			this.DRWApplicantDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.DRWApplicantDropEdit.TabIndex = 9;
			//
			// SupportingDocumentTypeDropEdit
			//
			this.SupportingDocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingDocumentTypeDropEdit, "SupportingDocumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).SupportingDocumentCode)));
			this.SupportingDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 186, true);
			this.SupportingDocumentTypeDropEdit.Name = "SupportingDocumentTypeDropEdit";
			this.SupportingDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.SupportingDocumentTypeDropEdit.TabIndex = 10;
			//
			// ValuationCodeDropEdit
			//
			this.ValuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationCodeDropEdit, "JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_ValuationCode)));
			this.ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 259, true);
			this.ValuationCodeDropEdit.Name = "ValuationCodeDropEdit";
			this.ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.ValuationCodeDropEdit.TabIndex = 11;
			//
			// CommercialInvoiceDetailsUserControl
			//
			this.Controls.Add(this.ValuationCodeDropEdit);
			this.Controls.Add(this.SupportingDocumentTypeDropEdit);
			this.Controls.Add(this.DRWApplicantDropEdit);
			this.Controls.Add(this.SupportingDocumentNoTextBox);
			this.Controls.Add(this.InboundDateEdit);
			this.Controls.Add(this.NoteTextLongTextControl);
			this.Controls.Add(this.NoOfPacksCalcDropEdit);
			this.Controls.Add(this.LetterOfCreditNumberTextBox);
			this.Controls.Add(this.PaymentTermsCodeDropEdit);
			this.Controls.Add(this.IncoTermCodeDropEdit);
			this.Name = "CommercialInvoiceDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 323, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncoTermCodeDropEdit.ResumeLayout(true);
			this.IncoTermCodeDropEdit.PerformLayout();
			this.PaymentTermsCodeDropEdit.ResumeLayout(true);
			this.PaymentTermsCodeDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.NoteTextLongTextControl.ResumeLayout(true);
			this.NoteTextLongTextControl.PerformLayout();
			this.InboundDateEdit.ResumeLayout(true);
			this.InboundDateEdit.PerformLayout();
			this.DRWApplicantDropEdit.ResumeLayout(true);
			this.DRWApplicantDropEdit.PerformLayout();
			this.SupportingDocumentTypeDropEdit.ResumeLayout(true);
			this.SupportingDocumentTypeDropEdit.PerformLayout();
			this.ValuationCodeDropEdit.ResumeLayout(true);
			this.ValuationCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ZDropEdit IncoTermCodeDropEdit;
		internal ZDropEdit PaymentTermsCodeDropEdit;
		internal ZTextBox LetterOfCreditNumberTextBox;
		internal ZCalcDropEdit NoOfPacksCalcDropEdit;
		internal Customs.GUI.LongTextControl NoteTextLongTextControl;
		public ZDateEdit InboundDateEdit;
		public ZTextBox SupportingDocumentNoTextBox;
		public ZDropEdit DRWApplicantDropEdit;
		public ZDropEdit SupportingDocumentTypeDropEdit;
		public ZDropEdit ValuationCodeDropEdit;
	}
}
