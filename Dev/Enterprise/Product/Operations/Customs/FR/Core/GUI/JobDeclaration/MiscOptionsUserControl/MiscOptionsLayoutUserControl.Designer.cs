using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.FR.GUI
{
	partial class MiscOptionsLayoutUserControl
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
			this.SupportingInformationUserControl = new Enterprise.Customs.FR.GUI.SupportingInformationControl();
			this.VatCanaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefermentAccountNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VATDeferTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VATDeferNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChargePaymentOrDestinationIDsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsGuaranteeNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VatCanaDropEdit.SuspendLayout();
			this.DefermentAccountNumberDropEdit.SuspendLayout();
			this.VATDeferTypeDropEdit.SuspendLayout();
			this.ChargePaymentOrDestinationIDsDropEdit.SuspendLayout();
			this.CustomsGuaranteeNumberDropEdit.SuspendLayout();
			this.PaymentSeparatorUserControl.SuspendLayout();
			this.SupportingInformationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// SupportingInformationUserControl
			// 
			this.SupportingInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
			this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
			this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 484, true);
			this.SupportingInformationUserControl.TabIndex = 0;
			// 
			// VatCanaDropEdit
			// 
			this.VatCanaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VatCanaDropEdit, "ZG_VATCANACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).ZG_VATCANACode)));
			this.VatCanaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 145, true);
			this.VatCanaDropEdit.Name = "VatCanaDropEdit";
			this.VatCanaDropEdit.PreBoundMaxLength = 4;
			this.VatCanaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.VatCanaDropEdit.TabIndex = 6;
			// 
			// DefermentAccountNumberDropEdit
			// 
			this.DefermentAccountNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefermentAccountNumberDropEdit, "JE_DefermentAccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).JE_DefermentAccountNumber)));
			this.DefermentAccountNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 45, true);
			this.DefermentAccountNumberDropEdit.Name = "DefermentAccountNumberDropEdit";
			this.DefermentAccountNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.DefermentAccountNumberDropEdit.TabIndex = 2;
			// 
			// VATDeferTypeDropEdit
			// 
			this.VATDeferTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATDeferTypeDropEdit, "ZG_VATDeferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).ZG_VATDeferType)));
			this.VATDeferTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 97, true);
			this.VATDeferTypeDropEdit.Name = "VATDeferTypeDropEdit";
			this.VATDeferTypeDropEdit.PreBoundMaxLength = 1;
			this.VATDeferTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.VATDeferTypeDropEdit.TabIndex = 4;
			// 
			// JE_VATDeferNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VATDeferNumberTextBox, "ZG_VATDeferNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).ZG_VATDeferNumber)));
			this.VATDeferNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 121, true);
			this.VATDeferNumberTextBox.Name = "VATDeferNumberTextBox";
			this.VATDeferNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.VATDeferNumberTextBox.TabIndex = 5;
			// 
			// ChargePaymentOrDestinationIDsDropEdit
			// 
			this.ChargePaymentOrDestinationIDsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargePaymentOrDestinationIDsDropEdit, "ChargePaymentOrDestinationID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).ChargePaymentOrDestinationID)));
			this.ChargePaymentOrDestinationIDsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 171, true);
			this.ChargePaymentOrDestinationIDsDropEdit.Name = "ChargePaymentOrDestinationIDsDropEdit";
			this.ChargePaymentOrDestinationIDsDropEdit.PreBoundMaxLength = 4;
			this.ChargePaymentOrDestinationIDsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ChargePaymentOrDestinationIDsDropEdit.TabIndex = 7;
			// 
			// CustomsGuaranteeNumberDropEdit
			// 
			this.CustomsGuaranteeNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsGuaranteeNumberDropEdit, "JE_CustomsGuaranteeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).JE_CustomsGuaranteeNumber)));
			this.CustomsGuaranteeNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 71, true);
			this.CustomsGuaranteeNumberDropEdit.Name = "CustomsGuaranteeNumberDropEdit";
			this.CustomsGuaranteeNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.CustomsGuaranteeNumberDropEdit.TabIndex = 3;
			// 
			// PaymentSeparatorUserControl
			// 
			this.PaymentSeparatorUserControl.AllowDrop = true;
			this.PaymentSeparatorUserControl.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("80A03DE5-9D9A-458F-A55D-B659F6311822", "Payment");
			this.PaymentSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PaymentSeparatorUserControl.Name = "PaymentSeparatorUserControl";
			this.PaymentSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.PaymentSeparatorUserControl.TabIndex = 8;
			// 
			// MiscOptionsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VatCanaDropEdit);
			this.Controls.Add(this.DefermentAccountNumberDropEdit);
			this.Controls.Add(this.VATDeferTypeDropEdit);
			this.Controls.Add(this.VATDeferNumberTextBox);
			this.Controls.Add(this.ChargePaymentOrDestinationIDsDropEdit);
			this.Controls.Add(this.CustomsGuaranteeNumberDropEdit);
			this.Controls.Add(this.PaymentSeparatorUserControl);
			this.Controls.Add(this.SupportingInformationUserControl);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 94, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VatCanaDropEdit.ResumeLayout(true);
			this.VatCanaDropEdit.PerformLayout();
			this.DefermentAccountNumberDropEdit.ResumeLayout(true);
			this.DefermentAccountNumberDropEdit.PerformLayout();
			this.VATDeferTypeDropEdit.ResumeLayout(true);
			this.VATDeferTypeDropEdit.PerformLayout();
			this.ChargePaymentOrDestinationIDsDropEdit.ResumeLayout(true);
			this.ChargePaymentOrDestinationIDsDropEdit.PerformLayout();
			this.CustomsGuaranteeNumberDropEdit.ResumeLayout(true);
			this.CustomsGuaranteeNumberDropEdit.PerformLayout();
			this.PaymentSeparatorUserControl.ResumeLayout(true);
			this.PaymentSeparatorUserControl.PerformLayout();
			this.SupportingInformationUserControl.ResumeLayout(true);
			this.SupportingInformationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal SupportingInformationControl SupportingInformationUserControl;
		internal ZArchitecture.GUI.ZDropEdit DefermentAccountNumberDropEdit;
		internal ZArchitecture.GUI.ZDropEdit VATDeferTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit VatCanaDropEdit;
		internal ZArchitecture.ZTextBox VATDeferNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit ChargePaymentOrDestinationIDsDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CustomsGuaranteeNumberDropEdit;
		internal ZArchitecture.GUI.SeparatorUserControl PaymentSeparatorUserControl;

		#endregion
	}
}
