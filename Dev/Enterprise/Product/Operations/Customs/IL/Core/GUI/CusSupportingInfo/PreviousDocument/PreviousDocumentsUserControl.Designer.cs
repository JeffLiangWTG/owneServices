namespace Enterprise.Customs.IL.GUI
{
	partial class PreviousDocumentsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PrevDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PrevDocsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrevDocsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.JobDeclaration);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.PreviousDocumentsGrid);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 238, true);
			this.TopPanel.TabIndex = 0;
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "CustomsEntryInstructions.PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).ReferenceNumberFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			this.PreviousDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ReferenceNumberFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsGrid.GridId = "1268DB1A-9F74-4F24-B8BA-52B38A3A6097";
			this.PreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousDocumentsGrid.LayoutKey = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsGrid.Name = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 238, true);
			this.PreviousDocumentsGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PrevDocsGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 75, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 75, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.AutoSize = true;
			this.PrevDocsGroupBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("8FC86462-CBDF-4E2A-BCA5-EC0EE147E687", "Previous Documents");
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsReferenceTextBox);
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsTypeDropEdit);
			this.PrevDocsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrevDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrevDocsGroupBox.Name = "PrevDocsGroupBox";
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 75, true);
			this.PrevDocsGroupBox.TabIndex = 0;
			this.PrevDocsGroupBox.TabStop = false;
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PrevDocsReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceTextBox, "CustomsEntryInstructions.PreviousDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.PrevDocsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 45, true);
			this.PrevDocsReferenceTextBox.Name = "PrevDocsReferenceTextBox";
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
			this.PrevDocsReferenceTextBox.TabIndex = 1;
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.AllowDrop = true;
			this.PrevDocsTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrevDocsTypeDropEdit, "CustomsEntryInstructions.PreviousDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			this.PrevDocsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
			this.PrevDocsTypeDropEdit.Name = "PrevDocsTypeDropEdit";
			this.PrevDocsTypeDropEdit.PreBoundMaxLength = 4;
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
			this.PrevDocsTypeDropEdit.TabIndex = 0;
			// 
			// PreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomPanel);
			this.Name = "PreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 313, true);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox PrevDocsGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox PrevDocsReferenceTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PrevDocsTypeDropEdit;
		internal protected Enterprise.ZArchitecture.ZGrid PreviousDocumentsGrid;
		protected Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
	}
}
