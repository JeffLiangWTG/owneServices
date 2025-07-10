namespace Enterprise.Customs.IN.GUI;

partial class CommercialInvoiceDetailsLayoutUserControl
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
			this.ExporterContractNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PaymentDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AuthorizedEconomicOperatorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AuthorizedEconomicOperatorCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizedEconomicOperatorCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizedEconomicOperatorRoleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IGSTPaymentStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorizedEconomicOperatorGuidFindBox.SuspendLayout();
			this.IGSTPaymentStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobComInvoiceHeader);
			// 
			// ExporterContractNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterContractNumberTextBox, "JZ_ExporterContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).JZ_ExporterContractNumber)));
			this.ExporterContractNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExporterContractNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 22, true);
			this.ExporterContractNumberTextBox.Name = "ExporterContractNumberTextBox";
			this.ExporterContractNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 38, true);
			this.ExporterContractNumberTextBox.TabIndex = 7;
			// 
			// PaymentDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentDaysCalcEdit, "JZ_PaymentDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).JZ_PaymentDays)));
			this.PaymentDaysCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PaymentDaysCalcEdit.DecimalPlaces = 2;
			this.PaymentDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 66, true);
			this.PaymentDaysCalcEdit.MaxValue = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.PaymentDaysCalcEdit.Name = "PaymentDaysCalcEdit";
			this.PaymentDaysCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.PaymentDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 38, true);
			this.PaymentDaysCalcEdit.TabIndex = 7;
			this.PaymentDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaymentDaysCalcEdit.TrackDisposedAccess = true;
			// 
			// AuthorizedEconomicOperatorGuidFindBox
			// 
			this.AuthorizedEconomicOperatorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizedEconomicOperatorGuidFindBox, "AuthorizedEconomicOperatorOrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).AuthorizedEconomicOperatorOrgPK)));
			this.AuthorizedEconomicOperatorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 110, true);
			this.AuthorizedEconomicOperatorGuidFindBox.Name = "AuthorizedEconomicOperatorGuidFindBox";
			this.AuthorizedEconomicOperatorGuidFindBox.ParentType = null;
			this.AuthorizedEconomicOperatorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 38, true);
			this.AuthorizedEconomicOperatorGuidFindBox.TabIndex = 3;
			// 
			// AuthorizedEconomicOperatorCountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorizedEconomicOperatorCountryTextBox, "AuthorizedEconomicOperatorCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).AuthorizedEconomicOperatorCountry)));
			this.AuthorizedEconomicOperatorCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 154, true);
			this.AuthorizedEconomicOperatorCountryTextBox.Name = "AuthorizedEconomicOperatorCountryTextBox";
			this.AuthorizedEconomicOperatorCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 38, true);
			this.AuthorizedEconomicOperatorCountryTextBox.TabIndex = 1;
			// 
			// AuthorizedEconomicOperatorCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorizedEconomicOperatorCodeTextBox, "AuthorizedEconomicOperatorCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).AuthorizedEconomicOperatorCode)));
			this.AuthorizedEconomicOperatorCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthorizedEconomicOperatorCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 198, true);
			this.AuthorizedEconomicOperatorCodeTextBox.Name = "AuthorizedEconomicOperatorCodeTextBox";
			this.AuthorizedEconomicOperatorCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 38, true);
			this.AuthorizedEconomicOperatorCodeTextBox.TabIndex = 8;
			// 
			// AuthorizedEconomicOperatorRoleTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorizedEconomicOperatorRoleTextBox, "JZ_AuthorizedEconomicOperatorRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).JZ_AuthorizedEconomicOperatorRole)));
			this.AuthorizedEconomicOperatorRoleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 242, true);
			this.AuthorizedEconomicOperatorRoleTextBox.Name = "AuthorizedEconomicOperatorRoleTextBox";
			this.AuthorizedEconomicOperatorRoleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 38, true);
			this.AuthorizedEconomicOperatorRoleTextBox.TabIndex = 9;
			// 
			// IGSTPaymentStatusDropEdit
			//
			this.BindingSource.SetBindingMember(this.IGSTPaymentStatusDropEdit, "JZ_GSTPaymentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(null)).JZ_GSTPaymentStatus)));
			this.IGSTPaymentStatusDropEdit.AllowDrop = true;
			this.IGSTPaymentStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 123, true);
			this.IGSTPaymentStatusDropEdit.Name = "IGSTPaymentStatusDropEdit";
			this.IGSTPaymentStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 15, true);
			this.IGSTPaymentStatusDropEdit.TabIndex = 8;
			this.IGSTPaymentStatusDropEdit.ShowDescriptionBox = false;
			// 
			// CommercialInvoiceDetailsLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IGSTPaymentStatusDropEdit);
			this.Controls.Add(this.ExporterContractNumberTextBox);
			this.Controls.Add(this.PaymentDaysCalcEdit);
			this.Controls.Add(this.AuthorizedEconomicOperatorGuidFindBox);
			this.Controls.Add(this.AuthorizedEconomicOperatorCountryTextBox);
			this.Controls.Add(this.AuthorizedEconomicOperatorCodeTextBox);
			this.Controls.Add(this.AuthorizedEconomicOperatorRoleTextBox);
			this.Name = "CommercialInvoiceDetailsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 347, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorizedEconomicOperatorGuidFindBox.ResumeLayout(true);
			this.AuthorizedEconomicOperatorGuidFindBox.PerformLayout();
			this.IGSTPaymentStatusDropEdit.ResumeLayout(true);
			this.IGSTPaymentStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZTextBox ExporterContractNumberTextBox;
	internal Enterprise.ZArchitecture.ZCalcEdit PaymentDaysCalcEdit;
	internal Enterprise.ZArchitecture.GUI.ZGuidFindBox AuthorizedEconomicOperatorGuidFindBox;
	internal ZArchitecture.ZTextBox AuthorizedEconomicOperatorCountryTextBox;
	internal ZArchitecture.ZTextBox AuthorizedEconomicOperatorCodeTextBox;
	internal ZArchitecture.ZTextBox AuthorizedEconomicOperatorRoleTextBox;
	internal ZArchitecture.GUI.ZDropEdit IGSTPaymentStatusDropEdit;
}

