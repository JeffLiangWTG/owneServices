using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class ScanForHeldShipmentsWizard
	{
		new void InitializeComponent()
		{
			this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.finnishButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.establishmentIDTextBoxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.establishmentIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.parcelsScannedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.fileLoadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.fileWritingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.directScanningButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.loadFileFromScanButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.writeFileToScanButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 278, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 24, true);
			this.MainStatusBar.TabIndex = 1;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ScanForOutturnHeldShipmentManager);
			// 
			// ButtonPanel
			// 
			this.buttonPanel.Controls.Add(this.finnishButton);
			this.buttonPanel.Controls.Add(this.cancelAndCloseButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.buttonPanel.Name = "ButtonPanel";
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 64, true);
			this.buttonPanel.TabIndex = 10;
			// 
			// FinnishButton
			// 
			this.finnishButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.finnishButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 6, true);
			this.finnishButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 30, true);
			this.finnishButton.Name = "FinnishButton";
			this.finnishButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 30, true);
			this.finnishButton.TabIndex = 0;
			this.finnishButton.Text = "Finish";
			this.finnishButton.UseVisualStyleBackColor = true;
			this.finnishButton.Click += new System.EventHandler(this.FinnishButton_Click);
			// 
			// CancelAndCloseButton
			// 
			this.cancelAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 5, true);
			this.cancelAndCloseButton.Name = "CancelAndCloseButton";
			this.cancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 30, true);
			this.cancelAndCloseButton.TabIndex = 1;
			this.cancelAndCloseButton.Text = "Cancel";
			this.cancelAndCloseButton.UseVisualStyleBackColor = true;
			this.cancelAndCloseButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// MainPanel
			// 
			this.mainPanel.Controls.Add(this.establishmentIDTextBoxLabel);
			this.mainPanel.Controls.Add(this.establishmentIDTextBox);
			this.mainPanel.Controls.Add(this.parcelsScannedLabel);
			this.mainPanel.Controls.Add(this.fileLoadingLabel);
			this.mainPanel.Controls.Add(this.fileWritingLabel);
			this.mainPanel.Controls.Add(this.directScanningButton);
			this.mainPanel.Controls.Add(this.loadFileFromScanButton);
			this.mainPanel.Controls.Add(this.writeFileToScanButton);
			this.mainPanel.Controls.Add(this.buttonPanel);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "MainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 302, true);
			this.mainPanel.TabIndex = 0;
			// 
			// EstablishmentIDTextBoxLabel
			// 
			this.establishmentIDTextBoxLabel.AutoSize = true;
			this.establishmentIDTextBoxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 24, true);
			this.establishmentIDTextBoxLabel.Name = "EstablishmentIDTextBoxLabel";
			this.establishmentIDTextBoxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 13, true);
			this.establishmentIDTextBoxLabel.TabIndex = 0;
			this.establishmentIDTextBoxLabel.Text = "Outturn Establishment ID:";
			// 
			// EstablishmentIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.establishmentIDTextBox, "OutturningPremiseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ScanForOutturnHeldShipmentManager)(null)).OutturningPremiseID)));
			this.establishmentIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.establishmentIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 20, true);
			this.establishmentIDTextBox.Name = "EstablishmentIDTextBox";
			this.establishmentIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.establishmentIDTextBox.TabIndex = 1;
			this.establishmentIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			// 
			// ParcelsScannedLabel
			// 
			this.parcelsScannedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.parcelsScannedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 179, true);
			this.parcelsScannedLabel.Name = "ParcelsScannedLabel";
			this.parcelsScannedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 37, true);
			this.parcelsScannedLabel.TabIndex = 9;
			this.parcelsScannedLabel.Text = "Parcels Scanned so far: 0";
			// 
			// FileLoadingLabel
			// 
			this.fileLoadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.fileLoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 120, true);
			this.fileLoadingLabel.Name = "FileLoadingLabel";
			this.fileLoadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 37, true);
			this.fileLoadingLabel.TabIndex = 7;
			this.fileLoadingLabel.Text = "File Not Loaded";
			// 
			// FileWritingLabel
			// 
			this.fileWritingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.fileWritingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 61, true);
			this.fileWritingLabel.Name = "FileWritingLabel";
			this.fileWritingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 37, true);
			this.fileWritingLabel.TabIndex = 5;
			this.fileWritingLabel.Text = "File Not Written";
			// 
			// DirectScanningButton
			// 
			this.directScanningButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 175, true);
			this.directScanningButton.Name = "DirectScanningButton";
			this.directScanningButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.directScanningButton.TabIndex = 8;
			this.directScanningButton.Text = "Commence Direct Scanning";
			this.directScanningButton.UseVisualStyleBackColor = true;
			this.directScanningButton.Click += new System.EventHandler(this.DirectScanningButton_Click);
			// 
			// LoadFileFromScanButton
			// 
			this.loadFileFromScanButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 116, true);
			this.loadFileFromScanButton.Name = "LoadFileFromScanButton";
			this.loadFileFromScanButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.loadFileFromScanButton.TabIndex = 6;
			this.loadFileFromScanButton.Text = "Load File From Scan Equipment";
			this.loadFileFromScanButton.UseVisualStyleBackColor = true;
			this.loadFileFromScanButton.Click += new System.EventHandler(this.LoadFileFromScanButton_Click);
			// 
			// WriteFileToScanButton
			// 
			this.writeFileToScanButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 57, true);
			this.writeFileToScanButton.Name = "WriteFileToScanButton";
			this.writeFileToScanButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.writeFileToScanButton.TabIndex = 4;
			this.writeFileToScanButton.Text = "Write File To Scan Equipment";
			this.writeFileToScanButton.UseVisualStyleBackColor = true;
			this.writeFileToScanButton.Click += new System.EventHandler(this.WriteFileToScanButton_Click);
			// 
			// ScanForHeldShipmentsWizard
			// 
			this.AcceptButton = this.finnishButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 302, true);
			this.Controls.Add(this.mainPanel);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ScanForOutturnHeldShipmentManager);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 340, true);
			this.Name = "ScanForHeldShipmentsWizard";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Held Shipments Scanning";
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		ZPanel buttonPanel;
		ZButton finnishButton;
		ZButton cancelAndCloseButton;
		ZPanel mainPanel;
		ZArchitecture.ZLabel parcelsScannedLabel;
		ZArchitecture.ZLabel fileLoadingLabel;
		ZArchitecture.ZLabel fileWritingLabel;
		ZButton directScanningButton;
		ZButton loadFileFromScanButton;
		ZButton writeFileToScanButton;
		protected ZArchitecture.ZTextBox establishmentIDTextBox;
		ZArchitecture.ZLabel establishmentIDTextBoxLabel;
	}
}
