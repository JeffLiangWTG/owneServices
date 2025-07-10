using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Diagnostics
{
	public partial class DirectxTDiagnosticsForm : ZChildForm
	{
		ZGroupBox SendGroupBox;
		ZArchitecture.ZLabel SendInfoLabel;
		ZGroupBox CheckGroupBox;
		ZButton CheckButton;
		ZArchitecture.ZLabel CheckStatusLabel;
		ZArchitecture.ZLabel zLabel1;
		ZButton SendButton;

		new void InitializeComponent()
		{
			this.SendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CheckGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CheckStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CheckButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendGroupBox.SuspendLayout();
			this.CheckGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 265, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 24, true);
			// 
			// SendGroupBox
			// 
			this.SendGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SendGroupBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|cd96f3c7-8dc0-4d25-89f5-cc67ab497a84", "Send Test Direct xT Message");
			this.SendGroupBox.Controls.Add(this.zLabel1);
			this.SendGroupBox.Controls.Add(this.SendInfoLabel);
			this.SendGroupBox.Controls.Add(this.SendButton);
			this.SendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 12, true);
			this.SendGroupBox.Name = "SendGroupBox";
			this.SendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 109, true);
			this.SendGroupBox.TabIndex = 3;
			this.SendGroupBox.TabStop = false;
			// 
			// SendInfoLabel
			// 
			this.SendInfoLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SendInfoLabel.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|002954ea-568a-4d81-90c1-622339169835", "Test message has been created. Please wait for 2 minutes and then press \'Check Now\' button.");
			this.SendInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 73, true);
			this.SendInfoLabel.Name = "SendInfoLabel";
			this.SendInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 33, true);
			this.SendInfoLabel.TabIndex = 3;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SendButton.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|aa28a478-f6fc-45c0-9f11-c1005a6a0033", "Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 42, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 28, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CheckGroupBox
			// 
			this.CheckGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CheckGroupBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|5599c868-1f24-4d62-87ae-ad87b46224f1", "Check if Test Message was Received");
			this.CheckGroupBox.Controls.Add(this.CheckStatusLabel);
			this.CheckGroupBox.Controls.Add(this.CheckButton);
			this.CheckGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 141, true);
			this.CheckGroupBox.Name = "CheckGroupBox";
			this.CheckGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 99, true);
			this.CheckGroupBox.TabIndex = 4;
			this.CheckGroupBox.TabStop = false;
			// 
			// CheckStatusLabel
			// 
			this.CheckStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CheckStatusLabel.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|ef41d80b-23b9-4d19-82f4-5f7ddf913813", "Check if Test Message was Received.");
			this.CheckStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 61, true);
			this.CheckStatusLabel.Name = "CheckStatusLabel";
			this.CheckStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 35, true);
			this.CheckStatusLabel.TabIndex = 4;
			// 
			// CheckButton
			// 
			this.CheckButton.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|383c5907-86c6-4c11-9117-47c421ebcaee", "Check Now");
			this.CheckButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.CheckButton.Name = "CheckButton";
			this.CheckButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 28, true);
			this.CheckButton.TabIndex = 3;
			this.CheckButton.UseVisualStyleBackColor = true;
			this.CheckButton.Click += new System.EventHandler(this.CheckButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.zLabel1.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|54112208-311e-4db5-9e7b-03529a58c1b8", "Confirm service tasks \'XTO\' and \'XTI\' are running before performing this test!");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 14, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 25, true);
			this.zLabel1.TabIndex = 4;
			// 
			// DirectxTDiagnosticsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 289, true);
			this.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("DirectxTDiagnosticsForm|6ff04d0f-b640-471d-bec4-1a93229a5593", "Direct xT Diagnostics Form");
			this.Controls.Add(this.SendGroupBox);
			this.Controls.Add(this.CheckGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 327, true);
			this.Name = "DirectxTDiagnosticsForm";
			this.Controls.SetChildIndex(this.CheckGroupBox, 0);
			this.Controls.SetChildIndex(this.SendGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendGroupBox.ResumeLayout(false);
			this.CheckGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
