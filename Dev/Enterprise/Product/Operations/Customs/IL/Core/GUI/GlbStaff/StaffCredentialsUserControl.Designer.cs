using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Enterprise.Customs.IL.GUI
{
    public partial class StaffCredentialsUserControl
    {
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CertificateAuthorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PinCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearSignatureButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddSignatureButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateAuthorityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.GlbStaffWrapper);
			// 
			// CertificateAuthorityDropEdit
			// 
			this.CertificateAuthorityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateAuthorityDropEdit, "PasswordCollection.GP_CertificateAuthority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbILStaffExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_CertificateAuthority)));
			this.CertificateAuthorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 57, true);
			this.CertificateAuthorityDropEdit.Name = "CertificateAuthorityDropEdit";
			this.CertificateAuthorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CertificateAuthorityDropEdit.TabIndex = 1;
			// 
			// CertificateIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateIdTextBox, "PasswordCollection.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbILStaffExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_UserID)));
			this.CertificateIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificateIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 83, true);
			this.CertificateIdTextBox.Name = "CertificateIdTextBox";
			this.CertificateIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CertificateIdTextBox.TabIndex = 2;
			// 
			// PinCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PinCodeTextBox, "PasswordCollection.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbILStaffExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).CurrentDecryptedPassword)));
			this.PinCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PinCodeTextBox.UseSystemPasswordChar = true;
			this.PinCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 112, true);
			this.PinCodeTextBox.Name = "PinCodeTextBox";
			this.PinCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PinCodeTextBox.TabIndex = 3;
			// 
			// ClearAutomaticSignatureButton
			// 
			this.ClearSignatureButton.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("7F940C42-5B4B-447E-8A15-1882060935ED", "Clear Signature");
			this.ClearSignatureButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 3, true);
			this.ClearSignatureButton.Name = "ClearSignatureButton";
			this.ClearSignatureButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.ClearSignatureButton.TabIndex = 5;
			this.ClearSignatureButton.ToolTipCaption = null;
			// 
			// AddAutomaticSignatureButton
			// 
			this.AddSignatureButton.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("760DA5F2-6D4B-4264-8B28-DECCED533B39", "Add Signature");
			this.AddSignatureButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 3, true);
			this.AddSignatureButton.Name = "AddSignatureButton";
			this.AddSignatureButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.AddSignatureButton.TabIndex = 4;
			this.AddSignatureButton.ToolTipCaption = null;
			// 
			// StaffCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClearSignatureButton);
			this.Controls.Add(this.AddSignatureButton);
			this.Controls.Add(this.CertificateAuthorityDropEdit);
			this.Controls.Add(this.CertificateIdTextBox);
			this.Controls.Add(this.PinCodeTextBox);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateAuthorityDropEdit.ResumeLayout(true);
			this.CertificateAuthorityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZDropEdit CertificateAuthorityDropEdit;
		Enterprise.ZArchitecture.ZTextBox CertificateIdTextBox;
		Enterprise.ZArchitecture.ZTextBox PinCodeTextBox;
		ZArchitecture.GUI.ZButton ClearSignatureButton;
		ZArchitecture.GUI.ZButton AddSignatureButton;

		#endregion
	}
}
