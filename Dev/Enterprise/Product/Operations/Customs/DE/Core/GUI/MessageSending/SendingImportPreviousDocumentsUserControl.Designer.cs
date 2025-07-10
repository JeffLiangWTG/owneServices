namespace Enterprise.Customs.DE.GUI
{
	public partial class SendingImportPreviousDocumentsUserControl
	{
		void InitializeComponent()
		{
			this.PreviousProcedureDropEdit.SuspendLayout();
			this.ProcedureGroupBox.SuspendLayout();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PreviousProcedureDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousProcedureDropEdit, "EntryInstruction.PreviousDocumentMaster.CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocumentMaster.CSI_Procedure)));
			// 
			// ProcedureGroupBox
			// 
			this.ProcedureGroupBox.Enabled = false;
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Enabled = false;
			// 
			// PrevDocsReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceTextBox, "EntryInstruction.PreviousDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			// 
			// PrevDocsTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PrevDocsTypeDropEdit, "EntryInstruction.PreviousDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_Code)));
			// 
			// PreviousDocumentsGrid
			// 
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "EntryInstruction.PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).CSI_CustomsOffice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).EntryInstruction.PreviousDocuments)).SyncRoot)).UsualProcessingFlag)));
			this.PreviousDocumentsGrid.ReadOnly = true;
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 184, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 238, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction);
			// 
			// SendingImportPreviousDocumentsUserControl
			// 
			this.Name = "SendingImportPreviousDocumentsUserControl";
			this.PreviousProcedureDropEdit.ResumeLayout(true);
			this.PreviousProcedureDropEdit.PerformLayout();
			this.ProcedureGroupBox.ResumeLayout(false);
			this.ProcedureGroupBox.PerformLayout();
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
