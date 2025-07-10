using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Diagnostics
{
	public partial class EmailDiagnosticsForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZTextBox ToTextBox;
		Enterprise.ZArchitecture.GUI.ZButton SendButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox SendGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox ReceiveGroupBox;
		Enterprise.ZArchitecture.ZTextBox ReceivedSubjectTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit ReceivedDateTimeDateEdit;
		Enterprise.ZArchitecture.GUI.ZButton CheckMailButton;
		Enterprise.ZArchitecture.ZLabel EmailSentLabel;

		System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			this.SendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EmailSentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceiveGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReceivedDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReceivedSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CheckMailButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendGroupBox.SuspendLayout();
			this.ReceiveGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 22, true);
			this.MainStatusBar.SizingGrip = false;
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Diagnostics.EmailDiagnostics);
			// 
			// SendGroupBox
			// 
			this.SendGroupBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|ca0edf76-2ccb-47c3-a93c-f7d620011524", "Send a test email");
			this.SendGroupBox.Controls.Add(this.EmailSentLabel);
			this.SendGroupBox.Controls.Add(this.SendButton);
			this.SendGroupBox.Controls.Add(this.ToTextBox);
			this.SendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SendGroupBox.Name = "SendGroupBox";
			this.SendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 82, true);
			this.SendGroupBox.TabIndex = 2;
			this.SendGroupBox.TabStop = false;
			// 
			// EmailSentLabel
			// 
			this.EmailSentLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EmailSentLabel, false);
			this.EmailSentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 52, true);
			this.EmailSentLabel.Name = "EmailSentLabel";
			this.EmailSentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 21, true);
			this.EmailSentLabel.TabIndex = 3;
			this.EmailSentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|55d6afd7-6b08-443f-a54e-15b6ed4ffe67", "Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 52, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ToTextBox
			// 
			this.ToTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ToTextBox, "To");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.EmailDiagnostics)(null)).To)));
			this.ToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ToTextBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|e932a564-09f9-44c2-8041-2ad7e41aad53", "To");
			this.ToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 22, true);
			this.ToTextBox.Name = "ToTextBox";
			this.ToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 20, true);
			this.ToTextBox.TabIndex = 1;
			// 
			// ReceiveGroupBox
			// 
			this.ReceiveGroupBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|84030cd1-cffc-415c-b1c2-d0438fb3b0dd", "Check for incoming test email");
			this.ReceiveGroupBox.Controls.Add(this.ReceivedDateTimeDateEdit);
			this.ReceiveGroupBox.Controls.Add(this.ReceivedSubjectTextBox);
			this.ReceiveGroupBox.Controls.Add(this.CheckMailButton);
			this.ReceiveGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 97, true);
			this.ReceiveGroupBox.Name = "ReceiveGroupBox";
			this.ReceiveGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 111, true);
			this.ReceiveGroupBox.TabIndex = 3;
			this.ReceiveGroupBox.TabStop = false;
			// 
			// ReceivedDateTimeDateEdit
			// 
			this.ReceivedDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivedDateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivedDateTimeDateEdit, "ReceivedDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Diagnostics.EmailDiagnostics)(null)).ReceivedDateTime)));
			this.ReceivedDateTimeDateEdit.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|e185a460-c938-46ad-8702-a81b40e282e8", "Test mail received");
			this.ReceivedDateTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivedDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 82, true);
			this.ReceivedDateTimeDateEdit.Name = "ReceivedDateTimeDateEdit";
			this.ReceivedDateTimeDateEdit.TabIndex = 3;
			// 
			// ReceivedSubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceivedSubjectTextBox, "ReceivedSubject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.EmailDiagnostics)(null)).ReceivedSubject)));
			this.ReceivedSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReceivedSubjectTextBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|4e3d3db4-cf9c-451d-a0d9-efec68b4d416", "Test mail subject");
			this.ReceivedSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 59, true);
			this.ReceivedSubjectTextBox.Name = "ReceivedSubjectTextBox";
			this.ReceivedSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.ReceivedSubjectTextBox.TabIndex = 2;
			// 
			// CheckMailButton
			// 
			this.CheckMailButton.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|0dfd70d1-5aa0-42c4-a872-4ecadd0a9823", "Check now");
			this.CheckMailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 22, true);
			this.CheckMailButton.Name = "CheckMailButton";
			this.CheckMailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CheckMailButton.TabIndex = 0;
			this.CheckMailButton.Click += new System.EventHandler(this.CheckMailButton_Click);
			// 
			// EmailDiagnosticsForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 252, true);
			this.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("EmailDiagnosticsForm|3f0c4a92-9278-4427-a455-4d68d1e96cd5", "Email Diagnostic Testing");
			this.Controls.Add(this.ReceiveGroupBox);
			this.Controls.Add(this.SendGroupBox);
			this.DataSourceType = typeof(Enterprise.Diagnostics.EmailDiagnostics);
			this.DataSourceTypeName = "Enterprise.Diagnostics.EmailDiagnostics";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "EmailDiagnosticsForm";
			this.Controls.SetChildIndex(this.SendGroupBox, 0);
			this.Controls.SetChildIndex(this.ReceiveGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendGroupBox.ResumeLayout(false);
			this.SendGroupBox.PerformLayout();
			this.ReceiveGroupBox.ResumeLayout(false);
			this.ReceiveGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
