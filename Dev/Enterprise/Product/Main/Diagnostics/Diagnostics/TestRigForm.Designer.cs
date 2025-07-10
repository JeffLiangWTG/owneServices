//SuppressCodeSmellById Id=ZSmells.ZTabPage, Reason=used by us internally as a dumping ground for things to test

#if DEBUG

using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Diagnostics
{
	public partial class TestRigForm : KForm
	{
		public TestRigForm()
		{
			InitializeComponent();
			Factory = new BusinessObjectFactory();
		}

		BusinessObjectFactory Factory;

		#region Windows Form Designer generated code

		OpenFileDialog virtualChannelExecuteOpenFileDialog;
		ZTabPage LicencingTabPage;
		CargoWise.Windows.UI.KTabControl LicencingTabControl;
		ZTabPage SystemRegKeyTabPage;
		CargoWise.Windows.UI.KLabel label6;
		CargoWise.Windows.UI.KButton DecryptButton;
		CargoWise.Windows.UI.KTextBox SystemRegCodeTextBox;
		CargoWise.Windows.UI.KButton SystemRegKeyButton;
		CargoWise.Windows.UI.KTextBox textBox1;
		CargoWise.Windows.UI.KLabel label11;
		CargoWise.Windows.UI.KTextBox textBox2;
		CargoWise.Windows.UI.KLabel label10;
		CargoWise.Windows.UI.KTextBox textBox3;
		CargoWise.Windows.UI.KLabel label9;
		CargoWise.Windows.UI.KTextBox textBox4;
		CargoWise.Windows.UI.KLabel label8;
		CargoWise.Windows.UI.KTextBox textBox5;
		CargoWise.Windows.UI.KLabel label7;
		private ZTabPage MessagingTabPage;
		private ZLabel EmailFilenameLabel;
		private ZButton ImportEmailFileButton;
		private ZButton BrowseButton;
		private ZTextBox EmailFilenameTextBox;
		private ZTabPage TimeZoneTabPage;
		private CargoWise.Windows.UI.KLabel UtcLabel;
		private CargoWise.Windows.UI.KTextBox DatabaseTimeTextBox;
		private CargoWise.Windows.UI.KTextBox LocalTimeTextBox;
		private CargoWise.Windows.UI.KTextBox UtcTextBox;
		private CargoWise.Windows.UI.KButton RefreshTimesButton;
		private CargoWise.Windows.UI.KLabel DatabaseTimeLabel;
		private CargoWise.Windows.UI.KLabel LocalTimeLabel;
		private ZTabPage DocEngineTabPage;
		private Enterprise.DocumentEngine.GUI.Unit_Testing_Utility_Classes.TestRigUserControl testRigUserControl1;
		protected internal ZTabControl tabControl1;
		CargoWise.Windows.UI.KLabel DbNameLabel;
		CargoWise.Windows.UI.KTextBox DbNameTextBox;
		CargoWise.Windows.UI.KLabel AppLoginLabel;
		CargoWise.Windows.UI.KTextBox AppLoginTextBox;
		CargoWise.Windows.UI.KTextBox AppPwdTextBox;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.virtualChannelExecuteOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.LicencingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicencingTabControl = new CargoWise.Windows.UI.KTabControl();
			this.SystemRegKeyTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.label6 = new CargoWise.Windows.UI.KLabel();
			this.DecryptButton = new CargoWise.Windows.UI.KButton();
			this.SystemRegCodeTextBox = new CargoWise.Windows.UI.KTextBox();
			this.SystemRegKeyButton = new CargoWise.Windows.UI.KButton();
			this.textBox1 = new CargoWise.Windows.UI.KTextBox();
			this.label11 = new CargoWise.Windows.UI.KLabel();
			this.textBox2 = new CargoWise.Windows.UI.KTextBox();
			this.label10 = new CargoWise.Windows.UI.KLabel();
			this.textBox3 = new CargoWise.Windows.UI.KTextBox();
			this.label9 = new CargoWise.Windows.UI.KLabel();
			this.textBox4 = new CargoWise.Windows.UI.KTextBox();
			this.label8 = new CargoWise.Windows.UI.KLabel();
			this.textBox5 = new CargoWise.Windows.UI.KTextBox();
			this.label7 = new CargoWise.Windows.UI.KLabel();
			this.UnrestrictedWriterLoginLabel = new CargoWise.Windows.UI.KLabel();
			this.UnrestrictedWriterLoginTextBox = new CargoWise.Windows.UI.KTextBox();
			this.UnrestrictedWriterPwdTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RestrictedWriterLoginLabel = new CargoWise.Windows.UI.KLabel();
			this.RestrictedWriterLoginTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RestrictedWriterPwdTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RestrictedReaderLoginLabel = new CargoWise.Windows.UI.KLabel();
			this.RestrictedReaderLoginTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RestrictedReaderPwdTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ReaderLoginLabel = new CargoWise.Windows.UI.KLabel();
			this.ReaderLoginTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ReaderPwdTextBox = new CargoWise.Windows.UI.KTextBox();
			this.DbNameLabel = new CargoWise.Windows.UI.KLabel();
			this.DbNameTextBox = new CargoWise.Windows.UI.KTextBox();
			this.AppLoginLabel = new CargoWise.Windows.UI.KLabel();
			this.AppLoginTextBox = new CargoWise.Windows.UI.KTextBox();
			this.AppPwdTextBox = new CargoWise.Windows.UI.KTextBox();
			this.MessagingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDIMessageCreationUserControl = new Enterprise.Customs.GUI.Testing.TestRigEDIMessageCreationUserControl();
			this.MessagingTopPael = new CargoWise.Windows.UI.KPanel();
			this.CreateUSTestJobsGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.CreateUSTestJobsLabel = new CargoWise.Windows.UI.KLabel();
			this.CreateUSTestJobsPanel = new CargoWise.Windows.UI.KPanel();
			this.CreateUSTestJobsButton = new CargoWise.Windows.UI.KButton();
			this.EmailFilenameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImportEmailFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EmailFilenameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TimeZoneTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DatabaseTimeLabel = new CargoWise.Windows.UI.KLabel();
			this.LocalTimeLabel = new CargoWise.Windows.UI.KLabel();
			this.UtcLabel = new CargoWise.Windows.UI.KLabel();
			this.DatabaseTimeTextBox = new CargoWise.Windows.UI.KTextBox();
			this.LocalTimeTextBox = new CargoWise.Windows.UI.KTextBox();
			this.UtcTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RefreshTimesButton = new CargoWise.Windows.UI.KButton();
			this.tabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DocEngineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.testRigUserControl1 = new Enterprise.DocumentEngine.GUI.Unit_Testing_Utility_Classes.TestRigUserControl();
			this.BillingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillingDataTextBox = new CargoWise.Windows.UI.KTextBox();
			this.BillingDecodeDataButton = new CargoWise.Windows.UI.KButton();
			this.BillingEncodedDataTextBox = new CargoWise.Windows.UI.KTextBox();
			this.FeatureControlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FeatureControlLabel2 = new CargoWise.Windows.UI.KLabel();
			this.FeatureControlLabel1 = new CargoWise.Windows.UI.KLabel();
			this.FeatureControlParameterTextBox = new CargoWise.Windows.UI.KTextBox();
			this.FeatureControlParameterButton = new CargoWise.Windows.UI.KButton();
			this.FeatureControlUtcNowTextbox = new CargoWise.Windows.UI.KTextBox();
			this.FeatureControlCodeTextbox = new CargoWise.Windows.UI.KTextBox();
			this.FeatureControlNewContentTextBox = new CargoWise.Windows.UI.KTextBox();
			this.FeatureControlSaveButton = new CargoWise.Windows.UI.KButton();
			this.FeatureControlContentTextBox = new CargoWise.Windows.UI.KTextBox();
			this.FeatureControlLoadButton = new CargoWise.Windows.UI.KButton();
			this.FeatureControlSampleButton = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicencingTabPage.SuspendLayout();
			this.LicencingTabControl.SuspendLayout();
			this.SystemRegKeyTabPage.SuspendLayout();
			this.MessagingTabPage.SuspendLayout();
			this.MessagingTopPael.SuspendLayout();
			this.CreateUSTestJobsGroupBox.SuspendLayout();
			this.CreateUSTestJobsPanel.SuspendLayout();
			this.TimeZoneTabPage.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.DocEngineTabPage.SuspendLayout();
			this.BillingTabPage.SuspendLayout();
			this.FeatureControlTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// virtualChannelExecuteOpenFileDialog
			// 
			this.virtualChannelExecuteOpenFileDialog.Filter = "Executables|*.exe|All files|*.*";
			this.virtualChannelExecuteOpenFileDialog.SupportMultiDottedExtensions = true;
			// 
			// LicencingTabPage
			// 
			this.LicencingTabPage.Controls.Add(this.LicencingTabControl);
			this.LicencingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LicencingTabPage.Name = "LicencingTabPage";
			this.LicencingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 490, true);
			this.LicencingTabPage.TabIndex = 22;
			this.LicencingTabPage.Text = "Licencing";
			// 
			// LicencingTabControl
			// 
			this.LicencingTabControl.Controls.Add(this.SystemRegKeyTabPage);
			this.LicencingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicencingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicencingTabControl.Name = "LicencingTabControl";
			this.LicencingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 490, true);
			this.LicencingTabControl.TabIndex = 16;
			// 
			// SystemRegKeyTabPage
			// 
			this.SystemRegKeyTabPage.Controls.Add(this.label6);
			this.SystemRegKeyTabPage.Controls.Add(this.DecryptButton);
			this.SystemRegKeyTabPage.Controls.Add(this.SystemRegCodeTextBox);
			this.SystemRegKeyTabPage.Controls.Add(this.SystemRegKeyButton);
			this.SystemRegKeyTabPage.Controls.Add(this.textBox1);
			this.SystemRegKeyTabPage.Controls.Add(this.label11);
			this.SystemRegKeyTabPage.Controls.Add(this.textBox2);
			this.SystemRegKeyTabPage.Controls.Add(this.label10);
			this.SystemRegKeyTabPage.Controls.Add(this.textBox3);
			this.SystemRegKeyTabPage.Controls.Add(this.label9);
			this.SystemRegKeyTabPage.Controls.Add(this.textBox4);
			this.SystemRegKeyTabPage.Controls.Add(this.label8);
			this.SystemRegKeyTabPage.Controls.Add(this.textBox5);
			this.SystemRegKeyTabPage.Controls.Add(this.label7);
			this.SystemRegKeyTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SystemRegKeyTabPage.Name = "SystemRegKeyTabPage";
			this.SystemRegKeyTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SystemRegKeyTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(671, 468, true);
			this.SystemRegKeyTabPage.TabIndex = 0;
			this.SystemRegKeyTabPage.Text = "System Reg Key";
			// 
			// label6
			// 
			this.label6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 12, true);
			this.label6.Name = "label6";
			this.label6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.label6.TabIndex = 2;
			this.label6.Text = "System Reg Key";
			this.label6.UseMnemonic = false;
			// 
			// DecryptButton
			// 
			this.DecryptButton.IsCaptionOverridden = true;
			this.DecryptButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 28, true);
			this.DecryptButton.Name = "DecryptButton";
			this.DecryptButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DecryptButton.TabIndex = 0;
			this.DecryptButton.Text = "Decrypt";
			this.DecryptButton.ToolTipCaption = null;
			this.DecryptButton.Click += new System.EventHandler(this.DecryptButton_Click);
			// 
			// SystemRegCodeTextBox
			// 
			this.SystemRegCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 28, true);
			this.SystemRegCodeTextBox.Name = "SystemRegCodeTextBox";
			this.SystemRegCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 17, true);
			this.SystemRegCodeTextBox.TabIndex = 1;
			// 
			// SystemRegKeyButton
			// 
			this.SystemRegKeyButton.IsCaptionOverridden = true;
			this.SystemRegKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 6, true);
			this.SystemRegKeyButton.Name = "SystemRegKeyButton";
			this.SystemRegKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SystemRegKeyButton.TabIndex = 13;
			this.SystemRegKeyButton.Text = "Load Key For This Product";
			this.SystemRegKeyButton.ToolTipCaption = null;
			this.SystemRegKeyButton.Click += new System.EventHandler(this.SystemRegKeyButton_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 60, true);
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.textBox1.TabIndex = 3;
			// 
			// label11
			// 
			this.label11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 188, true);
			this.label11.Name = "label11";
			this.label11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.label11.TabIndex = 12;
			this.label11.Text = "Expiry Date";
			this.label11.UseMnemonic = false;
			// 
			// textBox2
			// 
			this.textBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 92, true);
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.textBox2.TabIndex = 4;
			// 
			// label10
			// 
			this.label10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 156, true);
			this.label10.Name = "label10";
			this.label10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.label10.TabIndex = 11;
			this.label10.Text = "Server SID";
			this.label10.UseMnemonic = false;
			// 
			// textBox3
			// 
			this.textBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 124, true);
			this.textBox3.Name = "textBox3";
			this.textBox3.ReadOnly = true;
			this.textBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.textBox3.TabIndex = 5;
			// 
			// label9
			// 
			this.label9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 124, true);
			this.label9.Name = "label9";
			this.label9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.label9.TabIndex = 10;
			this.label9.Text = "Instance Name";
			this.label9.UseMnemonic = false;
			// 
			// textBox4
			// 
			this.textBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 156, true);
			this.textBox4.Name = "textBox4";
			this.textBox4.ReadOnly = true;
			this.textBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.textBox4.TabIndex = 6;
			// 
			// label8
			// 
			this.label8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 92, true);
			this.label8.Name = "label8";
			this.label8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.label8.TabIndex = 9;
			this.label8.Text = "DB Type";
			this.label8.UseMnemonic = false;
			// 
			// textBox5
			// 
			this.textBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 188, true);
			this.textBox5.Name = "textBox5";
			this.textBox5.ReadOnly = true;
			this.textBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.textBox5.TabIndex = 7;
			// 
			// label7
			// 
			this.label7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 60, true);
			this.label7.Name = "label7";
			this.label7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.label7.TabIndex = 8;
			this.label7.Text = "DatabaseName";
			this.label7.UseMnemonic = false;
			// 
			// MessagingTabPage
			// 
			this.MessagingTabPage.Controls.Add(this.EDIMessageCreationUserControl);
			this.MessagingTabPage.Controls.Add(this.MessagingTopPael);
			this.MessagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MessagingTabPage.Name = "MessagingTabPage";
			this.MessagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 490, true);
			this.MessagingTabPage.TabIndex = 26;
			this.MessagingTabPage.Text = "Messaging";
			// 
			// EDIMessageCreationUserControl
			// 
			this.EDIMessageCreationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDIMessageCreationUserControl.Location = new System.Drawing.Point(0, 187);
			this.EDIMessageCreationUserControl.Name = "EDIMessageCreationUserControl";
			this.EDIMessageCreationUserControl.Size = new System.Drawing.Size(1015, 548);
			this.EDIMessageCreationUserControl.TabIndex = 22;
			// 
			// MessagingTopPael
			// 
			this.MessagingTopPael.Controls.Add(this.CreateUSTestJobsGroupBox);
			this.MessagingTopPael.Controls.Add(this.EmailFilenameLabel);
			this.MessagingTopPael.Controls.Add(this.ImportEmailFileButton);
			this.MessagingTopPael.Controls.Add(this.BrowseButton);
			this.MessagingTopPael.Controls.Add(this.EmailFilenameTextBox);
			this.MessagingTopPael.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessagingTopPael.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagingTopPael.Name = "MessagingTopPael";
			this.MessagingTopPael.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 125, true);
			this.MessagingTopPael.TabIndex = 23;
			// 
			// CreateUSTestJobsGroupBox
			// 
			this.CreateUSTestJobsGroupBox.Controls.Add(this.CreateUSTestJobsLabel);
			this.CreateUSTestJobsGroupBox.Controls.Add(this.CreateUSTestJobsPanel);
			this.CreateUSTestJobsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 38, true);
			this.CreateUSTestJobsGroupBox.Name = "CreateUSTestJobsGroupBox";
			this.CreateUSTestJobsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 80, true);
			this.CreateUSTestJobsGroupBox.TabIndex = 21;
			this.CreateUSTestJobsGroupBox.TabStop = false;
			this.CreateUSTestJobsGroupBox.Text = "Create US Test Job And Messages";
			// 
			// CreateUSTestJobsLabel
			// 
			this.CreateUSTestJobsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreateUSTestJobsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CreateUSTestJobsLabel.Name = "CreateUSTestJobsLabel";
			this.CreateUSTestJobsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 39, true);
			this.CreateUSTestJobsLabel.TabIndex = 1;
			this.CreateUSTestJobsLabel.Text = "This option will create ABI, AES, AMS and ISF messages and their respective jobs";
			this.CreateUSTestJobsLabel.UseMnemonic = false;
			// 
			// CreateUSTestJobsPanel
			// 
			this.CreateUSTestJobsPanel.Controls.Add(this.CreateUSTestJobsButton);
			this.CreateUSTestJobsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CreateUSTestJobsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 53, true);
			this.CreateUSTestJobsPanel.Name = "CreateUSTestJobsPanel";
			this.CreateUSTestJobsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 25, true);
			this.CreateUSTestJobsPanel.TabIndex = 2;
			// 
			// CreateUSTestJobsButton
			// 
			this.CreateUSTestJobsButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.CreateUSTestJobsButton.IsCaptionOverridden = true;
			this.CreateUSTestJobsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 0, true);
			this.CreateUSTestJobsButton.Name = "CreateUSTestJobsButton";
			this.CreateUSTestJobsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CreateUSTestJobsButton.TabIndex = 0;
			this.CreateUSTestJobsButton.Text = "Create";
			this.CreateUSTestJobsButton.ToolTipCaption = null;
			this.CreateUSTestJobsButton.Click += new System.EventHandler(this.CreateUSTestJobsButton_Click);
			// 
			// EmailFilenameLabel
			// 
			this.EmailFilenameLabel.AutoSize = true;
			this.EmailFilenameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EmailFilenameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 15, true);
			this.EmailFilenameLabel.Name = "EmailFilenameLabel";
			this.EmailFilenameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 13, true);
			this.EmailFilenameLabel.TabIndex = 19;
			this.EmailFilenameLabel.Text = "Email Filename:";
			this.EmailFilenameLabel.UseMnemonic = false;
			// 
			// ImportEmailFileButton
			// 
			this.ImportEmailFileButton.IsCaptionOverridden = true;
			this.ImportEmailFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 10, true);
			this.ImportEmailFileButton.Name = "ImportEmailFileButton";
			this.ImportEmailFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 23, true);
			this.ImportEmailFileButton.TabIndex = 18;
			this.ImportEmailFileButton.Text = "Create MailDBItem";
			this.ImportEmailFileButton.ToolTipCaption = null;
			this.ImportEmailFileButton.Click += new System.EventHandler(this.ImportEmailFileButton_Click);
			// 
			// BrowseButton
			// 
			this.BrowseButton.IsCaptionOverridden = true;
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 10, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.BrowseButton.TabIndex = 17;
			this.BrowseButton.Text = "Browse";
			this.BrowseButton.ToolTipCaption = null;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// EmailFilenameTextBox
			// 
			this.EmailFilenameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 12, true);
			this.EmailFilenameTextBox.Name = "EmailFilenameTextBox";
			this.EmailFilenameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 17, true);
			this.EmailFilenameTextBox.TabIndex = 16;
			// 
			// TimeZoneTabPage
			// 
			this.TimeZoneTabPage.Controls.Add(this.DatabaseTimeLabel);
			this.TimeZoneTabPage.Controls.Add(this.LocalTimeLabel);
			this.TimeZoneTabPage.Controls.Add(this.UtcLabel);
			this.TimeZoneTabPage.Controls.Add(this.DatabaseTimeTextBox);
			this.TimeZoneTabPage.Controls.Add(this.LocalTimeTextBox);
			this.TimeZoneTabPage.Controls.Add(this.UtcTextBox);
			this.TimeZoneTabPage.Controls.Add(this.RefreshTimesButton);
			this.TimeZoneTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.TimeZoneTabPage.Name = "TimeZoneTabPage";
			this.TimeZoneTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TimeZoneTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 490, true);
			this.TimeZoneTabPage.TabIndex = 26;
			this.TimeZoneTabPage.Text = "Time Zone";
			// 
			// DatabaseTimeLabel
			// 
			this.DatabaseTimeLabel.AutoSize = true;
			this.DatabaseTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.DatabaseTimeLabel.Name = "DatabaseTimeLabel";
			this.DatabaseTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.DatabaseTimeLabel.TabIndex = 12;
			this.DatabaseTimeLabel.Text = "DB Time:";
			this.DatabaseTimeLabel.UseMnemonic = false;
			// 
			// LocalTimeLabel
			// 
			this.LocalTimeLabel.AutoSize = true;
			this.LocalTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 35, true);
			this.LocalTimeLabel.Name = "LocalTimeLabel";
			this.LocalTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.LocalTimeLabel.TabIndex = 11;
			this.LocalTimeLabel.Text = "Local Time:";
			this.LocalTimeLabel.UseMnemonic = false;
			// 
			// UtcLabel
			// 
			this.UtcLabel.AutoSize = true;
			this.UtcLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.UtcLabel.Name = "UtcLabel";
			this.UtcLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.UtcLabel.TabIndex = 10;
			this.UtcLabel.Text = "UTC:";
			this.UtcLabel.UseMnemonic = false;
			// 
			// DatabaseTimeTextBox
			// 
			this.DatabaseTimeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 58, true);
			this.DatabaseTimeTextBox.Name = "DatabaseTimeTextBox";
			this.DatabaseTimeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 17, true);
			this.DatabaseTimeTextBox.TabIndex = 9;
			// 
			// LocalTimeTextBox
			// 
			this.LocalTimeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 32, true);
			this.LocalTimeTextBox.Name = "LocalTimeTextBox";
			this.LocalTimeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 17, true);
			this.LocalTimeTextBox.TabIndex = 8;
			// 
			// UtcTextBox
			// 
			this.UtcTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 6, true);
			this.UtcTextBox.Name = "UtcTextBox";
			this.UtcTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 17, true);
			this.UtcTextBox.TabIndex = 7;
			// 
			// RefreshTimesButton
			// 
			this.RefreshTimesButton.IsCaptionOverridden = true;
			this.RefreshTimesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 6, true);
			this.RefreshTimesButton.Name = "RefreshTimesButton";
			this.RefreshTimesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 23, true);
			this.RefreshTimesButton.TabIndex = 6;
			this.RefreshTimesButton.Text = "Refresh Times";
			this.RefreshTimesButton.ToolTipCaption = null;
			this.RefreshTimesButton.UseVisualStyleBackColor = true;
			this.RefreshTimesButton.Click += new System.EventHandler(this.RefreshTimesButton_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControl1.Controls.Add(this.LicencingTabPage);
			this.tabControl1.Controls.Add(this.MessagingTabPage);
			this.tabControl1.Controls.Add(this.TimeZoneTabPage);
			this.tabControl1.Controls.Add(this.DocEngineTabPage);
			this.tabControl1.Controls.Add(this.BillingTabPage);
			this.tabControl1.Controls.Add(this.FeatureControlTabPage);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 512, true);
			this.tabControl1.TabIndex = 2;
			// 
			// DocEngineTabPage
			// 
			this.DocEngineTabPage.Controls.Add(this.testRigUserControl1);
			this.DocEngineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DocEngineTabPage.Name = "DocEngineTabPage";
			this.DocEngineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 490, true);
			this.DocEngineTabPage.TabIndex = 18;
			this.DocEngineTabPage.Text = "DocEngine";
			// 
			// testRigUserControl1
			// 
			this.testRigUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.testRigUserControl1.Location = new System.Drawing.Point(0, 0);
			this.testRigUserControl1.Name = "testRigUserControl1";
			this.testRigUserControl1.Size = new System.Drawing.Size(1015, 735);
			this.testRigUserControl1.TabIndex = 0;
			// 
			// BillingTabPage
			// 
			this.BillingTabPage.Controls.Add(this.BillingDataTextBox);
			this.BillingTabPage.Controls.Add(this.BillingDecodeDataButton);
			this.BillingTabPage.Controls.Add(this.BillingEncodedDataTextBox);
			this.BillingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.BillingTabPage.Name = "BillingTabPage";
			this.BillingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 490, true);
			this.BillingTabPage.TabIndex = 27;
			this.BillingTabPage.Text = "Billing";
			// 
			// BillingDataTextBox
			// 
			this.BillingDataTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BillingDataTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.BillingDataTextBox.Multiline = true;
			this.BillingDataTextBox.Name = "BillingDataTextBox";
			this.BillingDataTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 442, true);
			this.BillingDataTextBox.TabIndex = 8;
			// 
			// BillingDecodeDataButton
			// 
			this.BillingDecodeDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BillingDecodeDataButton.IsCaptionOverridden = true;
			this.BillingDecodeDataButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 12, true);
			this.BillingDecodeDataButton.Name = "BillingDecodeDataButton";
			this.BillingDecodeDataButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.BillingDecodeDataButton.TabIndex = 7;
			this.BillingDecodeDataButton.Text = "Decode";
			this.BillingDecodeDataButton.ToolTipCaption = null;
			this.BillingDecodeDataButton.UseVisualStyleBackColor = true;
			this.BillingDecodeDataButton.Click += new System.EventHandler(this.BillingDecodeDataButton_Click);
			// 
			// BillingEncodedDataTextBox
			// 
			this.BillingEncodedDataTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BillingEncodedDataTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 14, true);
			this.BillingEncodedDataTextBox.Name = "BillingEncodedDataTextBox";
			this.BillingEncodedDataTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 17, true);
			this.BillingEncodedDataTextBox.TabIndex = 5;
			// 
			// FeatureControlTabPage
			// 
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlLabel2);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlLabel1);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlParameterTextBox);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlParameterButton);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlUtcNowTextbox);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlCodeTextbox);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlNewContentTextBox);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlSampleButton);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlSaveButton);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlContentTextBox);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlLoadButton);
			this.FeatureControlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FeatureControlTabPage.Name = "FeatureControlTabPage";
			this.FeatureControlTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FeatureControlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 485, true);
			this.FeatureControlTabPage.TabIndex = 28;
			this.FeatureControlTabPage.Text = "Feature Control";
			this.FeatureControlTabPage.UseVisualStyleBackColor = true;
			// 
			// FeatureControlLabel2
			// 
			this.FeatureControlLabel2.AutoSize = true;
			this.FeatureControlLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 466, true);
			this.FeatureControlLabel2.Name = "FeatureControlLabel2";
			this.FeatureControlLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.FeatureControlLabel2.TabIndex = 14;
			this.FeatureControlLabel2.Text = "UTC Time";
			this.FeatureControlLabel2.UseMnemonic = false;
			// 
			// FeatureControlLabel1
			// 
			this.FeatureControlLabel1.AutoSize = true;
			this.FeatureControlLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 466, true);
			this.FeatureControlLabel1.Name = "FeatureControlLabel1";
			this.FeatureControlLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 13, true);
			this.FeatureControlLabel1.TabIndex = 13;
			this.FeatureControlLabel1.Text = "Feature Control Code";
			this.FeatureControlLabel1.UseMnemonic = false;
			// 
			// FeatureControlParameterTextBox
			// 
			this.FeatureControlParameterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 329, true);
			this.FeatureControlParameterTextBox.Multiline = true;
			this.FeatureControlParameterTextBox.Name = "FeatureControlParameterTextBox";
			this.FeatureControlParameterTextBox.ReadOnly = true;
			this.FeatureControlParameterTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FeatureControlParameterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 124, true);
			this.FeatureControlParameterTextBox.TabIndex = 12;
			// 
			// FeatureControlParameterButton
			// 
			this.FeatureControlParameterButton.IsCaptionOverridden = true;
			this.FeatureControlParameterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 456, true);
			this.FeatureControlParameterButton.Name = "FeatureControlParameterButton";
			this.FeatureControlParameterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.FeatureControlParameterButton.TabIndex = 11;
			this.FeatureControlParameterButton.Text = "Load Parameter";
			this.FeatureControlParameterButton.ToolTipCaption = null;
			this.FeatureControlParameterButton.UseVisualStyleBackColor = true;
			this.FeatureControlParameterButton.Click += new System.EventHandler(this.FeatureControlParameterButton_Click);
			// 
			// FeatureControlUtcNowTextbox
			// 
			this.FeatureControlUtcNowTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 459, true);
			this.FeatureControlUtcNowTextbox.Name = "FeatureControlUtcNowTextbox";
			this.FeatureControlUtcNowTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.FeatureControlUtcNowTextbox.TabIndex = 10;
			this.FeatureControlUtcNowTextbox.Text = "dd/MM/yyyy HH:mm:ss";
			// 
			// FeatureControlCodeTextbox
			// 
			this.FeatureControlCodeTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 459, true);
			this.FeatureControlCodeTextbox.Name = "FeatureControlCodeTextbox";
			this.FeatureControlCodeTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 20, true);
			this.FeatureControlCodeTextbox.TabIndex = 9;
			this.FeatureControlCodeTextbox.Text = "AUTHLOGIN";
			// 
			// FeatureControlNewContentTextBox
			// 
			this.FeatureControlNewContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 165, true);
			this.FeatureControlNewContentTextBox.Multiline = true;
			this.FeatureControlNewContentTextBox.Name = "FeatureControlNewContentTextBox";
			this.FeatureControlNewContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FeatureControlNewContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 124, true);
			this.FeatureControlNewContentTextBox.TabIndex = 8;
			// 
			// FeatureControlSaveButton
			// 
			this.FeatureControlSaveButton.IsCaptionOverridden = true;
			this.FeatureControlSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 295, true);
			this.FeatureControlSaveButton.Name = "FeatureControlSaveButton";
			this.FeatureControlSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.FeatureControlSaveButton.TabIndex = 7;
			this.FeatureControlSaveButton.Text = "Save Registry";
			this.FeatureControlSaveButton.ToolTipCaption = null;
			this.FeatureControlSaveButton.UseVisualStyleBackColor = true;
			this.FeatureControlSaveButton.Click += new System.EventHandler(this.FeatureControlSaveButton_Click);
			// 
			// FeatureControlContentTextBox
			// 
			this.FeatureControlContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.FeatureControlContentTextBox.Multiline = true;
			this.FeatureControlContentTextBox.Name = "FeatureControlContentTextBox";
			this.FeatureControlContentTextBox.ReadOnly = true;
			this.FeatureControlContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FeatureControlContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 124, true);
			this.FeatureControlContentTextBox.TabIndex = 8;
			// 
			// FeatureControlLoadButton
			// 
			this.FeatureControlLoadButton.IsCaptionOverridden = true;
			this.FeatureControlLoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 136, true);
			this.FeatureControlLoadButton.Name = "FeatureControlLoadButton";
			this.FeatureControlLoadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.FeatureControlLoadButton.TabIndex = 7;
			this.FeatureControlLoadButton.Text = "Load Registry";
			this.FeatureControlLoadButton.ToolTipCaption = null;
			this.FeatureControlLoadButton.UseVisualStyleBackColor = true;
			this.FeatureControlLoadButton.Click += new System.EventHandler(this.FeatureControlLoadButton_Click);
			// 
			// FeatureControlSampleButton
			// 
			this.FeatureControlSampleButton.IsCaptionOverridden = true;
			this.FeatureControlSampleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 295, true);
			this.FeatureControlSampleButton.Name = "FeatureControlSampleButton";
			this.FeatureControlSampleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.FeatureControlSampleButton.TabIndex = 7;
			this.FeatureControlSampleButton.Text = "Sample ...";
			this.FeatureControlSampleButton.ToolTipCaption = null;
			this.FeatureControlSampleButton.UseVisualStyleBackColor = true;
			this.FeatureControlSampleButton.Click += new System.EventHandler(this.FeatureControlSampleButton_Click);
			// 
			// TestRigForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 512, true);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TestRigForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Test Rig";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicencingTabPage.ResumeLayout(false);
			this.LicencingTabPage.PerformLayout();
			this.LicencingTabControl.ResumeLayout(false);
			this.LicencingTabControl.PerformLayout();
			this.SystemRegKeyTabPage.ResumeLayout(false);
			this.SystemRegKeyTabPage.PerformLayout();
			this.MessagingTabPage.ResumeLayout(false);
			this.MessagingTabPage.PerformLayout();
			this.MessagingTopPael.ResumeLayout(false);
			this.MessagingTopPael.PerformLayout();
			this.CreateUSTestJobsGroupBox.ResumeLayout(false);
			this.CreateUSTestJobsGroupBox.PerformLayout();
			this.CreateUSTestJobsPanel.ResumeLayout(false);
			this.CreateUSTestJobsPanel.PerformLayout();
			this.TimeZoneTabPage.ResumeLayout(false);
			this.TimeZoneTabPage.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabControl1.PerformLayout();
			this.DocEngineTabPage.ResumeLayout(false);
			this.DocEngineTabPage.PerformLayout();
			this.BillingTabPage.ResumeLayout(false);
			this.BillingTabPage.PerformLayout();
			this.FeatureControlTabPage.ResumeLayout(false);
			this.FeatureControlTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

