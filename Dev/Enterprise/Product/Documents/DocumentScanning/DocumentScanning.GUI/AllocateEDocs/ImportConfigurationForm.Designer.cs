
namespace Enterprise.DocumentScanning.GUI
{
	public partial class ImportConfigurationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Dispose

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

		#endregion

		#region Designer generated code

		private Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog ImportConfigDialog;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OuputFileOptionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutoAllocateCheckBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton AutoRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton AutoSingleRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton NewFileRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton BatchRadioButton;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UseCoverSheetCheckBox;

		new void InitializeComponent()
		{
			this.ImportConfigDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			this.OuputFileOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UseCoverSheetCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BatchRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NewFileRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AutoSingleRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AutoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AutoAllocateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OuputFileOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 240, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 23, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 8;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.FileImporter);
			// 
			// ImportConfigDialog
			// 
			this.ImportConfigDialog.CreateDirectory = false;
			this.ImportConfigDialog.Description = "";
			this.ImportConfigDialog.RequireMappablePath = false;
			this.ImportConfigDialog.RootFolder = System.Environment.SpecialFolder.Desktop;
			this.ImportConfigDialog.ShowNewFolderButton = true;
			// 
			// OuputFileOptionsGroupBox
			// 
			this.OuputFileOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OuputFileOptionsGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|d376cad6-b2b9-4d28-9248-9931882e1eb6", "Output File Options");
			this.OuputFileOptionsGroupBox.Controls.Add(this.UseCoverSheetCheckBox);
			this.OuputFileOptionsGroupBox.Controls.Add(this.BatchRadioButton);
			this.OuputFileOptionsGroupBox.Controls.Add(this.NewFileRadioButton);
			this.OuputFileOptionsGroupBox.Controls.Add(this.AutoSingleRadioButton);
			this.OuputFileOptionsGroupBox.Controls.Add(this.AutoRadioButton);
			this.OuputFileOptionsGroupBox.Controls.Add(this.AutoAllocateCheckBox);
			this.OuputFileOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 4, true);
			this.OuputFileOptionsGroupBox.Name = "OuputFileOptionsGroupBox";
			this.OuputFileOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 198, true);
			this.OuputFileOptionsGroupBox.TabIndex = 3;
			this.OuputFileOptionsGroupBox.TabStop = false;
			// 
			// UseCoverSheetCheckBox
			// 
			this.UseCoverSheetCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseCoverSheetCheckBox, "IsUsingCoverSheet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IsUsingCoverSheet)));
			this.UseCoverSheetCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseCoverSheetCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 25, true);
			this.UseCoverSheetCheckBox.Name = "UseCoverSheetCheckBox";
			this.UseCoverSheetCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
			this.UseCoverSheetCheckBox.TabIndex = 1;
			this.UseCoverSheetCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|8BFFDB2B-BF2E-4464-A7AF-30A454CF575A", "Use Cover Sheet");
			this.UseCoverSheetCheckBox.CheckedChanged += UseCoverSheetCheckBox_CheckedChanged;
			// 
			// BatchRadioButton
			// 
			this.BatchRadioButton.AutoCheck = false;
			this.BatchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BatchRadioButton, "IsOutputNewFileBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IsOutputNewFileBatch)));
			this.BatchRadioButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|6ac4d9f5-03f9-4864-88f0-c97f090c47d2", "New eDoc for batch of pages");
			this.BatchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BatchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 146, true);
			this.BatchRadioButton.Name = "BatchRadioButton";
			this.BatchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
			this.BatchRadioButton.TabIndex = 7;
			// 
			// NewFileRadioButton
			// 
			this.NewFileRadioButton.AutoCheck = false;
			this.NewFileRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NewFileRadioButton, "IsOutputNewFileSingle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IsOutputNewFileSingle)));
			this.NewFileRadioButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|30898a85-73a8-4a5e-81b6-397d55ca7a3c", "New eDoc created for each page");
			this.NewFileRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewFileRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 117, true);
			this.NewFileRadioButton.Name = "NewFileRadioButton";
			this.NewFileRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 17, true);
			this.NewFileRadioButton.TabIndex = 6;
			// 
			// AutoSingleRadioButton
			// 
			this.AutoSingleRadioButton.AutoCheck = false;
			this.AutoSingleRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoSingleRadioButton, "IsOutputAutomaticSingle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IsOutputAutomaticSingle)));
			this.AutoSingleRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoSingleRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 87, true);
			this.AutoSingleRadioButton.Name = "AutoSingleRadioButton";
			this.AutoSingleRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 17, true);
			this.AutoSingleRadioButton.TabIndex = 5;
			this.AutoSingleRadioButton.CheckedChanged += new System.EventHandler(this.SetAutoAllocateEnabled);
			// 
			// AutoRadioButton
			// 
			this.AutoRadioButton.AutoCheck = false;
			this.AutoRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoRadioButton, "IsOutputAutomatic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IsOutputAutomatic)));
			this.AutoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 57, true);
			this.AutoRadioButton.Name = "AutoRadioButton";
			this.AutoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 17, true);
			this.AutoRadioButton.TabIndex = 4;
			this.AutoRadioButton.CheckedChanged += new System.EventHandler(this.SetAutoAllocateEnabled);
			// 
			// AutoAllocateCheckBox
			// 
			this.AutoAllocateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateCheckBox, "IsAutoAllocate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IsAutoAllocate)));
			this.AutoAllocateCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|149262e8-8ab4-43ee-9452-39c1b7897ee2", "Auto Allocate");
			this.AutoAllocateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoAllocateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 22, true);
			this.AutoAllocateCheckBox.Name = "AutoAllocateCheckBox";
			this.AutoAllocateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.AutoAllocateCheckBox.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.AutoSize = true;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 208, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 26, true);
			this.PostingButtonsUserControl.TabIndex = 10;
			// 
			// ImportConfigurationForm
			// 

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|9b3ee408-497b-490c-977e-46dc67b9862a", "Import Configuration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 263, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OuputFileOptionsGroupBox);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.FileImporter);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.FileImporter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimizeBox = false;
			this.Name = "ImportConfigurationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OuputFileOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OuputFileOptionsGroupBox.ResumeLayout(false);
			this.OuputFileOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}