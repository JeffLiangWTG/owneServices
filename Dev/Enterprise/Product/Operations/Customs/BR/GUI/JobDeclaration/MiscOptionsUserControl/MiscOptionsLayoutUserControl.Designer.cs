namespace Enterprise.Customs.BR.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PaymentSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.BankAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentSeparatorUserControl.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// PaymentSeparatorUserControl
			// 
			this.PaymentSeparatorUserControl.AllowDrop = true;
			this.PaymentSeparatorUserControl.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9a7cee2f-76b5-418f-be09-25393d7ed1fc", "Payment");
			this.PaymentSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 90, true);
			this.PaymentSeparatorUserControl.Name = "PaymentSeparatorUserControl";
			this.PaymentSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.PaymentSeparatorUserControl.TabIndex = 23;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "PaymentBankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).PaymentBankAccountPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).Lookups.BankAccounts)));
			this.BankAccountGuidFindBox.BindToList = "Lookups.BankAccounts";
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 111, true);
			this.BankAccountGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccBankAccount;
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 24;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BankAccountGuidFindBox);
			this.Controls.Add(this.PaymentSeparatorUserControl);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentSeparatorUserControl.ResumeLayout(true);
			this.PaymentSeparatorUserControl.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.SeparatorUserControl PaymentSeparatorUserControl;
		internal ZArchitecture.GUI.ZGuidFindBox BankAccountGuidFindBox;
	}
}
