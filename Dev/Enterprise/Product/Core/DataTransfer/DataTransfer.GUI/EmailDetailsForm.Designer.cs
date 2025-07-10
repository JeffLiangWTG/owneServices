namespace Enterprise.DataTransfer.GUI
{
	public partial class EmailDetailsForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.BodyTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 299, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(216);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(217);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataTransfer.Business.EmailExportInstructions);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|4fd21c97-23b8-4f30-9d2a-d5e90930b709", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 276, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|d54d8fbb-d3fb-484e-afe8-bb71967b6a69", "Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 276, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SendButton.TabIndex = 4;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// SubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "Subject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DataTransfer.Business.EmailExportInstructions)(null)).Subject)));
			this.SubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubjectTextBox.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|63808128-8661-4ef1-82b5-8124a72dc59a", "Subject (optional)");
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 45, true);
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.SubjectTextBox.TabIndex = 2;
			// 
			// ToTextbox
			// 
			this.BindingSource.SetBindingMember(this.ToTextbox, "UserEnteredRecipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DataTransfer.Business.EmailExportInstructions)(null)).UserEnteredRecipients)));
			this.ToTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ToTextbox.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|f500db51-6403-4b4d-9fff-9887f209e734", "Recipients");
			this.ToTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 22, true);
			this.ToTextbox.Name = "ToTextbox";
			this.ToTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ToTextbox.TabIndex = 1;
			// 
			// BodyTextbox
			// 
			this.BindingSource.SetBindingMember(this.BodyTextbox, "Body");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DataTransfer.Business.EmailExportInstructions)(null)).Body)));
			this.BodyTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BodyTextbox.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|1f301353-8b3d-4e69-9225-d4b538c2b60c", "Body (optional)");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.BodyTextbox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.BodyTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 97, true);
			this.BodyTextbox.Multiline = true;
			this.BodyTextbox.Name = "BodyTextbox";
			this.BodyTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 148, true);
			this.BodyTextbox.TabIndex = 3;
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|07996341-ba44-43c1-97d6-41960afd5627", "Send this export file using the following details");
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 22, true);
			this.InstructionsLabel.TabIndex = 9;
			// 
			// EmailDetailsForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 321, true);
			this.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("EmailDetailsForm|58aef659-9650-443f-9924-f205ebe4bdeb", "Email Export File");
			this.Controls.Add(this.InstructionsLabel);
			this.Controls.Add(this.BodyTextbox);
			this.Controls.Add(this.ToTextbox);
			this.Controls.Add(this.SubjectTextBox);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.DataTransfer";
			this.DataSourceType = typeof(Enterprise.DataTransfer.Business.EmailExportInstructions);
			this.DataSourceTypeName = "Enterprise.DataTransfer.Business.EmailExportInstructions";
			this.Name = "EmailDetailsForm";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.SubjectTextBox, 0);
			this.Controls.SetChildIndex(this.ToTextbox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BodyTextbox, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton SendButton;
		protected Enterprise.ZArchitecture.ZTextBox SubjectTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ToTextbox;
		protected Enterprise.ZArchitecture.ZTextBox BodyTextbox;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.ZLabel InstructionsLabel;
	}
}