#endregion

		private CargoWise.Windows.UI.KGroupBox CreateUSTestJobsGroupBox;
		private CargoWise.Windows.UI.KLabel CreateUSTestJobsLabel;
		private CargoWise.Windows.UI.KPanel CreateUSTestJobsPanel;
		private CargoWise.Windows.UI.KButton CreateUSTestJobsButton;
		private System.ComponentModel.IContainer components;
		private CargoWise.Windows.UI.KLabel ReaderLoginLabel;
		private CargoWise.Windows.UI.KTextBox ReaderLoginTextBox;
		private CargoWise.Windows.UI.KTextBox ReaderPwdTextBox;
		private ZTabPage BillingTabPage;
		private KTextBox BillingEncodedDataTextBox;
		private KButton BillingDecodeDataButton;
		private KTextBox BillingDataTextBox;
		private KLabel RestrictedReaderLoginLabel;
		private KTextBox RestrictedReaderLoginTextBox;
		private KTextBox RestrictedReaderPwdTextBox;
		private KLabel UnrestrictedWriterLoginLabel;
		private KTextBox UnrestrictedWriterLoginTextBox;
		private KTextBox UnrestrictedWriterPwdTextBox;
		private KLabel RestrictedWriterLoginLabel;
		private KTextBox RestrictedWriterLoginTextBox;
		private KTextBox RestrictedWriterPwdTextBox;
		private KPanel MessagingTopPael;
		private Enterprise.Customs.GUI.Testing.TestRigEDIMessageCreationUserControl EDIMessageCreationUserControl;
		private ZTabPage FeatureControlTabPage;
		private KTextBox FeatureControlContentTextBox;
		private KButton FeatureControlLoadButton;
		private KTextBox FeatureControlCodeTextbox;
		private KTextBox FeatureControlUtcNowTextbox;
		private KTextBox FeatureControlParameterTextBox;
		private KButton FeatureControlParameterButton;
		private KTextBox FeatureControlNewContentTextBox;
		private KButton FeatureControlSaveButton;
		private KLabel FeatureControlLabel2;
		private KLabel FeatureControlLabel1;
		private KButton FeatureControlSampleButton;
	}
}
#endif
