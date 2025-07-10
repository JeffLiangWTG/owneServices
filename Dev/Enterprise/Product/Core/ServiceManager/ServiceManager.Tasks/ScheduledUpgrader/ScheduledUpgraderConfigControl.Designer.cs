namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader
{
	partial class ScheduledUpgraderConfigControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.autoDownload = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.patchOnly = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notifyOnSuccess = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notificationGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.notificationGroupFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ScheduledUpgraderConfig);
			// 
			// autoDownload
			// 
			this.autoDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.autoDownload, "AutoDownload");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.autoDownload.CaptionResourceString = Enterprise.ServiceManager.Tasks.ScheduledUpgrader.Res.GetData("ScheduledUpgraderConfigControl|B8EE4E9D-E6AA-4D3F-BFEB-BE7F08CD9D4E", "Automatically download the latest upgrade package from the upgrade server");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ScheduledUpgraderConfig)(null)).AutoDownload)));
			this.autoDownload.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoDownload.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.autoDownload.Name = "autoDownload";
			this.autoDownload.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 21, true);
			this.autoDownload.TabIndex = 0;
			this.autoDownload.UseVisualStyleBackColor = true;
			// 
			// patchOnly
			// 
			this.patchOnly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.patchOnly, "PatchOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.patchOnly.CaptionResourceString = Enterprise.ServiceManager.Tasks.ScheduledUpgrader.Res.GetData("ScheduledUpgraderConfigControl|3A874F01-4047-4A67-A387-118CB2C7B954", "Only apply patches for the current release. Do not apply major version upgrades");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ScheduledUpgraderConfig)(null)).PatchOnly)));
			this.patchOnly.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.patchOnly.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 31, true);
			this.patchOnly.Name = "patchOnly";
			this.patchOnly.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 21, true);
			this.patchOnly.TabIndex = 1;
			this.patchOnly.UseVisualStyleBackColor = true;
			// 
			// notifyOnSuccess
			// 
			this.notifyOnSuccess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notifyOnSuccess, "NotifyOnSuccess");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ScheduledUpgraderConfig)(null)).NotifyOnSuccess)));
			this.notifyOnSuccess.CaptionResourceString = Enterprise.ServiceManager.Tasks.ScheduledUpgrader.Res.GetData("ScheduledUpgraderConfigControl|6a917e18-81ac-4e78-ae2e-94fac3e54b9d", "Notify via E-mail on Successful Upgrade");
			this.notifyOnSuccess.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyOnSuccess.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 58, true);
			this.notifyOnSuccess.Name = "notifyOnSuccess";
			this.notifyOnSuccess.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 21, true);
			this.notifyOnSuccess.TabIndex = 2;
			this.notifyOnSuccess.UseVisualStyleBackColor = true;
			// 
			// notificationGroupFindBox
			// 
			this.notificationGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.notificationGroupFindBox, "NotificationGroup_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ScheduledUpgraderConfig)(null)).NotificationGroup_PK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ScheduledUpgraderConfig)(null)).GlbGroups)));
			this.notificationGroupFindBox.BindToList = "GlbGroups";
			this.notificationGroupFindBox.CaptionResourceString = Enterprise.ServiceManager.Tasks.ScheduledUpgrader.Res.GetData("ScheduledUpgraderConfigControl|f8a4dac8-638f-4779-8b87-6458b083e16e", "Group to Notify");
			this.notificationGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 85, true);
			this.notificationGroupFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.notificationGroupFindBox.Name = "notificationGroupFindBox";
			this.notificationGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 18, true);
			this.notificationGroupFindBox.TabIndex = 4;
			// 
			// ScheduledUpgraderConfigControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.patchOnly);
			this.Controls.Add(this.autoDownload);
			this.Controls.Add(this.notifyOnSuccess);
			this.Controls.Add(this.notificationGroupFindBox);
			this.Name = "ScheduledUpgraderConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 114, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.notificationGroupFindBox.ResumeLayout(true);
			this.notificationGroupFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox autoDownload;
		internal ZArchitecture.GUI.ZCheckBox patchOnly;
		internal ZArchitecture.GUI.ZCheckBox notifyOnSuccess;
		internal ZArchitecture.GUI.ZGuidFindBox notificationGroupFindBox;
	}
}
