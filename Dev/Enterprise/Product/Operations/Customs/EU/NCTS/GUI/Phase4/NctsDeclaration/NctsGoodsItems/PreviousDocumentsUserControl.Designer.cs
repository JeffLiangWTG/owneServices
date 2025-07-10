namespace Enterprise.Customs.EU.NCTS.GUI
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1  = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.PreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PreviousDocumentsSplitter = new CargoWise.Windows.UI.KSplitter();
			this.PreviousDocumentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PrevDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PrevDocsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrevDocsClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrevDocsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.PreviousDocumentsPanel.SuspendLayout();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsClassDropEdit.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.PreviousDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo2.IsCustomColumn = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zMultiControlColumnStyleInfo1 .CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1 .ColumnName = "CSI_ReferenceNumber";
			zMultiControlColumnStyleInfo1 .IsCustomColumn = false;
			zMultiControlColumnStyleInfo1 .Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(361);
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ReferenceNumberFieldType";
			this.PreviousDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1 );
			this.PreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsGrid.GridId = "5D4271B1-7658-474D-BD0D-223B8F76F1A9";
			this.PreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousDocumentsGrid.LayoutKey = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsGrid.Name = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 111, true);
			this.PreviousDocumentsGrid.TabIndex = 6;
			// 
			// PreviousDocumentsSplitter
			// 
			this.PreviousDocumentsSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PreviousDocumentsSplitter.DoNotSaveSplitterLayout = false;
			this.PreviousDocumentsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.PreviousDocumentsSplitter.Name = "PreviousDocumentsSplitter";
			this.PreviousDocumentsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 3, true);
			this.PreviousDocumentsSplitter.TabIndex = 7;
			this.PreviousDocumentsSplitter.TabStop = false;
			// 
			// PreviousDocumentsPanel
			// 
			this.PreviousDocumentsPanel.Controls.Add(this.PrevDocsGroupBox);
			this.PreviousDocumentsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PreviousDocumentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
			this.PreviousDocumentsPanel.Name = "PreviousDocumentsPanel";
			this.PreviousDocumentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 90, true);
			this.PreviousDocumentsPanel.TabIndex = 8;
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e97bd7f8-c279-4485-8ccb-311579a15764", "[40] Previous Documents");
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsReferenceTextBox);
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsClassDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsTypeDropEdit);
			this.PrevDocsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrevDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrevDocsGroupBox.Name = "PrevDocsGroupBox";
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 90, true);
			this.PrevDocsGroupBox.TabIndex = 0;
			this.PrevDocsGroupBox.TabStop = false;
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PrevDocsReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceTextBox, "PreviousDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.PrevDocsReferenceTextBox.CaptionResourceString = null;
			this.PrevDocsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 62, true);
			this.PrevDocsReferenceTextBox.Name = "PrevDocsReferenceTextBox";
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 20, true);
			this.PrevDocsReferenceTextBox.TabIndex = 7;
			// 
			// PrevDocsClassDropEdit
			// 
			this.PrevDocsClassDropEdit.AllowDrop = true;
			this.PrevDocsClassDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrevDocsClassDropEdit, "PreviousDocuments.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			this.PrevDocsClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 38, true);
			this.PrevDocsClassDropEdit.Name = "PrevDocsClassDropEdit";
			this.PrevDocsClassDropEdit.PreBoundMaxLength = 4;
			this.PrevDocsClassDropEdit.ShouldResizeByMaxLength = true;
			this.PrevDocsClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 20, true);
			this.PrevDocsClassDropEdit.TabIndex = 5;
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.AllowDrop = true;
			this.PrevDocsTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrevDocsTypeDropEdit, "PreviousDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			this.PrevDocsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.PrevDocsTypeDropEdit.Name = "PrevDocsTypeDropEdit";
			this.PrevDocsTypeDropEdit.PreBoundMaxLength = 4;
			this.PrevDocsTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 20, true);
			this.PrevDocsTypeDropEdit.TabIndex = 3;
			// 
			// PreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousDocumentsGrid);
			this.Controls.Add(this.PreviousDocumentsSplitter);
			this.Controls.Add(this.PreviousDocumentsPanel);
			this.Name = "PreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 204, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.PreviousDocumentsPanel.ResumeLayout(false);
			this.PreviousDocumentsPanel.PerformLayout();
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsClassDropEdit.ResumeLayout(true);
			this.PrevDocsClassDropEdit.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitter PreviousDocumentsSplitter;
		protected Enterprise.ZArchitecture.GUI.ZPanel PreviousDocumentsPanel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox PrevDocsGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox PrevDocsReferenceTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PrevDocsClassDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PrevDocsTypeDropEdit;
		protected Enterprise.ZArchitecture.ZGrid PreviousDocumentsGrid;
	}
}
