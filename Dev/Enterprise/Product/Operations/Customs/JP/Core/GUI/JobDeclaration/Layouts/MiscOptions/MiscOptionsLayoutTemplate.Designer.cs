using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class MiscOptionsLayoutTemplate
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
			components = new System.ComponentModel.Container();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			naccsCredentialGuidDropEdit = new ZArchitecture.GUI.ZGuidDropEdit();
			paymentOptionsSeparatorUserControl = new SeparatorUserControl();
			paymentPartyDropEdit = new ZDropEdit();
			paymentDeadlineExtensionDropEdit = new ZDropEdit();
			naccsCredentialGuidDropEdit.SuspendLayout();
			paymentOptionsSeparatorUserControl.SuspendLayout();
			paymentPartyDropEdit.SuspendLayout();
			paymentDeadlineExtensionDropEdit.SuspendLayout();
			this.SuspendLayout();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			// 
			// naccsCredentialGuidDropEdit
			// 
			this.BindingSource.SetBindingMember(naccsCredentialGuidDropEdit, "JE_NACCSCredential");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_NACCSCredential)));
			naccsCredentialGuidDropEdit.Name = "NACCSCredentialGuidDropEdit";
			naccsCredentialGuidDropEdit.ShowDescriptionBox = false;
			// 
			// paymentOptionsSeparator
			//
			paymentOptionsSeparatorUserControl.Name = "PaymentOptionsSeparatorUserControl";
			paymentOptionsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("E9BF27AB-9347-42EE-9B93-FE39C7F69B38", "Payment Options");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_PaymentDeadlineExtension)));
			// 
			// paymentMethodDropEdit
			// 
			paymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			paymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 20, true);
			this.BindingSource.SetBindingMember(paymentPartyDropEdit, "JE_PaymentMethod");
			// 
			// paymentDeadlineExtensionDropEdit
			// 
			paymentDeadlineExtensionDropEdit.Name = "PaymentDeadlineExtensionDropEdit";
			paymentDeadlineExtensionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 40, true);
			this.BindingSource.SetBindingMember(paymentDeadlineExtensionDropEdit, "JE_PaymentDeadlineExtension");
			// 
			// MiscOptionsLayoutTemplate
			//
			this.Controls.Add(naccsCredentialGuidDropEdit);
			this.Controls.Add(paymentOptionsSeparatorUserControl);
			this.Controls.Add(paymentPartyDropEdit);
			this.Controls.Add(paymentDeadlineExtensionDropEdit);
			naccsCredentialGuidDropEdit.ResumeLayout(true);
			naccsCredentialGuidDropEdit.PerformLayout();
			paymentOptionsSeparatorUserControl.ResumeLayout(true);
			paymentOptionsSeparatorUserControl.PerformLayout();
			paymentPartyDropEdit.ResumeLayout(true);
			paymentPartyDropEdit.PerformLayout();
			paymentDeadlineExtensionDropEdit.ResumeLayout(true);
			paymentDeadlineExtensionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZGuidDropEdit naccsCredentialGuidDropEdit;
		SeparatorUserControl paymentOptionsSeparatorUserControl;
		ZDropEdit paymentPartyDropEdit;
		ZDropEdit paymentDeadlineExtensionDropEdit;

		#endregion
	}
}
