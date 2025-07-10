namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class NctsPreviousProceduresUserControl
	{
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
			this.PrevDocsClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviousProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProcedureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportSumARegisterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrevDocsClassDropEdit.SuspendLayout();
			this.PreviousProcedureDropEdit.SuspendLayout();
			this.ImportSumARegisterButton.SuspendLayout();
			this.ProcedureGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsClassDropEdit);
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 95, true);
			this.PrevDocsGroupBox.Visible = false;
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsClassDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			// 
			// PrevDocsReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceTextBox, "PreviousProcedures.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_ReferenceNumber)));
			this.PrevDocsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 67, true);
			// 
			// PrevDocsTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PrevDocsTypeDropEdit, "PreviousProcedures.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_Code)));
			this.PrevDocsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 19, true);
			// 
			// PreviousDocumentsGrid
			// 
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "PreviousProcedures");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_LineNo)));
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 164, true);
			this.PreviousDocumentsGrid.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ProcedureGroupBox);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 218, true);
			this.TopPanel.Controls.SetChildIndex(this.ProcedureGroupBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.PreviousDocumentsGrid, 0);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 95, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 95, true);
			this.BottomPanel.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// PrevDocsClassDropEdit
			// 
			this.PrevDocsClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrevDocsClassDropEdit, "PreviousProcedures.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedures)).SyncRoot)).CSI_SubType)));
			this.PrevDocsClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 43, true);
			this.PrevDocsClassDropEdit.Name = "PrevDocsClassDropEdit";
			this.PrevDocsClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
			this.PrevDocsClassDropEdit.TabIndex = 2;
			// 
			// PreviousProcedureDropEdit
			// 
			this.PreviousProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreviousProcedureDropEdit, "PreviousProcedureMaster.CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedureMaster.CSI_Procedure)));
			this.PreviousProcedureDropEdit.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("770970d9-ced6-41bc-9fda-c14d9c567111", "Previous Procedure");
			this.PreviousProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 17, true);
			this.PreviousProcedureDropEdit.Name = "PreviousProcedureDropEdit";
			this.PreviousProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 20, true);
			this.PreviousProcedureDropEdit.TabIndex = 0;
			//
			// ImportSumARegisterButton
			// 
			this.ImportSumARegisterButton.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("8D91D09C-BA62-4ED0-AEAE-A0D9146663F3", "Import from SumA Register");
			this.ImportSumARegisterButton.Name = "ImportSumARegisterButton";
			this.ImportSumARegisterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 17, true);
			this.ImportSumARegisterButton.Click += new System.EventHandler(this.ImportSumARegisterButtonClick);
			this.ImportSumARegisterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ImportSumARegisterButton.ToolTipCaption = null;
			this.ImportSumARegisterButton.TabIndex = 1;
			this.ImportSumARegisterButton.Visible = false;
			//
			// ProcedureGroupBox
			// 
			this.ProcedureGroupBox.AutoSize = true;
			this.ProcedureGroupBox.Controls.Add(this.PreviousProcedureDropEdit);
			this.ProcedureGroupBox.Controls.Add(this.ImportSumARegisterButton);
			this.ProcedureGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProcedureGroupBox, false);
			this.ProcedureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcedureGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ProcedureGroupBox.Name = "ProcedureGroupBox";
			this.ProcedureGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ProcedureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 54, true);
			this.ProcedureGroupBox.TabIndex = 0;
			this.ProcedureGroupBox.TabStop = false;
			// 
			// NctsPreviousProceduresUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "NctsPreviousProceduresUserControl";
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
			this.PrevDocsClassDropEdit.ResumeLayout(true);
			this.PrevDocsClassDropEdit.PerformLayout();
			this.PreviousProcedureDropEdit.ResumeLayout(true);
			this.PreviousProcedureDropEdit.PerformLayout();
			this.ImportSumARegisterButton.ResumeLayout(true);
			this.ImportSumARegisterButton.PerformLayout();
			this.ProcedureGroupBox.ResumeLayout(false);
			this.ProcedureGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox ProcedureGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PreviousProcedureDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PrevDocsClassDropEdit;
		internal ZArchitecture.GUI.ZButton ImportSumARegisterButton;
	}
}
