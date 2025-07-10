using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.GUI.Records
{
	partial class OfflineStorageForm
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
			this.components = new System.ComponentModel.Container();
			this.folderBrowserDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.folderButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.archiveDateToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.locationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.abortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.progressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.archiveProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.archiveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.confirmationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.mainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.archiveTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.requiredCapacityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.freeSpaceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.archiveTabPage.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 540, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Records.OfflineStorage);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|9a92bf08-acb4-4123-a893-8e76246118ac", "Step 1: Specify Archive Parameters");
			this.zGroupBox1.Controls.Add(this.freeSpaceCalcEdit);
			this.zGroupBox1.Controls.Add(this.folderButton);
			this.zGroupBox1.Controls.Add(this.archiveDateToDateEdit);
			this.zGroupBox1.Controls.Add(this.locationTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 84, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// freeSpaceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.freeSpaceCalcEdit, "FreeSpaceInMB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ArchiveManager.Business.Records.OfflineStorage)(null)).FreeSpaceInMB)));
			this.freeSpaceCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.freeSpaceCalcEdit.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|2bdd1306-b3d0-42cd-830b-3f6e41626afb", "Free space", "Free space (MB)", "Free space (MB)", "");
			this.freeSpaceCalcEdit.Decimals = 0;
			this.freeSpaceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 45, true);
			this.freeSpaceCalcEdit.Name = "freeSpaceCalcEdit";
			this.freeSpaceCalcEdit.ReadOnly = true;
			this.freeSpaceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.freeSpaceCalcEdit.TabIndex = 5;
			this.freeSpaceCalcEdit.Text = "0";
			this.freeSpaceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// folderButton
			// 
			this.folderButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|c4a7dd52-3cdf-4729-8194-99670181877e", "...", "Press to select a drive or folder.");
			this.folderButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.folderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 45, true);
			this.folderButton.Name = "folderButton";
			this.folderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.folderButton.TabIndex = 3;
			this.folderButton.UseVisualStyleBackColor = true;
			this.folderButton.Click += new System.EventHandler(this.folderButton_Click);
			// 
			// archiveDateToDateEdit
			// 
			this.archiveDateToDateEdit.AutoCompleteMonthThreshold = 1;
			this.archiveDateToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.archiveDateToDateEdit, "ArchiveDateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ArchiveManager.Business.Records.OfflineStorage)(null)).ArchiveDateTo)));
			this.archiveDateToDateEdit.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|f5e313f0-599c-493d-be24-24d19b7273c4", "Records Archived On or Before");
			this.archiveDateToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 19, true);
			this.archiveDateToDateEdit.Name = "archiveDateToDateEdit";
			this.archiveDateToDateEdit.TabIndex = 0;
			// 
			// locationTextBox
			// 
			this.BindingSource.SetBindingMember(this.locationTextBox, "OfflineLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ArchiveManager.Business.Records.OfflineStorage)(null)).OfflineLocation)));
			this.locationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.locationTextBox.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|98fdfae9-d7b5-4d76-be8d-1d1fa3bd5402", "File Path", "File Path", "File Path", "Enter a directory or drive to store archive volume files.");
			this.locationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 46, true);
			this.locationTextBox.Name = "locationTextBox";
			this.locationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.locationTextBox.TabIndex = 2;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|3f1511c8-e7eb-44da-9535-150509b362b2", "Step 2: Review Archive Capacity");
			this.zGroupBox2.Controls.Add(this.requiredCapacityCalcEdit);
			this.zGroupBox2.Controls.Add(this.zCalcEdit2);
			this.zGroupBox2.Controls.Add(this.confirmationLabel);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 96, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 121, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			// 
			// requiredCapacityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.requiredCapacityCalcEdit, "StorageCapacityRequiredInMB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ArchiveManager.Business.Records.OfflineStorage)(null)).StorageCapacityRequiredInMB)));
			this.requiredCapacityCalcEdit.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|c5d36019-42a4-43f3-b98d-88604eab8602", "Estimated Total Volume Size (MB)");
			this.requiredCapacityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 19, true);
			this.requiredCapacityCalcEdit.Name = "requiredCapacityCalcEdit";
			this.requiredCapacityCalcEdit.ReadOnly = true;
			this.requiredCapacityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.requiredCapacityCalcEdit.TabIndex = 0;
			this.requiredCapacityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "StorageMainRecordsForArchiving");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ArchiveManager.Business.Records.OfflineStorage)(null)).StorageMainRecordsForArchiving)));
			this.zCalcEdit2.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|7b8c8b26-b158-4d8d-a226-421c5f669164", "No. of Archived Records");
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 19, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.ReadOnly = true;
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zCalcEdit2.TabIndex = 1;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// confirmationLabel
			// 
			this.BindingSource.SetBindingMember(this.confirmationLabel, "ConfirmationLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ArchiveManager.Business.Records.OfflineStorage)(null)).ConfirmationLabel)));
			this.confirmationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 55, true);
			this.confirmationLabel.Name = "confirmationLabel";
			this.confirmationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 59, true);
			this.confirmationLabel.TabIndex = 2;
			this.confirmationLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox3.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|d1f0ba30-fae6-4913-9551-b04bdda4f299", "Step 3: Archive And Monitor Progress");
			this.zGroupBox3.Controls.Add(this.abortButton);
			this.zGroupBox3.Controls.Add(this.progressTextBox);
			this.zGroupBox3.Controls.Add(this.archiveProgressBar);
			this.zGroupBox3.Controls.Add(this.archiveButton);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 223, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 285, true);
			this.zGroupBox3.TabIndex = 2;
			this.zGroupBox3.TabStop = false;
			// 
			// abortButton
			// 
			this.abortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.abortButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|6772747c-3b42-4a66-b565-7f24083de77f", "Abort");
			this.abortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.abortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 19, true);
			this.abortButton.Name = "abortButton";
			this.abortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.abortButton.TabIndex = 1;
			this.abortButton.UseVisualStyleBackColor = true;
			this.abortButton.Visible = false;
			this.abortButton.Click += new System.EventHandler(this.abortButton_Click);
			// 
			// progressTextBox
			// 
			this.progressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.progressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.progressTextBox, false);
			this.progressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 48, true);
			this.progressTextBox.Multiline = true;
			this.progressTextBox.Name = "progressTextBox";
			this.progressTextBox.ReadOnly = true;
			this.progressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.progressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 234, true);
			this.progressTextBox.TabIndex = 2;
			// 
			// archiveProgressBar
			// 
			this.archiveProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.archiveProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.archiveProgressBar.Name = "archiveProgressBar";
			this.archiveProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 23, true);
			this.archiveProgressBar.TabIndex = 0;
			// 
			// archiveButton
			//
			this.archiveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.archiveButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|fbfcbb89-323a-42c3-9b4f-7de5e4507819", "Archive");
			this.archiveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.archiveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 19, true);
			this.archiveButton.Name = "archiveButton";
			this.archiveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.archiveButton.TabIndex = 6;
			this.archiveButton.UseVisualStyleBackColor = true;
			this.archiveButton.Click += new System.EventHandler(this.archiveButton_Click);
			// 
			// mainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTabControl.Controls.Add(this.archiveTabPage);
			this.mainTabControl.Controls.Add(this.zLogsTabPage1);
			this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 564, true);
			this.mainTabControl.TabIndex = 0;
			// 
			// archiveTabPage
			// 
			this.archiveTabPage.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("OfflineStorageForm|eb1e1ef8-e16d-4291-a196-33ddb66e59c1", "Archive Offline");
			this.archiveTabPage.Controls.Add(this.zGroupBox2);
			this.archiveTabPage.Controls.Add(this.zGroupBox3);
			this.archiveTabPage.Controls.Add(this.zGroupBox1);
			this.archiveTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.archiveTabPage.Name = "archiveTabPage";
			this.archiveTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.archiveTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 537, true);
			this.archiveTabPage.TabIndex = 0;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 537, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// OfflineStorageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.abortButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 564, true);
			this.Controls.Add(this.mainTabControl);
			this.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Records.OfflineStorage);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 600, true);
			this.Name = "OfflineStorageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "OfflineStorageForm";
			this.Controls.SetChildIndex(this.mainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.mainTabControl.ResumeLayout(false);
			this.archiveTabPage.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog folderBrowserDialog;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZDateEdit archiveDateToDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox3;
		public Enterprise.ZArchitecture.ZLabel confirmationLabel;
		private Enterprise.ZArchitecture.GUI.ZButton archiveButton;
		private Enterprise.ZArchitecture.GUI.ZButton folderButton;
		private Enterprise.ZArchitecture.ZTextBox locationTextBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl mainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage archiveTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private CargoWise.Windows.UI.KProgressBar archiveProgressBar;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private Enterprise.ZArchitecture.ZTextBox progressTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton abortButton;
		private Enterprise.ZArchitecture.ZCalcEdit requiredCapacityCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit2;
		private Enterprise.ZArchitecture.ZCalcEdit freeSpaceCalcEdit;
	}
}
