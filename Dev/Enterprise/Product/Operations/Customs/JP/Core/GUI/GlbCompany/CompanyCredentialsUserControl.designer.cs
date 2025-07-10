namespace Enterprise.Customs.JP.GUI;

partial class CompanyCredentialsUserControl
{
	void InitializeComponent()
	{
		this.NaccsMailboxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.NaccsMailboxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.MailboxTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.MailboxDomainLabel = new Enterprise.ZArchitecture.ZLabel();
		this.HasReceivedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.PasswordTextbox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.NaccsMailboxGroupBox.SuspendLayout();
		this.NaccsMailboxPanel.SuspendLayout();
		this.StatusDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Common.JPGlbCompanyWrapper);
		// 
		// NaccsMailboxGroupBox
		//
		this.NaccsMailboxGroupBox.Controls.Add(this.NaccsMailboxPanel);
		this.NaccsMailboxGroupBox.Controls.Add(this.PasswordTextbox);
		this.NaccsMailboxGroupBox.Controls.Add(this.StatusDropEdit);
		this.NaccsMailboxGroupBox.Controls.Add(this.HasReceivedCheckBox);
		this.NaccsMailboxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.NaccsMailboxGroupBox.Name = "NaccsMailboxGroupBox";
		this.NaccsMailboxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 163, true);
		this.NaccsMailboxGroupBox.TabIndex = 0;
		this.NaccsMailboxGroupBox.TabStop = false;
		this.NaccsMailboxGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("12662A07-0FC2-4EEB-ABC9-3934B81E0B24", "NACCS Mailbox");
		// 
		// NaccsMailboxPanel
		// 
		this.NaccsMailboxPanel.Controls.Add(this.MailboxTextBox);
		this.NaccsMailboxPanel.Controls.Add(this.MailboxDomainLabel);
		this.NaccsMailboxPanel.Name = "NaccsMailboxPanel";
		this.NaccsMailboxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 29, true);
		this.NaccsMailboxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 20, true);
		// 
		// MailboxTextBox
		//
		this.BindingSource.SetBindingMember(this.MailboxTextBox, "MailboxCredential.GP_MailBoxID");
		this.MailboxTextBox.Name = "MailboxTextBox";
		this.MailboxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
		this.MailboxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 24, true);
		// 
		// MailboxDomainLabel
		//
		this.BindingSource.SetBindingMember(this.MailboxDomainLabel, "MailboxCredential.MailboxDomain");
		this.MailboxDomainLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 0, true);
		this.MailboxDomainLabel.Name = "MailboxDomainLabel";
		this.MailboxDomainLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
		// 
		// PasswordTextbox
		//
		this.BindingSource.SetBindingMember(this.PasswordTextbox, "MailboxCredential.CurrentDecryptedPassword");
		this.PasswordTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 50, true);
		this.PasswordTextbox.Name = "PasswordTextbox";
		this.PasswordTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 24, true);
		this.PasswordTextbox.PasswordChar = '*';
		// 
		// HasReceivedCheckBox
		//
		this.BindingSource.SetBindingMember(this.HasReceivedCheckBox, "MailboxCredential.ShouldReceive");
		this.HasReceivedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 75, true);
		this.HasReceivedCheckBox.Name = "HasReceivedCheckBox";
		this.HasReceivedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 24, true);
		this.HasReceivedCheckBox.AutoSize = true;
		this.HasReceivedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
		// 
		// StatusDropEdit
		//
		this.BindingSource.SetBindingMember(this.StatusDropEdit, "MailboxCredential.GP_PasswordStatus");
		this.StatusDropEdit.AllowDrop = true;
		this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 100, true);
		this.StatusDropEdit.Name = "StatusDropEdit";
		this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 24, true);
		// 
		// CompanyCredentialsUserControl
		// 
		this.Controls.Add(this.NaccsMailboxGroupBox);
		this.Name = "CompanyCredentialsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 260, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.NaccsMailboxGroupBox.ResumeLayout(false);
		this.NaccsMailboxGroupBox.PerformLayout();
		this.NaccsMailboxPanel.ResumeLayout(false);
		this.NaccsMailboxPanel.PerformLayout();
		this.StatusDropEdit.ResumeLayout(true);
		this.StatusDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	ZArchitecture.GUI.ZGroupBox NaccsMailboxGroupBox;
	ZArchitecture.GUI.ZPanel NaccsMailboxPanel;
	ZArchitecture.ZTextBox MailboxTextBox;
	ZArchitecture.ZLabel MailboxDomainLabel;
	ZArchitecture.ZTextBox PasswordTextbox;
	ZArchitecture.GUI.ZDropEdit StatusDropEdit;
	ZArchitecture.GUI.ZCheckBox HasReceivedCheckBox;
}
