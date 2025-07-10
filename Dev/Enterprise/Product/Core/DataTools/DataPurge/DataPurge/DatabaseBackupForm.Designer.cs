namespace Enterprise.DataPurge
{
	public partial class DatabaseBackupForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>

		new void InitializeComponent()
		{
			this.BackupFileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Step1GroupdBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BackupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OutputTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Step1GroupdBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 341, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 22, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(376);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataPurge.DataPurger);
			// 
			// BackupFileNameTextBox
			// 
			this.BackupFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BackupFileNameTextBox, "BackupFilePath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DataPurge.DataPurger)(null)).BackupFilePath)));
			this.BackupFileNameTextBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DatabaseBackupForm|a9534b02-fb0e-4221-9572-432048a5359f", "Backup File Full Path\r\n(relative to DB server)");
			this.BackupFileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BackupFileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 22, true);
			this.BackupFileNameTextBox.Name = "BackupFileNameTextBox";
			this.BackupFileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 18, true);
			this.BackupFileNameTextBox.TabIndex = 1;
			// 
			// Step1GroupdBox
			// 
			this.Step1GroupdBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.Step1GroupdBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DatabaseBackupForm|a921f3a5-9789-403d-b355-21bc21843353", "Backup Database");
			this.Step1GroupdBox.Controls.Add(this.BackupFileNameTextBox);
			this.Step1GroupdBox.Controls.Add(this.BackupButton);
			this.Step1GroupdBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.Step1GroupdBox.Name = "Step1GroupdBox";
			this.Step1GroupdBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 52, true);
			this.Step1GroupdBox.TabIndex = 0;
			this.Step1GroupdBox.TabStop = false;
			// 
			// BackupButton
			// 
			this.BackupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BackupButton.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DatabaseBackupForm|8d1ccebf-f27d-4fe1-a7f5-c27b89f3d077", "Backup");
			this.BackupButton.IsCaptionOverridden = false;
			this.BackupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 21, true);
			this.BackupButton.Name = "BackupButton";
			this.BackupButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BackupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.BackupButton.TabIndex = 2;
			this.BackupButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.BackupButton.ToolTipCaption = null;
			this.BackupButton.Click += new System.EventHandler(this.BackupButton_Click);
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OutputTextBox, "Output");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DataPurge.DataPurger)(null)).Output)));
			this.OutputTextBox.CaptionResourceString = null;
			this.OutputTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutputTextBox, false);
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 70, true);
			this.OutputTextBox.Multiline = true;
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 266, true);
			this.OutputTextBox.TabIndex = 1;
			// 
			// DatabaseBackupForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DatabaseBackupForm|ccbb1dfe-95e8-47f2-a8b3-f7b1f69ef7ea", "Backup Database");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 362, true);
			this.Controls.Add(this.Step1GroupdBox);
			this.Controls.Add(this.OutputTextBox);
			this.DataSourceAssemblyName = "Enterprise.DataPurge";
			this.DataSourceType = typeof(Enterprise.DataPurge.DataPurger);
			this.DataSourceTypeName = "Enterprise.DataPurge.DataPurger";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 400, true);
			this.Name = "DatabaseBackupForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.OutputTextBox, 0);
			this.Controls.SetChildIndex(this.Step1GroupdBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Step1GroupdBox.ResumeLayout(false);
			this.Step1GroupdBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox BackupFileNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox Step1GroupdBox;
		private Enterprise.ZArchitecture.GUI.ZButton BackupButton;
		private Enterprise.ZArchitecture.ZTextBox OutputTextBox;
	}
}
