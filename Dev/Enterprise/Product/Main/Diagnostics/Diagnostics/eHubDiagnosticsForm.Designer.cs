using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Diagnostics
{
	public partial class eHubDiagnosticsForm : ZChildForm
	{
		ZGroupBox SendGroupBox;
		ZArchitecture.ZLabel SendInfoLabel;
		ZGroupBox CheckGroupBox;
		protected ZButton CheckButton;
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
			this.SendGroupBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|22cbcd0a-a2c7-48b9-89a5-139ce53db98c", "Send Test eHub Message");
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
			this.SendInfoLabel.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|ce5b0c14-cb22-4010-b9ce-7dc001afdc66", "Test message has been created. Please wait for 2 minutes and then press \'Check Now\' button.");
			this.SendInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 73, true);
			this.SendInfoLabel.Name = "SendInfoLabel";
			this.SendInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 33, true);
			this.SendInfoLabel.TabIndex = 3;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SendButton.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|6df42af7-cc14-454b-b3a5-79900f1a1dae", "Send");
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
			this.CheckGroupBox.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|647718db-a0b3-45c3-a7fb-6e84d13d3d99", "Check if Test Message was Received");
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
			this.CheckStatusLabel.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|5147efc7-8bbe-4e6e-a5cb-2d02a7e29394", "Check if Test Message was Received.");
			this.CheckStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 61, true);
			this.CheckStatusLabel.Name = "CheckStatusLabel";
			this.CheckStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 35, true);
			this.CheckStatusLabel.TabIndex = 4;
			// 
			// CheckButton
			// 
			this.CheckButton.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|747a9fbd-930d-41b7-bec2-946c98079042", "Check Now");
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
			this.zLabel1.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|6d267eee-033d-409a-8479-970b80d50a3e", "Confirm service tasks \'EHO\' and \'EHI\' are running before performing this test!");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 14, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 25, true);
			this.zLabel1.TabIndex = 4;
			// 
			// eHubDiagnosticsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 289, true);
			this.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("eHubDiagnosticsForm|378c9f57-8e77-4360-8445-6ee282222f47", "eHub Diagnostics Form");
			this.Controls.Add(this.SendGroupBox);
			this.Controls.Add(this.CheckGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 327, true);
			this.Name = "eHubDiagnosticsForm";
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
