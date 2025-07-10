namespace Enterprise.Customs.IN.GUI;

partial class InvoiceLinePartiesUserControl
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
			this.ManufacturerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManufacturerGroupBox.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
			// 
			// ManufacturerGroupBox
			// 
			this.ManufacturerGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("7B0019BC-9B3A-4478-A3E7-AE863580063C", "Manufacturer / Producer / Grower");
			this.ManufacturerGroupBox.Controls.Add(this.ManufacturerAddressControl);
			this.ManufacturerGroupBox.Controls.Add(this.CodeTextBox);
			this.ManufacturerGroupBox.Controls.Add(this.CodeTypeTextBox);
			this.ManufacturerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 13, true);
			this.ManufacturerGroupBox.Name = "ManufacturerGroupBox";
			this.ManufacturerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 107, true);
			this.ManufacturerGroupBox.TabIndex = 0;
			this.ManufacturerGroupBox.TabStop = false;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "FilteredInvoiceLines.JI_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.BindToOrgList = "FilteredInvoiceLines.Lookups.OrganizationList";
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 19, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ManufacturerAddressControl.TabIndex = 1;
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "FilteredInvoiceLines.JI_MPG_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MPG_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 71, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.CodeTextBox.TabIndex = 3;
			// 
			// CodeTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTypeTextBox, "FilteredInvoiceLines.JI_MPG_CodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MPG_CodeType)));
			this.CodeTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 45, true);
			this.CodeTypeTextBox.Name = "CodeTypeTextBox";
			this.CodeTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CodeTypeTextBox.TabIndex = 2;
			// 
			// InvoiceLinePartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManufacturerGroupBox);
			this.Name = "InvoiceLinePartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 138, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManufacturerGroupBox.ResumeLayout(false);
			this.ManufacturerGroupBox.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZGroupBox ManufacturerGroupBox;
	internal ZArchitecture.ZTextBox CodeTextBox;
	internal ZArchitecture.ZTextBox CodeTypeTextBox;
	internal ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
}
