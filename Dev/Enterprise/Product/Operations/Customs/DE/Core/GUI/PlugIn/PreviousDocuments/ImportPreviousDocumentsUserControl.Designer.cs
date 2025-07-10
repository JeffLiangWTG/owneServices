namespace Enterprise.Customs.DE.GUI
{
	partial class ImportPreviousDocumentsUserControl
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProcedureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreviousProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportFromSumARegisterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProcedureGroupBox.SuspendLayout();
			this.PreviousProcedureDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 75, true);
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 20, true);
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 20, true);
			// 
			// PreviousDocumentsGrid
			// 
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "CustomsEntryInstructions.PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 184, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ProcedureGroupBox);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 238, true);
			this.TopPanel.Controls.SetChildIndex(this.ProcedureGroupBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.PreviousDocumentsGrid, 0);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 75, true);
			this.BottomPanel.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// ProcedureGroupBox
			// 
			this.ProcedureGroupBox.AutoSize = true;
			this.ProcedureGroupBox.Controls.Add(this.PreviousProcedureDropEdit);
			this.ProcedureGroupBox.Controls.Add(this.ImportFromSumARegisterButton);
			this.ProcedureGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProcedureGroupBox, false);
			this.ProcedureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcedureGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ProcedureGroupBox.Name = "ProcedureGroupBox";
			this.ProcedureGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ProcedureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 54, true);
			this.ProcedureGroupBox.TabIndex = 9;
			this.ProcedureGroupBox.TabStop = false;
			// 
			// PreviousProcedureDropEdit
			// 
			this.PreviousProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreviousProcedureDropEdit, "CustomsEntryInstructions.PreviousDocumentMaster.CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocumentMaster.CSI_Procedure)));
			this.PreviousProcedureDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("82b66f19-3188-49e5-bd57-9f422b3fde6d", "Previous Procedure");
			this.PreviousProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 17, true);
			this.PreviousProcedureDropEdit.Name = "PreviousProcedureDropEdit";
			this.PreviousProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 20, true);
			this.PreviousProcedureDropEdit.TabIndex = 0;
			// 
			// ImportFromSumARegisterButton
			// 
			this.ImportFromSumARegisterButton.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("FD41B059-9481-450D-8194-AE1EECC35CF0", "Import from SumA Register");
			this.ImportFromSumARegisterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 15, true);
			this.ImportFromSumARegisterButton.Name = "ImportFromSumARegisterButton";
			this.ImportFromSumARegisterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 22, true);
			this.ImportFromSumARegisterButton.TabIndex = 1;
			this.ImportFromSumARegisterButton.ToolTipCaption = null;
			this.ImportFromSumARegisterButton.Click += new System.EventHandler(this.ImportFromSumARegisterButton_Click);
			// 
			// ImportPreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ImportPreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 313, true);
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
			this.ProcedureGroupBox.ResumeLayout(false);
			this.ProcedureGroupBox.PerformLayout();
			this.PreviousProcedureDropEdit.ResumeLayout(true);
			this.PreviousProcedureDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZDropEdit PreviousProcedureDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ProcedureGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZButton ImportFromSumARegisterButton;

	}
}
