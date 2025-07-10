using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	partial class HCProgramsBasedUserUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		protected System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GTINNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufactureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExceptProcessing1CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BatchLotNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ModelNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ComponentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComponentGridUserControl = new Enterprise.Customs.CA.GUI.ComponentUserControl();
			this.LPCOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IntendedUseCodeDropEdit.SuspendLayout();
			this.ManufactureDateEdit.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			this.ExpiryDateEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ManufacturerUserControl.SuspendLayout();
			this.ComponentGroupBox.SuspendLayout();
			this.ComponentGridUserControl.SuspendLayout();
			this.LPCOGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.HCPGAHeader);
			// 
			// IntendedUseCodeDropEdit
			// 
			this.IntendedUseCodeDropEdit.AllowDrop = true;
			this.IntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("50F77534-09D6-4887-A095-905DE8E52586", "Intended", "Intended Use", "Intended Use Code");
			this.IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 14, true);
			this.IntendedUseCodeDropEdit.Name = "IntendedUseCodeDropEdit";
			this.IntendedUseCodeDropEdit.ShouldResizeByMaxLength = true;
			this.IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.IntendedUseCodeDropEdit.TabIndex = 1;
			// 
			// GTINNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.GTINNumberTextBox, "CA_GTINNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_GTINNumber)));
			this.GTINNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A5F42509-A55E-490D-AB48-9D7215C42B80", "GTIN", "GTIN No.", "GTIN Number");
			this.GTINNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 14, true);
			this.GTINNumberTextBox.Name = "GTINNumberTextBox";
			this.GTINNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.GTINNumberTextBox.TabIndex = 10;
			// 
			// ManufactureDateEdit
			// 
			this.ManufactureDateEdit.AllowDrop = true;
			this.ManufactureDateEdit.AutoCompleteMonthThreshold = 1;
			this.ManufactureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ManufactureDateEdit, "InvoiceLine.CA_ProductionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.CA_ProductionDate)));
			this.ManufactureDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("77919CB2-36BE-4BCE-BCB5-2D475F74FF1B", "Manuf.", "Manuf. Date", "Manufacture Date");
			this.ManufactureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 92, true);
			this.ManufactureDateEdit.Name = "ManufactureDateEdit";
			this.ManufactureDateEdit.TabIndex = 4;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.CategoryDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C9ABFAB5-CDB0-486A-A4B8-935A131CD1EF", "Commodity Type");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.ShouldResizeByMaxLength = true;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CategoryDropEdit.TabIndex = 2;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "InvoiceLine.JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_BrandName)));
			this.BrandNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A2F30B2B-91D2-46FA-90A3-FDC0F9B9EE6D", "Brand Name");
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 40, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.BrandNameTextBox.TabIndex = 19;
			// 
			// ExpiryDateEdit
			// 
			this.ExpiryDateEdit.AllowDrop = true;
			this.ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExpiryDateEdit, "InvoiceLine.CA_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.CA_ExpiryDate)));
			this.ExpiryDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("FAB14695-E6BB-4ADE-AABC-450FA0704266", "Expiry Date");
			this.ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1084, 40, true);
			this.ExpiryDateEdit.Name = "ExpiryDateEdit";
			this.ExpiryDateEdit.TabIndex = 9;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.ExceptProcessing1CheckBox);
			this.DetailsGroupBox.Controls.Add(this.TradeNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.IntendedUseCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.GTINNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.ManufactureDateEdit);
			this.DetailsGroupBox.Controls.Add(this.CategoryDropEdit);
			this.DetailsGroupBox.Controls.Add(this.BrandNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.BatchLotNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.ExpiryDateEdit);
			this.DetailsGroupBox.Controls.Add(this.ManufacturerUserControl);
			this.DetailsGroupBox.Controls.Add(this.ModelNameTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3273, 112, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// ExceptProcessing1CheckBox
			// 
			this.ExceptProcessing1CheckBox.AutoSize = true;
			this.ExceptProcessing1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExceptProcessing1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 92, true);
			this.ExceptProcessing1CheckBox.Name = "ExceptProcessing1CheckBox";
			this.ExceptProcessing1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExceptProcessing1CheckBox.TabIndex = 13;
			this.ExceptProcessing1CheckBox.UseVisualStyleBackColor = true;
			// 
			// TradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.TradeNameTextBox, "InvoiceLine.CA_TradeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.CA_TradeName)));
			this.TradeNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3ABC79F0-5C11-48C0-A4F3-44D7B1E1B689", "Trade Name");
			this.TradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 92, true);
			this.TradeNameTextBox.Name = "TradeNameTextBox";
			this.TradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.TradeNameTextBox.TabIndex = 5;
			// 
			// BatchLotNumberTextBox
			// 
			this.BatchLotNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BatchLotNumberTextBox, "CA_BatchLotNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_BatchLotNumber)));
			this.BatchLotNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0727AEFA-AA5B-44FC-B575-CACD32D1D9E1", "Batch/Lot", "Batch/Lot No.", "Batch/Lot Number");
			this.BatchLotNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 66, true);
			this.BatchLotNumberTextBox.Name = "BatchLotNumberTextBox";
			this.BatchLotNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.BatchLotNumberTextBox.TabIndex = 5;
			// 
			// ManufacturerUserControl
			// 
			this.ManufacturerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerUserControl, "OA_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).OA_Manufacturer)));
			this.ManufacturerUserControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.ManufacturerUserControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3460E53F-C7BA-4E95-8357-0FBED84B4F83", "Manuf.", "Manufacturer", "Manufacturer Address");
			this.ManufacturerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.ManufacturerUserControl.Name = "ManufacturerUserControl";
			this.ManufacturerUserControl.PopupCaption = "";
			this.ManufacturerUserControl.ReadOnly = false;
			this.ManufacturerUserControl.ShowAddress = false;
			this.ManufacturerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ManufacturerUserControl.TabIndex = 3;
			// 
			// ModelNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelNameTextBox, "InvoiceLine.JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_Model)));
			this.ModelNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("09FAF64B-71A4-49CA-AA79-618C909B3982", "Model Name");
			this.ModelNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 118, true);
			this.ModelNameTextBox.Name = "ModelNameTextBox";
			this.ModelNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ModelNameTextBox.TabIndex = 17;
			// 
			// ComponentGroupBox
			// 
			this.ComponentGroupBox.Controls.Add(this.ComponentGridUserControl);
			this.ComponentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 0, true);
			this.ComponentGroupBox.Name = "ComponentGroupBox";
			this.ComponentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2923, 1111, true);
			this.ComponentGroupBox.TabIndex = 2;
			this.ComponentGroupBox.TabStop = false;
			this.ComponentGroupBox.Text = "Ingredients";
			// 
			// ComponentGridUserControl
			// 
			this.ComponentGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComponentGridUserControl, "Components");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.ComponentCollection)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).Components)));
			this.ComponentGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ComponentGridUserControl.Name = "ComponentGridUserControl";
			this.ComponentGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2917, 1092, true);
			this.ComponentGridUserControl.TabIndex = 0;
			// 
			// LPCOGroupBox
			// 
			this.LPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOGroupBox.Name = "LPCOGroupBox";
			this.LPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 1111, true);
			this.LPCOGroupBox.TabIndex = 1;
			this.LPCOGroupBox.TabStop = false;
			this.LPCOGroupBox.Text = "LPCOs";
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 1092, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ComponentGroupBox);
			this.BottomPanel.Controls.Add(this.LPCOGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3273, 1111, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// HCProgramsBasedUserUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "HCProgramsBasedUserUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3273, 1223, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IntendedUseCodeDropEdit.ResumeLayout(true);
			this.IntendedUseCodeDropEdit.PerformLayout();
			this.ManufactureDateEdit.ResumeLayout(true);
			this.ManufactureDateEdit.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.ExpiryDateEdit.ResumeLayout(true);
			this.ExpiryDateEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ManufacturerUserControl.ResumeLayout(true);
			this.ManufacturerUserControl.PerformLayout();
			this.ComponentGroupBox.ResumeLayout(false);
			this.ComponentGroupBox.PerformLayout();
			this.ComponentGridUserControl.ResumeLayout(true);
			this.ComponentGridUserControl.PerformLayout();
			this.LPCOGroupBox.ResumeLayout(false);
			this.LPCOGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ComponentGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox LPCOGroupBox;
		internal ComponentUserControl ComponentGridUserControl;
		internal LPCOGridUserControl LPCOGridUserControl;
		internal ZArchitecture.GUI.ZDropEdit IntendedUseCodeDropEdit;
		internal ZArchitecture.ZTextBox GTINNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ManufactureDateEdit;
		internal ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		internal ZTextBox BrandNameTextBox;
		internal ZTextBox BatchLotNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ExpiryDateEdit;
		internal ZArchitecture.GUI.ZAddressControl ManufacturerUserControl;
		internal ZTextBox TradeNameTextBox;
		internal ZArchitecture.ZTextBox ModelNameTextBox;
		internal ZArchitecture.GUI.ZCheckBox ExceptProcessing1CheckBox;
		internal ZArchitecture.GUI.ZPanel BottomPanel;
	}
}
