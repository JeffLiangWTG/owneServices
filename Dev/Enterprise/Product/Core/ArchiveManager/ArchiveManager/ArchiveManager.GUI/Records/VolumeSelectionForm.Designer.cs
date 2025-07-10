using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.GUI.Records
{
	partial class VolumeSelectionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.folderBrowserDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			this.volumeLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.folderSelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.hintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.selectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Records.VolumeSelection);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.ShowNewFolderButton = false;
			// 
			// volumeLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.volumeLocationTextBox, "VolumeLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ArchiveManager.Business.Records.VolumeSelection)(null)).VolumeLocation)));
			this.volumeLocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.volumeLocationTextBox.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("VolumeSelectionForm|d3c9c723-bbeb-453e-9952-0349a5195144", "", "Select a directory or drive.");
			this.volumeLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 57, true);
			this.volumeLocationTextBox.Name = "volumeLocationTextBox";
			this.volumeLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.volumeLocationTextBox.TabIndex = 1;
			// 
			// folderSelectButton
			// 
			this.folderSelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 57, true);
			this.folderSelectButton.Name = "folderSelectButton";
			this.folderSelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.folderSelectButton.TabIndex = 2;
			this.folderSelectButton.Text = "...";
			this.folderSelectButton.UseVisualStyleBackColor = true;
			this.folderSelectButton.Click += new System.EventHandler(this.folderSelectButton_Click);
			// 
			// hintLabel
			// 
			this.BindingSource.SetBindingMember(this.hintLabel, "Hint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ArchiveManager.Business.Records.VolumeSelection)(null)).Hint)));
			this.hintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 41, true);
			this.hintLabel.TabIndex = 0;
			// 
			// selectButton
			// 
			this.selectButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("VolumeSelectionForm|6009027f-758a-41c8-9082-3621a4981973", "OK");
			this.selectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 101, true);
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.selectButton.TabIndex = 3;
			this.selectButton.UseVisualStyleBackColor = true;
			this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("VolumeSelectionForm|c6ef102c-5b68-4227-9cc8-27fd51b3ef26", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 100, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// VolumeSelectionForm
			// 
			this.AcceptButton = this.selectButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 154, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.hintLabel);
			this.Controls.Add(this.volumeLocationTextBox);
			this.Controls.Add(this.folderSelectButton);
			this.Controls.Add(this.selectButton);
			this.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Records.VolumeSelection);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(471, 190, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(471, 190, true);
			this.Name = "VolumeSelectionForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "VolumeSelectionForm";
			this.Controls.SetChildIndex(this.selectButton, 0);
			this.Controls.SetChildIndex(this.folderSelectButton, 0);
			this.Controls.SetChildIndex(this.volumeLocationTextBox, 0);
			this.Controls.SetChildIndex(this.hintLabel, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog folderBrowserDialog;
		private Enterprise.ZArchitecture.ZTextBox volumeLocationTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton folderSelectButton;
		private Enterprise.ZArchitecture.ZLabel hintLabel;
		private Enterprise.ZArchitecture.GUI.ZButton selectButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}
