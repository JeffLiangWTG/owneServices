namespace Enterprise.Customs.IT.GUI
{
	partial class InvoiceLineDetailsUserControl
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
			this.PortTaxRateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginCountryStateUserControl = new Enterprise.Customs.IT.GUI.OriginCountryStateUserControl();
			this.VatTypeAndDescriptionUserControl = new Enterprise.Customs.IT.GUI.VatTypeAndDescriptionUserControl();
			this.CountryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortTaxRateDropEdit.SuspendLayout();
			this.GoodsOriginDropEdit.SuspendLayout();
			this.OriginCountryStateUserControl.SuspendLayout();
			this.VatTypeAndDescriptionUserControl.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.CountryOfExportDropEdit.SuspendLayout();
			this.InvoiceNumberDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine);
			// 
			// PortTaxRateDropEdit
			// 
			this.PortTaxRateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortTaxRateDropEdit, "ZG_PortTaxRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).ZG_PortTaxRate)));
			this.PortTaxRateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 14, true);
			this.PortTaxRateDropEdit.Name = "PortTaxRateDropEdit";
			this.PortTaxRateDropEdit.PreBoundMaxLength = 2;
			this.PortTaxRateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.PortTaxRateDropEdit.TabIndex = 28;
			// 
			// GoodsOriginDropEdit
			// 
			this.GoodsOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginDropEdit, "JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).JI_CountryOfOrigin)));
			this.GoodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 94, true);
			this.GoodsOriginDropEdit.Name = "GoodsOriginDropEdit";
			this.GoodsOriginDropEdit.PreBoundMaxLength = 3;
			this.GoodsOriginDropEdit.ShouldResizeByMaxLength = false;
			this.GoodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.GoodsOriginDropEdit.TabIndex = 34;
			// 
			// OriginCountryStateUserControl
			// 
			this.OriginCountryStateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCountryStateUserControl, ".");
			this.OriginCountryStateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 40, true);
			this.OriginCountryStateUserControl.Name = "OriginCountryStateUserControl";
			this.OriginCountryStateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 21, true);
			this.OriginCountryStateUserControl.TabIndex = 35;
			// 
			// VatTypeAndDescriptionUserControl
			// 
			this.VatTypeAndDescriptionUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VatTypeAndDescriptionUserControl, ".");
			this.VatTypeAndDescriptionUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 67, true);
			this.VatTypeAndDescriptionUserControl.Name = "VatTypeAndDescriptionUserControl";
			this.VatTypeAndDescriptionUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 21, true);
			this.VatTypeAndDescriptionUserControl.TabIndex = 36;
			// 
			// CountryOfDestinationDropEdit
			// 
			this.CountryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationDropEdit, "ZG_CountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfDestination)));
			this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 120, true);
			this.CountryOfDestinationDropEdit.Name = "CountryOfDestinationDropEdit";
			this.CountryOfDestinationDropEdit.PreBoundMaxLength = 3;
			this.CountryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.CountryOfDestinationDropEdit.TabIndex = 29;
			// 
			// CountryOfExportDropEdit
			// 
			this.CountryOfExportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfExportDropEdit, "JI_RN_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).JI_RN_NKCountryOfExport)));
			this.CountryOfExportDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("2a1f3821-d0bf-4d16-8e2b-3d9cb76e570d", "Country of Export", "Country of Export of the goods being moved.");
			this.CountryOfExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 146, true);
			this.CountryOfExportDropEdit.Name = "CountryOfExportDropEdit";
			this.CountryOfExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.CountryOfExportDropEdit.TabIndex = 30;
			//
			// InvoiceNumberDropEdit
			//
			this.InvoiceNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNumberDropEdit, "JI_Calc_Invoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).JI_Calc_Invoice)));
			this.InvoiceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 172, true);
			this.InvoiceNumberDropEdit.Name = "InvoiceNumberDropEdit";
			this.InvoiceNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.InvoiceNumberDropEdit.TabIndex = 31;
			this.InvoiceNumberDropEdit.ShowDescriptionBox = false;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryOfDestinationDropEdit);
			this.Controls.Add(this.CountryOfExportDropEdit);
			this.Controls.Add(this.VatTypeAndDescriptionUserControl);
			this.Controls.Add(this.OriginCountryStateUserControl);
			this.Controls.Add(this.GoodsOriginDropEdit);
			this.Controls.Add(this.PortTaxRateDropEdit);
			this.Controls.Add(this.InvoiceNumberDropEdit);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 218, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortTaxRateDropEdit.ResumeLayout(true);
			this.PortTaxRateDropEdit.PerformLayout();
			this.GoodsOriginDropEdit.ResumeLayout(true);
			this.GoodsOriginDropEdit.PerformLayout();
			this.OriginCountryStateUserControl.ResumeLayout(true);
			this.OriginCountryStateUserControl.PerformLayout();
			this.VatTypeAndDescriptionUserControl.ResumeLayout(true);
			this.VatTypeAndDescriptionUserControl.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.CountryOfExportDropEdit.ResumeLayout(true);
			this.CountryOfExportDropEdit.PerformLayout();
			this.InvoiceNumberDropEdit.ResumeLayout(true);
			this.InvoiceNumberDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit PortTaxRateDropEdit;
		internal ZArchitecture.GUI.ZDropEdit GoodsOriginDropEdit;
		internal OriginCountryStateUserControl OriginCountryStateUserControl;
		internal VatTypeAndDescriptionUserControl VatTypeAndDescriptionUserControl;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDestinationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfExportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit InvoiceNumberDropEdit;
	}
}
