using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI;

partial class CommercialInvoiceDetailsUserControl
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
		this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.ValuationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.TotNoOfInvPagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.AttestationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.InvoiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.PaymentMethodDropEdit.SuspendLayout();
		this.ValuationCodeDropEdit.SuspendLayout();
		this.TotNoOfInvPagesCalcEdit.SuspendLayout();
		this.AttestationNoTextBox.SuspendLayout();
		this.InvoiceTypeDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(JobComInvoiceHeader);
		// 
		// PaymentMethodDropEdit
		// 
		this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "JZ_PaymentMethod");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.JobComInvoiceHeader)(null)).JZ_PaymentMethod)));
		this.PaymentMethodDropEdit.AllowDrop = true;
		this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 28, true);
		this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
		this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
		this.PaymentMethodDropEdit.TabIndex = 0;
		// 
		// TotNoOfInvPagesCalcEdit
		// 
		this.TotNoOfInvPagesCalcEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.TotNoOfInvPagesCalcEdit, "JZ_TotNoOfInvPages");
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobComInvoiceHeader)(null)).JZ_TotNoOfInvPages)));
		this.TotNoOfInvPagesCalcEdit.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("7FAB23AF-D0F8-47AE-AC1B-11002040AAF5", "Total No Of Pages");
		this.TotNoOfInvPagesCalcEdit.Name = "TotNoOfInvPagesCalcEdit";
		// 
		// ValuationCodeDropEdit
		// 
		this.BindingSource.SetBindingMember(this.ValuationCodeDropEdit, "JZ_ValuationCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.JobComInvoiceHeader)(null)).JZ_ValuationCode)));
		this.ValuationCodeDropEdit.AllowDrop = true;
		this.ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 61, true);
		this.ValuationCodeDropEdit.Name = "ValuationCodeDropEdit";
		this.ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
		this.ValuationCodeDropEdit.TabIndex = 1;
		// 
		// InvoiceTypeDropEdit
		//
		this.InvoiceTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.InvoiceTypeDropEdit, "JZ_InvoiceType");
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobComInvoiceHeader)(null)).JZ_InvoiceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobComInvoiceHeader)(null)).Lookups.InvoiceTypeList)));
		this.InvoiceTypeDropEdit.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("265122AD-0852-43B9-9E47-D73A05FA4CB4", "Invoice Type");
		this.InvoiceTypeDropEdit.Name = "InvoiceTypeDropEdit";
		// 
		// AttestationNoTextBox
		// 
		this.AttestationNoTextBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.AttestationNoTextBox, "JZ_AttestationNo");
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobComInvoiceHeader)(null)).JZ_AttestationNo)));
		this.AttestationNoTextBox.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("0B822702-0C30-4D56-964F-71C00815CAF7", "Attestation No");
		this.AttestationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 4, true);
		this.AttestationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
		this.AttestationNoTextBox.Name = "AttestationNoTextBox";
		// 
		// CommercialInvoiceDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ValuationCodeDropEdit);
		this.Controls.Add(this.PaymentMethodDropEdit);
		this.Controls.Add(this.TotNoOfInvPagesCalcEdit);
		this.Controls.Add(this.AttestationNoTextBox);
		this.Controls.Add(this.InvoiceTypeDropEdit);
		this.Name = "CommercialInvoiceDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 120, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.PaymentMethodDropEdit.ResumeLayout(true);
		this.PaymentMethodDropEdit.PerformLayout();
		this.ValuationCodeDropEdit.ResumeLayout(true);
		this.ValuationCodeDropEdit.PerformLayout();
		this.TotNoOfInvPagesCalcEdit.ResumeLayout(true);
		this.AttestationNoTextBox.ResumeLayout(true);
		this.TotNoOfInvPagesCalcEdit.PerformLayout();
		this.AttestationNoTextBox.PerformLayout();
		this.InvoiceTypeDropEdit.ResumeLayout(true);
		this.InvoiceTypeDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDropEdit PaymentMethodDropEdit;
	internal ZArchitecture.GUI.ZDropEdit ValuationCodeDropEdit;
	internal Enterprise.ZArchitecture.ZTextBox AttestationNoTextBox;
	internal Enterprise.ZArchitecture.ZCalcEdit TotNoOfInvPagesCalcEdit;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceTypeDropEdit;
}
