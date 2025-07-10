namespace Enterprise.Customs.EU.NCTS.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SupportingDocumentsSplitter = new CargoWise.Windows.UI.KSplitter();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupDocReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupDocTypeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SupDocReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.gridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.SupDocTypeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc);
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_Description)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(272);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(680);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "3b710cba-62e3-49e2-a94f-8370b1f1a0ee";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 160, true);
			this.SupportingDocumentsGrid.TabIndex = 1;
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
			this.BottomPanel.Controls.Add(this.SupportingDocumentsGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 159, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// SupportingDocumentsGroupBox
			//
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("D6DD165E-9EFB-4883-A1D2-7E1321A0F178", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocReasonTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocTypeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocReferenceTextBox);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 159, true);
			this.SupportingDocumentsGroupBox.TabIndex = 0;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// SupDocReasonTextBox
			// 
			this.SupDocReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SupDocReasonTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupDocReasonTextBox, "SupportingDocuments.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_Description)));
			this.SupDocReasonTextBox.CaptionResourceString = null;
			this.SupDocReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupDocReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 40, true);
			this.SupDocReasonTextBox.Multiline = true;
			this.SupDocReasonTextBox.Name = "SupDocReasonTextBox";
			this.SupDocReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 108, true);
			this.SupDocReasonTextBox.TabIndex = 2;
			// 
			// SupDocTypeFindBox
			// 
			this.SupDocTypeFindBox.AllowDrop = true;
			this.SupDocTypeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupDocTypeFindBox, "SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.SupDocTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 14, true);
			this.SupDocTypeFindBox.Name = "SupDocTypeFindBox";
			this.SupDocTypeFindBox.PreBoundMaxLength = 4;
			this.SupDocTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
			this.SupDocTypeFindBox.TabIndex = 1;
			// 
			// SupDocReferenceTextBox
			// 
			this.SupDocReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupDocReferenceTextBox, "SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.SupDocReferenceTextBox.CaptionResourceString = null;
			this.SupDocReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupDocReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.SupDocReferenceTextBox.Name = "SupDocReferenceTextBox";
			this.SupDocReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.SupDocReferenceTextBox.TabIndex = 0;
			// 
			// gridSplitter
			// 
			this.gridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gridSplitter.DoNotSaveSplitterLayout = false;
			this.gridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.gridSplitter.Name = "gridSplitter";
			this.gridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 10, true);
			this.gridSplitter.TabIndex = 2;
			this.gridSplitter.TabStop = false;
			// 
			// SupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGrid);
			this.Controls.Add(this.gridSplitter);
			this.Controls.Add(this.BottomPanel);
			this.Name = "SupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 329, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.SupDocTypeFindBox.ResumeLayout(true);
			this.SupDocTypeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitter SupportingDocumentsSplitter;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private CargoWise.Windows.UI.KSplitter gridSplitter;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox SupDocReasonTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox SupDocTypeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox SupDocReferenceTextBox;
		protected Enterprise.ZArchitecture.ZGrid SupportingDocumentsGrid;
	}
}
