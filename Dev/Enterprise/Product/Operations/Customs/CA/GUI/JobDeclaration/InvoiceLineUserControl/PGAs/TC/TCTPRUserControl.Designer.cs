using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	partial class TCTPRUserControl
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
			this.CommodityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.USImporterDeclarationStateDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.USImporterDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImporterDeclarationStateDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TireClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TireTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TireSizeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ImportReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MakeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommodityGroupBox.SuspendLayout();
			this.TireClassDropEdit.SuspendLayout();
			this.TireTypeDropEdit.SuspendLayout();
			this.TireSizeDropEdit.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.CountCalcDropEdit.SuspendLayout();
			this.ImportReasonCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.TCPGAHeader);
			// 
			// CommodityGroupBox
			// 
			this.CommodityGroupBox.Controls.Add(this.USImporterDeclarationStateDescLabel);
			this.CommodityGroupBox.Controls.Add(this.USImporterDeclarationCheckBox);
			this.CommodityGroupBox.Controls.Add(this.ImporterDeclarationStateDescLabel);
			this.CommodityGroupBox.Controls.Add(this.TireClassDropEdit);
			this.CommodityGroupBox.Controls.Add(this.TireTypeDropEdit);
			this.CommodityGroupBox.Controls.Add(this.TireSizeDropEdit);
			this.CommodityGroupBox.Controls.Add(this.ImporterDeclarationCheckBox);
			this.CommodityGroupBox.Controls.Add(this.ManufacturerAddressControl);
			this.CommodityGroupBox.Controls.Add(this.CountCalcDropEdit);
			this.CommodityGroupBox.Controls.Add(this.ImportReasonCodeDropEdit);
			this.CommodityGroupBox.Controls.Add(this.MakeTextBox);
			this.CommodityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityGroupBox.Name = "CommodityGroupBox";
			this.CommodityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 283, true);
			this.CommodityGroupBox.TabIndex = 1;
			this.CommodityGroupBox.TabStop = false;
			// 
			// USImporterDeclarationStateDescLabel
			// 
			this.BindingSource.SetBindingMember(this.USImporterDeclarationStateDescLabel, "USImporterDeclarationStateDescForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).USImporterDeclarationStateDescForBinding)));
			this.USImporterDeclarationStateDescLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.USImporterDeclarationStateDescLabel.IsFontBold = true;
			this.USImporterDeclarationStateDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 211, true);
			this.USImporterDeclarationStateDescLabel.Name = "USImporterDeclarationStateDescLabel";
			this.USImporterDeclarationStateDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 50, true);
			this.USImporterDeclarationStateDescLabel.TabIndex = 5;
			this.USImporterDeclarationStateDescLabel.Text = "State\r";
			this.USImporterDeclarationStateDescLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// USImporterDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.USImporterDeclarationCheckBox, "IsUSImporterDeclared");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).IsUSImporterDeclared)));
			this.USImporterDeclarationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4699b374-e718-4f99-9ea8-7b2dfa53443a", "Declaration");
			this.USImporterDeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.USImporterDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 185, true);
			this.USImporterDeclarationCheckBox.Name = "USImporterDeclarationCheckBox";
			this.USImporterDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 23, true);
			this.USImporterDeclarationCheckBox.TabIndex = 21;
			this.USImporterDeclarationCheckBox.UseVisualStyleBackColor = true;
			// 
			// ImporterDeclarationStateDescLabel
			// 
			this.BindingSource.SetBindingMember(this.ImporterDeclarationStateDescLabel, "ImporterDeclarationStateDescForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).ImporterDeclarationStateDescForBinding)));
			this.ImporterDeclarationStateDescLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("29d7a822-5453-4c27-90fa-adad8df6c903", "State");
			this.ImporterDeclarationStateDescLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ImporterDeclarationStateDescLabel.IsFontBold = true;
			this.ImporterDeclarationStateDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 124, true);
			this.ImporterDeclarationStateDescLabel.Name = "ImporterDeclarationStateDescLabel";
			this.ImporterDeclarationStateDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 50, true);
			this.ImporterDeclarationStateDescLabel.TabIndex = 20;
			this.ImporterDeclarationStateDescLabel.Text = "State\r";
			this.ImporterDeclarationStateDescLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// TireClassDropEdit
			// 
			this.TireClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TireClassDropEdit, "CA_ProductClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ProductClass)));
			this.TireClassDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8438806a-d22d-4e8a-a2da-8db0c98e16d3", "Tire Class");
			this.TireClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 19, true);
			this.TireClassDropEdit.Name = "TireClassDropEdit";
			this.TireClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.TireClassDropEdit.TabIndex = 1;
			// 
			// TireTypeDropEdit
			// 
			this.TireTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TireTypeDropEdit, "CA_ProductType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ProductType)));
			this.TireTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("326113bf-7f14-416c-b2f0-9f70f0425e1d", "Tire Type");
			this.TireTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 45, true);
			this.TireTypeDropEdit.Name = "TireTypeDropEdit";
			this.TireTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.TireTypeDropEdit.TabIndex = 2;
			// 
			// TireSizeDropEdit
			// 
			this.TireSizeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TireSizeDropEdit, "CA_ProductSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ProductSize)));
			this.TireSizeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4cd25629-09b2-4f89-8347-3e5647c66ca9", "Tire Size");
			this.TireSizeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 71, true);
			this.TireSizeDropEdit.Name = "TireSizeDropEdit";
			this.TireSizeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.TireSizeDropEdit.TabIndex = 3;
			// 
			// ImporterDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterDeclarationCheckBox, "IsZZImporterDeclared");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).IsZZImporterDeclared)));
			this.ImporterDeclarationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2dae6cd8-47fc-4166-97cf-c307e279c00a", "Declaration");
			this.ImporterDeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImporterDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 97, true);
			this.ImporterDeclarationCheckBox.Name = "ImporterDeclarationCheckBox";
			this.ImporterDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 23, true);
			this.ImporterDeclarationCheckBox.TabIndex = 4;
			this.ImporterDeclarationCheckBox.UseVisualStyleBackColor = true;
			// 
			// ManufacturerAddressControl
			// 
			this.BindingSource.SetBindingMember(ManufacturerAddressControl, "OA_Manufacturer");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).OA_Manufacturer)));
			this.ManufacturerAddressControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.ManufacturerAddressControl.AllowDrop = true;
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1761c45a-ba31-4053-9cd4-74bff9f63b9c", "Manuf.", "Manufacturer", "");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 97, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ManufacturerAddressControl.TabIndex = 9;
			// 
			// CountCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(CountCalcDropEdit, ".");
			this.CountCalcDropEdit.BindToAmount = "InvoiceLine.JI_InvoiceQuantity";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).InvoiceLine.JI_InvoiceQuantity)));
			this.CountCalcDropEdit.BindToUnit = "InvoiceLine.JI_InvoiceUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).InvoiceLine.JI_InvoiceUQ)));
			this.CountCalcDropEdit.AllowDrop = true;
			this.CountCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4998d6a8-43ee-4ea8-9992-262a84434e4a", "Count");
			this.CountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 19, true);
			this.CountCalcDropEdit.Name = "CountCalcDropEdit";
			this.CountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.CountCalcDropEdit.TabIndex = 6;
			// 
			// ImportReasonCodeDropEdit
			// 
			this.ImportReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportReasonCodeDropEdit, "CA_ImportReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ImportReasonCode)));
			this.ImportReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("68f389a1-003f-4eca-9183-53ab950a0f98", "Import Reason");
			this.ImportReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 45, true);
			this.ImportReasonCodeDropEdit.Name = "ImportReasonCodeDropEdit";
			this.ImportReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.ImportReasonCodeDropEdit.TabIndex = 7;
			// 
			// MakeTextBox
			// 
			this.BindingSource.SetBindingMember(MakeTextBox, "JI_BrandName");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).JI_BrandName)));
			this.MakeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2bcdaa3e-76a9-401d-92be-fb02db572b7e", "Make");
			this.MakeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 71, true);
			this.MakeTextBox.Name = "MakeTextBox";
			this.MakeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.MakeTextBox.TabIndex = 8;
			// 
			// TCTPRUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommodityGroupBox);
			this.Name = "TCTPRUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommodityGroupBox.ResumeLayout(false);
			this.CommodityGroupBox.PerformLayout();
			this.TireClassDropEdit.ResumeLayout(true);
			this.TireClassDropEdit.PerformLayout();
			this.TireTypeDropEdit.ResumeLayout(true);
			this.TireTypeDropEdit.PerformLayout();
			this.TireSizeDropEdit.ResumeLayout(true);
			this.TireSizeDropEdit.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.CountCalcDropEdit.ResumeLayout(true);
			this.CountCalcDropEdit.PerformLayout();
			this.ImportReasonCodeDropEdit.ResumeLayout(true);
			this.ImportReasonCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZDropEdit TireClassDropEdit;
		private ZArchitecture.GUI.ZGroupBox CommodityGroupBox;
		private ZArchitecture.GUI.ZCheckBox ImporterDeclarationCheckBox;
		private ZArchitecture.ZTextBox MakeTextBox;
		private ZArchitecture.GUI.ZDropEdit TireTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit TireSizeDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit CountCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit ImportReasonCodeDropEdit;
		private ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
		private ZArchitecture.ZLabel ImporterDeclarationStateDescLabel;
		private ZArchitecture.GUI.ZCheckBox USImporterDeclarationCheckBox;
		private ZArchitecture.ZLabel USImporterDeclarationStateDescLabel;
	}
}
