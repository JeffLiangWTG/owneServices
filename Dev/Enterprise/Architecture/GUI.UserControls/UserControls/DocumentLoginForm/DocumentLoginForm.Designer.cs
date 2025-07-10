using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class DocumentLoginForm
	{
		#region Windows Form Designer generated code

		ZTextBox PasswordTextBox;
		ZTextBox LoginTextBox;
		public ZButton PrintButton;
		public ZButton CancelPrintButton;
		public ZButton ApprovalRequestButton;
		System.ComponentModel.Container components = null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.PasswordTextBox = new ZTextBox();
			this.LoginTextBox = new ZTextBox();
			this.PrintButton = new ZButton();
			this.CancelPrintButton = new ZButton();
			this.ApprovalRequestButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(SecurityLogin);
			//
			// PasswordTextBox
			//
			this.PasswordTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((SecurityLogin)(null)).Password);
			this.PasswordTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("DocumentLoginForm|50f60994-97a1-4289-b7ad-b4a2b5202b42", "Password");
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 35, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.PasswordTextBox.TabIndex = 1;
			//
			// LoginTextBox
			//
			this.LoginTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.LoginTextBox, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((SecurityLogin)(null)).Login);
			this.LoginTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("DocumentLoginForm|2636b68d-13ee-49e3-a40d-7d860db291a4", "User Name");
			this.LoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 12, true);
			this.LoginTextBox.Name = "LoginTextBox";
			this.LoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.LoginTextBox.TabIndex = 0;
			//
			// PrintButton
			//
			this.PrintButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.PrintButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("DocumentLoginForm|c0d59c31-6e08-4e60-b5b1-c415271c23c4", "OK");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 61, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 3;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			//
			// CancelPrintButton
			//
			this.CancelPrintButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.CancelPrintButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("DocumentLoginForm|adaeb383-60ed-49a4-9870-60b704c8cced", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.No;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 61, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 4;
			//
			// ApprovalRequestButton
			//
			this.ApprovalRequestButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.ApprovalRequestButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("4e27278c-3867-4141-a5d1-4db0f196e46c", "Approval Request");
			this.ApprovalRequestButton.DialogResult = System.Windows.Forms.DialogResult.Ignore;
			this.ApprovalRequestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 61, true);
			this.ApprovalRequestButton.Name = "ApprovalRequestButton";
			this.ApprovalRequestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 21, true);
			this.ApprovalRequestButton.TabIndex = 2;
			//
			// DocumentLoginForm
			//
			this.AcceptButton = this.PrintButton;

			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 110, true);
			this.Controls.Add(this.ApprovalRequestButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.LoginTextBox);
			this.Controls.Add(this.PasswordTextBox);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(SecurityLogin);
			this.DataSourceTypeName = "Enterprise.MasterFiles.CreditControl.Business.DocumentLogin";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "DocumentLoginForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PasswordTextBox, 0);
			this.Controls.SetChildIndex(this.LoginTextBox, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.ApprovalRequestButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
