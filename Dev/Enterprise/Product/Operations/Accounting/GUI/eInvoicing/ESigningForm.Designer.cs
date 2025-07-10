
namespace Enterprise.Accounting.GUI.EInvoicing
{
	partial class ESigningForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.ChipsetDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SignButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.PinTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SigningDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CancelSignButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ChipsetDropEdit.SuspendLayout();
            this.CertificateDropEdit.SuspendLayout();
            this.SigningDetailsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning.ESigningBusinessObject);
            // 
            // ChipsetDropEdit
            // 
            this.ChipsetDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ChipsetDropEdit, "ChipsetType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning.ESigningBusinessObject)(null)).ChipsetType)));
            this.ChipsetDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 18, true);
            this.ChipsetDropEdit.Name = "ChipsetDropEdit";
            this.ChipsetDropEdit.ShowDescriptionBox = false;
            this.ChipsetDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.ChipsetDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.ChipsetDropEdit.TabIndex = 1;
            // 
            // CertificateDropEdit
            // 
            this.CertificateDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CertificateDropEdit, "CertificateCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning.ESigningBusinessObject)(null)).CertificateCode)));
            this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 40, true);
            this.CertificateDropEdit.Name = "CertificateDropEdit";
            this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
            this.CertificateDropEdit.TabIndex = 2;
            // 
            // SignButton
            // 
            this.SignButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ESigningForm|8b5c665e-7bbc-4adf-8404-b6874c8a4cca", "&Sign");
            this.SignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 128, true);
            this.SignButton.Name = "SignButton";
            this.SignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.SignButton.TabIndex = 4;
            this.SignButton.ToolTipCaption = null;
            this.SignButton.UseVisualStyleBackColor = true;
            this.SignButton.Click += new System.EventHandler(this.SignButton_Click);
            // 
            // PinTextBox
            // 
            this.BindingSource.SetBindingMember(this.PinTextBox, "EnteredPin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning.ESigningBusinessObject)(null)).EnteredPin)));
            this.PinTextBox.CaptionResourceString = null;
            this.PinTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 62, true);
            this.PinTextBox.Name = "PinTextBox";
            this.PinTextBox.PasswordChar = '*';
            this.PinTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 15, true);
            this.PinTextBox.TabIndex = 3;
            // 
            // SigningDetailsGroupBox
            // 
            this.SigningDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ESigningForm|bf31db78-d529-4324-bdbe-8805c325418a", "Token Details");
            this.SigningDetailsGroupBox.Controls.Add(this.PinTextBox);
            this.SigningDetailsGroupBox.Controls.Add(this.ChipsetDropEdit);
            this.SigningDetailsGroupBox.Controls.Add(this.CertificateDropEdit);
            this.SigningDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 32, true);
            this.SigningDetailsGroupBox.Name = "SigningDetailsGroupBox";
            this.SigningDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 91, true);
            this.SigningDetailsGroupBox.TabIndex = 6;
            this.SigningDetailsGroupBox.TabStop = false;
            // 
            // CancelSignButton
            // 
            this.CancelSignButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ESigningForm|7f73a3f0-b3dd-4dfa-b8ee-abcddc9bd32a", "&Cancel");
            this.CancelSignButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelSignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 128, true);
            this.CancelSignButton.Name = "CancelSignButton";
            this.CancelSignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.CancelSignButton.TabIndex = 5;
            this.CancelSignButton.ToolTipCaption = null;
            this.CancelSignButton.UseVisualStyleBackColor = true;
            this.CancelSignButton.Click += new System.EventHandler(this.CancelSignButton_Click);
            // 
            // HeadingLabel
            // 
            this.HeadingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 7, true);
            this.HeadingLabel.Name = "HeadingLabel";
            this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 14, true);
            this.HeadingLabel.TabIndex = 7;
            // 
            // ESigningForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ESigningForm|0167b4bf-ca42-4342-80d7-af5bdee9c219", "E-Sign");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 182, true);
            this.Controls.Add(this.HeadingLabel);
            this.Controls.Add(this.CancelSignButton);
            this.Controls.Add(this.SigningDetailsGroupBox);
            this.Controls.Add(this.SignButton);
            this.DataSourceType = typeof(Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning.ESigningBusinessObject);
            this.Name = "ESigningForm";
            this.Controls.SetChildIndex(this.SignButton, 0);
            this.Controls.SetChildIndex(this.SigningDetailsGroupBox, 0);
            this.Controls.SetChildIndex(this.CancelSignButton, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.HeadingLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ChipsetDropEdit.ResumeLayout(true);
            this.ChipsetDropEdit.PerformLayout();
            this.CertificateDropEdit.ResumeLayout(true);
            this.CertificateDropEdit.PerformLayout();
            this.SigningDetailsGroupBox.ResumeLayout(false);
            this.SigningDetailsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChipsetDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton SignButton;
		private Enterprise.ZArchitecture.ZTextBox PinTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SigningDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton CancelSignButton;
		private Enterprise.ZArchitecture.ZLabel HeadingLabel;
	}
}
