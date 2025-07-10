
namespace Enterprise.Customs.ES.GUI
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
			this.CommercialReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.T2LItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GlobalWarmingPotentialCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CountryOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RegionOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MethodOfPayment2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VATIGICTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AIEMTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExciseExemptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExciseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PVPCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.REAProductCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.READirectConsumptionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HasNonRecycledPlasticsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfDestinationCodeFindBox.SuspendLayout();
			this.MethodOfPaymentDropEdit.SuspendLayout();
			this.MethodOfPayment2DropEdit.SuspendLayout();
			this.VATIGICTypeDropEdit.SuspendLayout();
			this.AIEMTypeDropEdit.SuspendLayout();
			this.ExciseExemptionDropEdit.SuspendLayout();
			this.ExciseCodeDropEdit.SuspendLayout();
			this.PVPCalcFindBox.SuspendLayout();
			this.REAProductCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine);
			// 
			// CommercialReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommercialReferenceTextBox, "ZG_CommercialReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_CommercialReference)));
			this.CommercialReferenceTextBox.CaptionResourceString = null;
			this.CommercialReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommercialReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 3, true);
			this.CommercialReferenceTextBox.Name = "CommercialReferenceTextBox";
			this.CommercialReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 20, true);
			this.CommercialReferenceTextBox.TabIndex = 1;
			// 
			// T2LItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.T2LItemNumberCalcEdit, "ZG_T2LItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_T2LItemNumber)));
			this.T2LItemNumberCalcEdit.CaptionResourceString = null;
			this.T2LItemNumberCalcEdit.DecimalPlaces = 0;
			this.T2LItemNumberCalcEdit.Decimals = 0;
			this.T2LItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 51, true);
			this.T2LItemNumberCalcEdit.MaxValue = new decimal(new int[] { 999, 0, 0, 0 });
			this.T2LItemNumberCalcEdit.Name = "T2LItemNumberCalcEdit";
			this.T2LItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.T2LItemNumberCalcEdit.TabIndex = 15;
			this.T2LItemNumberCalcEdit.Text = "0";
			this.T2LItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GlobalWarmingPotentialCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GlobalWarmingPotentialCalcEdit, "ZG_GlobalWarmingPotential");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_GlobalWarmingPotential)));
			this.GlobalWarmingPotentialCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 80, true);
			this.GlobalWarmingPotentialCalcEdit.Name = "GlobalWarmingPotentialCalcEdit";
			this.GlobalWarmingPotentialCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.GlobalWarmingPotentialCalcEdit.TabIndex = 15;
			this.GlobalWarmingPotentialCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CountryOfDestinationCodeFindBox
			// 
			this.CountryOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationCodeFindBox, "ZG_CountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfDestination)));
			this.CountryOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 106, true);
			this.CountryOfDestinationCodeFindBox.Name = "CountryOfDestinationCodeFindBox";
			this.CountryOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryOfDestinationCodeFindBox.ParentType = null;
			this.CountryOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.CountryOfDestinationCodeFindBox.TabIndex = 16;
			// 
			// RegionOfDestinationCodeFindBox
			// 
			this.RegionOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegionOfDestinationCodeFindBox, "ZG_RegionOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_RegionOfDestination)));
			this.RegionOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 132, true);
			this.RegionOfDestinationCodeFindBox.Name = "RegionOfDestinationCodeFindBox";
			this.RegionOfDestinationCodeFindBox.PreBoundMaxLength = 1;
			this.RegionOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 15, true);
			this.RegionOfDestinationCodeFindBox.TabIndex = 17;
			// 
			// MethodOfPaymentDropEdit
			// 
			this.MethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodOfPaymentDropEdit, "ZG_MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_MethodOfPayment)));
			this.MethodOfPaymentDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("E7C316A0-5CFE-4202-8311-7B607BFCE002", "Method Of Payment");
			this.MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 132, true);
			this.MethodOfPaymentDropEdit.Name = "MethodOfPaymentDropEdit";
			this.MethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.MethodOfPaymentDropEdit.TabIndex = 18;
			// 
			// MethodOfPayment2DropEdit
			// 
			this.MethodOfPayment2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodOfPayment2DropEdit, "ZG_MethodOfPayment2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_MethodOfPayment2)));
			this.MethodOfPayment2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 158, true);
			this.MethodOfPayment2DropEdit.Name = "MethodOfPayment2DropEdit";
			this.MethodOfPayment2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.MethodOfPayment2DropEdit.TabIndex = 19;
			// 
			// VATIGICTypeDropEdit
			// 
			this.VATIGICTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATIGICTypeDropEdit, "JI_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).JI_ZZF_NKTaxType)));
			this.VATIGICTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 184, true);
			this.VATIGICTypeDropEdit.Name = "VATIGICTypeDropEdit";
			this.VATIGICTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.VATIGICTypeDropEdit.TabIndex = 20;
			// 
			// AIEMTypeDropEdit
			// 
			this.AIEMTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIEMTypeDropEdit, "ZG_AIEMType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_AIEMType)));
			this.AIEMTypeDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("502A281C-35C6-4097-8542-F67F4DE3B28E", "AIEM Type");
			this.AIEMTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 210, true);
			this.AIEMTypeDropEdit.Name = "AIEMTypeDropEdit";
			this.AIEMTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AIEMTypeDropEdit.TabIndex = 21;
			// 
			// ExciseExemptionDropEdit
			// 
			this.ExciseExemptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExciseExemptionDropEdit, "ZG_ExciseExemption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExciseExemption)));
			this.ExciseExemptionDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A1CB7D48-39C8-446C-83CA-57961E00E7F7", "Excise Exemption");
			this.ExciseExemptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 80, true);
			this.ExciseExemptionDropEdit.Name = "ExciseExemptionDropEdit";
			this.ExciseExemptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.ExciseExemptionDropEdit.TabIndex = 22;
			// 
			// ExciseCodeDropEdit
			// 
			this.ExciseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExciseCodeDropEdit, "ZG_ExciseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExciseCode)));
			this.ExciseCodeDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("DA8EF288-534E-467A-A5E9-5A0CB5337A41", "Excise Code");
			this.ExciseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 3, true);
			this.ExciseCodeDropEdit.Name = "ExciseCodeDropEdit";
			this.ExciseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ExciseCodeDropEdit.TabIndex = 23;
			// 
			// PVPCalcFindBox
			// 
			this.PVPCalcFindBox.AllowDrop = true;
			this.PVPCalcFindBox.BindToAmount = "ZG_TotalRetailPrice";
			this.PVPCalcFindBox.BindToUnit = "TotalRetailPriceCurrency";
			this.PVPCalcFindBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("16D49863-48D4-4A14-B44B-3E091F4FB68D", "PVP");
			this.PVPCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PVPCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 80, true);
			this.PVPCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.PVPCalcFindBox.Name = "PVPCalcFindBox";
			this.PVPCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.PVPCalcFindBox.TabIndex = 24;
			// 
			// REAProductCodeDropEdit
			// 
			this.REAProductCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REAProductCodeDropEdit, "ZG_REAProductCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_REAProductCode)));
			this.REAProductCodeDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A7B665A3-70F8-478E-9AA9-165D78003546", "[C33] REA Product Code");
			this.REAProductCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 132, true);
			this.REAProductCodeDropEdit.Name = "REAProductCodeDropEdit";
			this.REAProductCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.REAProductCodeDropEdit.TabIndex = 25;
			// 
			// READirectConsumptionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.READirectConsumptionCheckBox, "ZG_IsREADirectConsumption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_IsREADirectConsumption)));
			this.READirectConsumptionCheckBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("7D593A9E-B9E5-440B-8C17-B31438740ED7", "REA Direct Consumption");
			this.READirectConsumptionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 158, true);
			this.READirectConsumptionCheckBox.Name = "READirectConsumptionCheckBox";
			this.READirectConsumptionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 24, true);
			this.READirectConsumptionCheckBox.TabIndex = 26;
			// 
			// HasNonRecycledPlasticsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HasNonRecycledPlasticsCheckBox, "ZG_HasNonRecycledPlastics");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(null)).ZG_HasNonRecycledPlastics)));
			this.HasNonRecycledPlasticsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 184, true);
			this.HasNonRecycledPlasticsCheckBox.Name = "HasNonRecycledPlasticsCheckBox";
			this.HasNonRecycledPlasticsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.HasNonRecycledPlasticsCheckBox.TabIndex = 27;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommercialReferenceTextBox);
			this.Controls.Add(this.T2LItemNumberCalcEdit);
			this.Controls.Add(this.GlobalWarmingPotentialCalcEdit);
			this.Controls.Add(this.CountryOfDestinationCodeFindBox);
			this.Controls.Add(this.RegionOfDestinationCodeFindBox);
			this.Controls.Add(this.MethodOfPaymentDropEdit);
			this.Controls.Add(this.MethodOfPayment2DropEdit);
			this.Controls.Add(this.VATIGICTypeDropEdit);
			this.Controls.Add(this.AIEMTypeDropEdit);
			this.Controls.Add(this.ExciseExemptionDropEdit);
			this.Controls.Add(this.ExciseCodeDropEdit);
			this.Controls.Add(this.PVPCalcFindBox);
			this.Controls.Add(this.REAProductCodeDropEdit);
			this.Controls.Add(this.READirectConsumptionCheckBox);
			this.Controls.Add(this.HasNonRecycledPlasticsCheckBox);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfDestinationCodeFindBox.ResumeLayout(true);
			this.CountryOfDestinationCodeFindBox.PerformLayout();
			this.RegionOfDestinationCodeFindBox.ResumeLayout(true);
			this.RegionOfDestinationCodeFindBox.PerformLayout();
			this.MethodOfPaymentDropEdit.ResumeLayout(true);
			this.MethodOfPaymentDropEdit.PerformLayout();
			this.MethodOfPayment2DropEdit.ResumeLayout(true);
			this.MethodOfPayment2DropEdit.PerformLayout();
			this.VATIGICTypeDropEdit.ResumeLayout(true);
			this.VATIGICTypeDropEdit.PerformLayout();
			this.AIEMTypeDropEdit.ResumeLayout(true);
			this.AIEMTypeDropEdit.PerformLayout();
			this.ExciseExemptionDropEdit.ResumeLayout(true);
			this.ExciseExemptionDropEdit.PerformLayout();
			this.ExciseCodeDropEdit.ResumeLayout(true);
			this.ExciseCodeDropEdit.PerformLayout();
			this.PVPCalcFindBox.ResumeLayout(true);
			this.PVPCalcFindBox.PerformLayout();
			this.REAProductCodeDropEdit.ResumeLayout(true);
			this.REAProductCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox CommercialReferenceTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit T2LItemNumberCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfDestinationCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox RegionOfDestinationCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MethodOfPaymentDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MethodOfPayment2DropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit VATIGICTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit AIEMTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ExciseExemptionDropEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit GlobalWarmingPotentialCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ExciseCodeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcFindBox PVPCalcFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit REAProductCodeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox READirectConsumptionCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox HasNonRecycledPlasticsCheckBox;

		#endregion
	}
}
