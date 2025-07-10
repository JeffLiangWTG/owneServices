using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class SupportingDocumentsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.gridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.SupportingDocumentsSplitter = new CargoWise.Windows.UI.KSplitter();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SupportingDocumentDetailGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CSI_CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.SupportingDocumentDetailGroupBox.SuspendLayout();
			this.CSI_CodeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider);
			// 
			// gridSplitter
			// 
			this.gridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gridSplitter.DoNotSaveSplitterLayout = false;
			this.gridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
			this.gridSplitter.Name = "gridSplitter";
			this.gridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 3, true);
			this.gridSplitter.TabIndex = 0;
			this.gridSplitter.TabStop = false;
			// 
			// SupportingDocumentsSplitter
			// 
			this.SupportingDocumentsSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SupportingDocumentsSplitter.DoNotSaveSplitterLayout = false;
			this.SupportingDocumentsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.SupportingDocumentsSplitter.Name = "SupportingDocumentsSplitter";
			this.SupportingDocumentsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 3, true);
			this.SupportingDocumentsSplitter.TabIndex = 2;
			this.SupportingDocumentsSplitter.TabStop = false;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.SupportingDocumentDetailGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 108, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 108, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// SupportingDocumentDetailGroupBox
			// 
			this.SupportingDocumentDetailGroupBox.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("34b02a69-6317-471c-be0e-d458e1684913", "Details");
			this.SupportingDocumentDetailGroupBox.Controls.Add(this.CSI_CodeDropEdit);
			this.SupportingDocumentDetailGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.SupportingDocumentDetailGroupBox.Controls.Add(this.CSI_AdditionalDescriptionTextBox);
			this.SupportingDocumentDetailGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentDetailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentDetailGroupBox.Name = "SupportingDocumentDetailGroupBox";
			this.SupportingDocumentDetailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 108, true);
			this.SupportingDocumentDetailGroupBox.TabIndex = 0;
			this.SupportingDocumentDetailGroupBox.TabStop = false;
			// 
			// CSI_CodeDropEdit
			// 
			this.CSI_CodeDropEdit.AllowDrop = true;
			this.CSI_CodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_CodeDropEdit, "SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.CSI_CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 17, true);
			this.CSI_CodeDropEdit.Name = "CSI_CodeDropEdit";
			this.CSI_CodeDropEdit.ShouldResizeByMaxLength = true;
			this.CSI_CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CSI_CodeDropEdit.TabIndex = 0;
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 42, true);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CSI_ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CSI_AdditionalDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.CSI_AdditionalDescriptionTextBox, "SupportingDocuments.CSI_AdditionalDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
			this.CSI_AdditionalDescriptionTextBox.CaptionResourceString = null;
			this.CSI_AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 67, true);
			this.CSI_AdditionalDescriptionTextBox.Name = "CSI_AdditionalDescriptionTextBox";
			this.CSI_AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CSI_AdditionalDescriptionTextBox.TabIndex = 2;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.SupportingDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.ISupportingDocumentsProvider)(null)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_AdditionalDescription";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.GridId = "f34647a9-06c0-4d4c-9845-675d5ed44f89";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 5, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 122, true);
			this.SupportingDocumentsGrid.TabIndex = 0;
			// 
			// SupportingDocumentsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGrid);
			this.Controls.Add(this.gridSplitter);
			this.Controls.Add(this.BottomPanel);
			this.Name = "SupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 244, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.SupportingDocumentDetailGroupBox.ResumeLayout(false);
			this.SupportingDocumentDetailGroupBox.PerformLayout();
			this.CSI_CodeDropEdit.ResumeLayout(true);
			this.CSI_CodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitter SupportingDocumentsSplitter;
		protected CargoWise.Windows.UI.KSplitter gridSplitter;
		protected ZPanel BottomPanel;
		protected ZGroupBox SupportingDocumentDetailGroupBox;
		protected ZDropEdit CSI_CodeDropEdit;
		protected ZTextBox CSI_ReferenceNumberTextBox;
		protected ZTextBox CSI_AdditionalDescriptionTextBox;
		protected ZGrid SupportingDocumentsGrid;
	}
}
