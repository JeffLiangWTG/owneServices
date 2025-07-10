namespace Enterprise.Customs.DE.GUI
{
	partial class REXDISDeclarationUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.REXDISAWBDeclarationUserControl = new Enterprise.Customs.DE.GUI.REXDISAWBDeclarationUserControl();
			this.REXDISREGDeclarationUserControl = new Enterprise.Customs.DE.GUI.REXDISREGDeclarationUserControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LineGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CreatedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ATONumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcedureTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IdentificationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.REXDISAWBDeclarationUserControl.SuspendLayout();
			this.REXDISREGDeclarationUserControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.LinePanel.SuspendLayout();
			this.LineGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.CreatedDateDateEdit.SuspendLayout();
			this.ProcedureTypeDropEdit.SuspendLayout();
			this.IdentificationTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec);
			// 
			// REXDISAWBDeclarationUserControl
			// 
			this.REXDISAWBDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXDISAWBDeclarationUserControl, ".");
			this.REXDISAWBDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.REXDISAWBDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.REXDISAWBDeclarationUserControl.Name = "REXDISAWBDeclarationUserControl";
			this.REXDISAWBDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 138, true);
			this.REXDISAWBDeclarationUserControl.TabIndex = 0;
			// 
			// REXDISREGDeclarationUserControl
			// 
			this.REXDISREGDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXDISREGDeclarationUserControl, ".");
			this.REXDISREGDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.REXDISREGDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 428, true);
			this.REXDISREGDeclarationUserControl.Name = "REXDISREGDeclarationUserControl";
			this.REXDISREGDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 68, true);
			this.REXDISREGDeclarationUserControl.TabIndex = 1;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.LinePanel);
			this.MainPanel.Controls.Add(this.TopPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 530, true);
			this.MainPanel.TabIndex = 0;
			// 
			// LinePanel
			// 
			this.LinePanel.Controls.Add(this.LineGridGroupBox);
			this.LinePanel.Controls.Add(this.REXDISAWBDeclarationUserControl);
			this.LinePanel.Controls.Add(this.REXDISREGDeclarationUserControl);
			this.LinePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 34, true);
			this.LinePanel.Name = "LinePanel";
			this.LinePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 496, true);
			this.LinePanel.TabIndex = 0;
			// 
			// LineGridGroupBox
			// 
			this.LineGridGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("23E378E7-91E4-43E9-B20A-02645208B2B9", "Lines for Re-Export");
			this.LineGridGroupBox.Controls.Add(this.LinesGrid);
			this.LineGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LineGridGroupBox.Name = "LineGridGroupBox";
			this.LineGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 290, true);
			this.LineGridGroupBox.TabIndex = 0;
			this.LineGridGroupBox.TabStop = false;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_ReferenceNumberLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_DestinationPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			this.LinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TSL_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SumALine+TSL_ReferenceNumberLine";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.DE.GUI.Res.GetData("2aab79a1-20ee-4ed2-b3c3-f35fb7211124", "ATB");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SumALine+ReferenceNumber";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.DE.GUI.Res.GetData("2aab79a1-20ee-4ed2-b3c3-f35fb7211124", "ATB");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("9AF5EC0C-C5F2-4EB4-8B8B-9338C2D432A5", "New Owner Reference Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(159);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("6508683E-D255-4957-8240-650E4D90FD94", "New Owner Reference Number");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "TSL_OwnerReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(174);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("1200CDC7-C074-4344-8F79-22CC7D97BDB6", "Package Quantity");
			zCalcEditColumnStyleInfo3.ColumnName = "TSL_PackageQty";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("72E3A476-8018-417D-8ACB-A928294D4682", "Destination Place");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "TSL_DestinationPlace";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("E1890596-332F-423E-9234-7A97B26F39C1", "Customs Status");
			zTextBoxColumnStyleInfo4.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "65f1cbfd-aaf0-4c42-9799-3a85507997fd";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 271, true);
			this.LinesGrid.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.CreatedDateDateEdit);
			this.TopPanel.Controls.Add(this.StatusTextBox);
			this.TopPanel.Controls.Add(this.ATONumberTextBox);
			this.TopPanel.Controls.Add(this.ProcedureTypeDropEdit);
			this.TopPanel.Controls.Add(this.IdentificationTypeDropEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 34, true);
			this.TopPanel.TabIndex = 0;
			// 
			// CreatedDateDateEdit
			// 
			this.CreatedDateDateEdit.AllowDrop = true;
			this.CreatedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreatedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CreatedDateDateEdit, "STH_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).STH_SystemCreateTimeUtc)));
			this.CreatedDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.CreatedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(795, 9, true);
			this.CreatedDateDateEdit.Name = "CreatedDateDateEdit";
			this.CreatedDateDateEdit.TabIndex = 5;
			this.CreatedDateDateEdit.TabStop = false;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "STH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).STH_MessageStatus)));
			this.StatusTextBox.CaptionResourceString = null;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 9, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.StatusTextBox.TabIndex = 4;
			this.StatusTextBox.TabStop = false;
			// 
			// ATONumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ATONumberTextBox, "ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).ReferenceNumber)));
			this.ATONumberTextBox.CaptionResourceString = null;
			this.ATONumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 9, true);
			this.ATONumberTextBox.Name = "ATONumberTextBox";
			this.ATONumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.ATONumberTextBox.TabIndex = 3;
			this.ATONumberTextBox.TabStop = false;
			// 
			// ProcedureTypeDropEdit
			// 
			this.ProcedureTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcedureTypeDropEdit, "STH_DeclarationSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).STH_DeclarationSubType)));
			this.ProcedureTypeDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ba0a9ecc-8f51-4e8c-ac60-5b5bc34f8bd9", "Procedure Type");
			this.ProcedureTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 9, true);
			this.ProcedureTypeDropEdit.Name = "ProcedureTypeDropEdit";
			this.ProcedureTypeDropEdit.PreBoundMaxLength = 1;
			this.ProcedureTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ProcedureTypeDropEdit.ShowDescriptionBox = false;
			this.ProcedureTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.ProcedureTypeDropEdit.TabIndex = 2;
			// 
			// IdentificationTypeDropEdit
			// 
			this.IdentificationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IdentificationTypeDropEdit, "STH_IdentificationIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).STH_IdentificationIndicator)));
			this.IdentificationTypeDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("7DDCEE10-6583-412D-99C8-BA7742850988", "Identification Type");
			this.IdentificationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 9, true);
			this.IdentificationTypeDropEdit.Name = "IdentificationTypeDropEdit";
			this.IdentificationTypeDropEdit.PreBoundMaxLength = 3;
			this.IdentificationTypeDropEdit.ShouldResizeByMaxLength = true;
			this.IdentificationTypeDropEdit.ShowDescriptionBox = false;
			this.IdentificationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.IdentificationTypeDropEdit.TabIndex = 1;
			// 
			// REXDISDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "REXDISDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 530, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.REXDISAWBDeclarationUserControl.ResumeLayout(true);
			this.REXDISAWBDeclarationUserControl.PerformLayout();
			this.REXDISREGDeclarationUserControl.ResumeLayout(true);
			this.REXDISREGDeclarationUserControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.LinePanel.ResumeLayout(false);
			this.LinePanel.PerformLayout();
			this.LineGridGroupBox.ResumeLayout(false);
			this.LineGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.CreatedDateDateEdit.ResumeLayout(true);
			this.CreatedDateDateEdit.PerformLayout();
			this.ProcedureTypeDropEdit.ResumeLayout(true);
			this.ProcedureTypeDropEdit.PerformLayout();
			this.IdentificationTypeDropEdit.ResumeLayout(true);
			this.IdentificationTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZDropEdit IdentificationTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProcedureTypeDropEdit;
		private ZArchitecture.ZTextBox ATONumberTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.GUI.ZDateEdit CreatedDateDateEdit;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZGroupBox LineGridGroupBox;
		private ZArchitecture.ZGrid LinesGrid;
		private REXDISAWBDeclarationUserControl REXDISAWBDeclarationUserControl;
		private REXDISREGDeclarationUserControl REXDISREGDeclarationUserControl;
		public ZArchitecture.GUI.ZPanel LinePanel;
	}
}

