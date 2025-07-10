namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7BillPartiesUserControl
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
			this.ImporterSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ExporterSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.SellerSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.IdentificationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterIdentificationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfImportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfSellerDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterSeparatorUserControl.SuspendLayout();
			this.ExporterSeparatorUserControl.SuspendLayout();
			this.SellerSeparatorUserControl.SuspendLayout();
			this.IdentificationNoTextBox.SuspendLayout();
			this.ImporterIdentificationTypeDropEdit.SuspendLayout();
			this.CountryOfImportDropEdit.SuspendLayout();
			this.CountryOfExportDropEdit.SuspendLayout();
			this.CountryOfSellerDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBill);
			// 
			// ImporterSeparatorUserControl
			// 
			this.ImporterSeparatorUserControl.AllowDrop = true;
			this.ImporterSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("7c655f66-0713-4bef-a119-63e078ac877c", "Importer");
			this.ImporterSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 0, true);
			this.ImporterSeparatorUserControl.Name = "ImporterSeparatorUserControl";
			this.ImporterSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.ImporterSeparatorUserControl.TabIndex = 0;
			// 
			// ExporterSeparatorUserControl
			// 
			this.ExporterSeparatorUserControl.AllowDrop = true;
			this.ExporterSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("f9256fac-7a7a-4ea0-9332-0bff9d129bd8", "Exporter");
			this.ExporterSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 4, true);
			this.ExporterSeparatorUserControl.Name = "ExporterSeparatorUserControl";
			this.ExporterSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.ExporterSeparatorUserControl.TabIndex = 1;
			//
			// SellerSeparatorUserControl
			// 
			this.SellerSeparatorUserControl.AllowDrop = true;
			this.SellerSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("ed6afb90-aeb8-4fc7-a1db-8efde69c126b", "Seller");
			this.SellerSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 4, true);
			this.SellerSeparatorUserControl.Name = "SellerSeparatorUserControl";
			this.SellerSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.SellerSeparatorUserControl.TabIndex = 2;
			// 
			// IdentificationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.IdentificationNoTextBox, "ABL_ConsigneeRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_ConsigneeRegNo)));
			this.IdentificationNoTextBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("997d5207-2aef-4a6d-b3fa-da18b472b675", "Identification No.");
			this.IdentificationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 240, true);
			this.IdentificationNoTextBox.Name = "IdentificationNoTextBox";
			this.IdentificationNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.IdentificationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.IdentificationNoTextBox.TabIndex = 3;
			// 
			// ImporterIdentificationTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ImporterIdentificationTypeDropEdit, "ABL_ConsigneeRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_ConsigneeRegNoType)));
			this.ImporterIdentificationTypeDropEdit.CaptionResourceString = Res.GetData("97710E72-A7F4-45D2-B3C4-50D01815ABEA", "ID No. Type");
			this.ImporterIdentificationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 262, true);
			this.ImporterIdentificationTypeDropEdit.Name = "ImporterIdentificationTypeDropEdit";
			this.ImporterIdentificationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.ImporterIdentificationTypeDropEdit.TabIndex = 4;
			// 
			// CountryOfImportDropEdit
			// 
			this.CountryOfImportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfImportDropEdit, "ABL_RN_NKConsigneeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_RN_NKConsigneeCountry)));
			this.CountryOfImportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 39, true);
			this.CountryOfImportDropEdit.Name = "CountryOfImportDropEdit";
			this.CountryOfImportDropEdit.PreBoundMaxLength = 27;
			this.CountryOfImportDropEdit.ShouldResizeByMaxLength = true;
			this.CountryOfImportDropEdit.ShowDescriptionBox = false;
			this.CountryOfImportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CountryOfImportDropEdit.TabIndex = 5;
			// 
			// CountryOfExportDropEdit
			// 
			this.CountryOfExportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfExportDropEdit, "ABL_RN_NKShipperCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_RN_NKShipperCountry)));
			this.CountryOfExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1074, 39, true);
			this.CountryOfExportDropEdit.Name = "CountryOfExportDropEdit";
			this.CountryOfExportDropEdit.PreBoundMaxLength = 27;
			this.CountryOfExportDropEdit.ShouldResizeByMaxLength = true;
			this.CountryOfExportDropEdit.ShowDescriptionBox = false;
			this.CountryOfExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CountryOfExportDropEdit.TabIndex = 6;
			// 
			// CountryOfSellerDropEdit
			// 
			this.CountryOfSellerDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfSellerDropEdit, "ABL_RN_NKSellerCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_RN_NKSellerCountry)));
			this.CountryOfSellerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1698, 39, true);
			this.CountryOfSellerDropEdit.Name = "CountryOfSellerDropEdit";
			this.CountryOfSellerDropEdit.PreBoundMaxLength = 27;
			this.CountryOfSellerDropEdit.ShouldResizeByMaxLength = true;
			this.CountryOfSellerDropEdit.ShowDescriptionBox = false;
			this.CountryOfSellerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CountryOfSellerDropEdit.TabIndex = 7;
			// 
			// EUH7BillPartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ImporterSeparatorUserControl);
			this.Controls.Add(this.ExporterSeparatorUserControl);
			this.Controls.Add(this.SellerSeparatorUserControl);
			this.Controls.Add(this.IdentificationNoTextBox);
			this.Controls.Add(this.ImporterIdentificationTypeDropEdit);
			this.Controls.Add(this.CountryOfImportDropEdit);
			this.Controls.Add(this.CountryOfExportDropEdit);
			this.Controls.Add(this.CountryOfSellerDropEdit);
			this.Name = "EUH7BillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 188, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterSeparatorUserControl.ResumeLayout(true);
			this.ImporterSeparatorUserControl.PerformLayout();
			this.ExporterSeparatorUserControl.ResumeLayout(true);
			this.ExporterSeparatorUserControl.PerformLayout();
			this.SellerSeparatorUserControl.ResumeLayout(true);
			this.SellerSeparatorUserControl.PerformLayout();
			this.IdentificationNoTextBox.ResumeLayout(true);
			this.IdentificationNoTextBox.PerformLayout();
			this.ImporterIdentificationTypeDropEdit.ResumeLayout(true);
			this.ImporterIdentificationTypeDropEdit.PerformLayout();
			this.CountryOfImportDropEdit.ResumeLayout(true);
			this.CountryOfImportDropEdit.PerformLayout();
			this.CountryOfExportDropEdit.ResumeLayout(true);
			this.CountryOfExportDropEdit.PerformLayout();
			this.CountryOfSellerDropEdit.ResumeLayout(true);
			this.CountryOfSellerDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.SeparatorUserControl ImporterSeparatorUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl ExporterSeparatorUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl SellerSeparatorUserControl;
		internal Enterprise.ZArchitecture.ZTextBox IdentificationNoTextBox;
		internal ZArchitecture.GUI.ZDropEdit ImporterIdentificationTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfImportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfExportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfSellerDropEdit;
	}
}
