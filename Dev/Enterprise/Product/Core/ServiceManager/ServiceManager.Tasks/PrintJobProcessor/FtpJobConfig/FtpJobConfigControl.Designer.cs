namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	partial class FtpJobConfigControl
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
		private void InitializeComponent()
		{
			this.notifyPrintUser = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notifyOnSuccess = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notifyOnFailure = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notificationGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.notificationGroupFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Tasks.PrintJobProcessor.FtpJobConfig);
			// 
			// notifyOnSuccess
			// 
			this.notifyOnSuccess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notifyOnSuccess, "NotifyOnSuccess");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Tasks.PrintJobProcessor.FtpJobConfig)(null)).NotifyOnSuccess)));
			this.notifyOnSuccess.CaptionResourceString = Enterprise.ServiceManager.Tasks.PrintJobProcessor.Res.GetData("FtpJobConfigControl|0389F4CF-6478-4C50-A009-CECC629CC7B4", "Notify via E-mail on Successful Report Delivery");
			this.notifyOnSuccess.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyOnSuccess.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.notifyOnSuccess.Name = "notifyOnSuccess";
			this.notifyOnSuccess.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 21, true);
			this.notifyOnSuccess.TabIndex = 0;
			this.notifyOnSuccess.UseVisualStyleBackColor = true;
			// 
			// notifyOnFailure
			// 
			this.notifyOnFailure.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notifyOnFailure, "NotifyOnFailure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Tasks.PrintJobProcessor.FtpJobConfig)(null)).NotifyOnFailure)));
			this.notifyOnFailure.CaptionResourceString = Enterprise.ServiceManager.Tasks.PrintJobProcessor.Res.GetData("FtpJobConfigControl|252EC634-549E-4607-B01F-6C13DB1122D6", "Notify via E-mail on Failed Report Delivery");
			this.notifyOnFailure.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyOnFailure.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 31, true);
			this.notifyOnFailure.Name = "notifyOnFailure";
			this.notifyOnFailure.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 21, true);
			this.notifyOnFailure.TabIndex = 1;
			this.notifyOnFailure.UseVisualStyleBackColor = true;
			// 
			// notificationGroupFindBox
			// 
			this.notificationGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.notificationGroupFindBox, "NotificationGroup_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ServiceManager.Tasks.PrintJobProcessor.FtpJobConfig)(null)).NotificationGroup_PK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Tasks.PrintJobProcessor.FtpJobConfig)(null)).GlbGroups)));
			this.notificationGroupFindBox.BindToList = "GlbGroups";
			this.notificationGroupFindBox.CaptionResourceString = Enterprise.ServiceManager.Tasks.PrintJobProcessor.Res.GetData("FtpJobConfigControl|134ADA34-D980-4A70-AAF3-0B947F9457D4", "Group to Notify");
			this.notificationGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 58, true);
			this.notificationGroupFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.notificationGroupFindBox.Name = "notificationGroupFindBox";
			this.notificationGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.notificationGroupFindBox.TabIndex = 2;
			// 
			// notifyPrintUser
			// 
			this.notifyPrintUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notifyPrintUser, "NotifyPrintUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.notifyPrintUser.CaptionResourceString = Enterprise.ServiceManager.Tasks.PrintJobProcessor.Res.GetData("FtpJobConfigControl|21FAEAEC-C631-4AD0-9683-C7730D814994", "Notify Print User");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Tasks.PrintJobProcessor.FtpJobConfig)(null)).NotifyPrintUser)));
			this.notifyPrintUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyPrintUser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 85, true);
			this.notifyPrintUser.Name = "notifyPrintUser";
			this.notifyPrintUser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 21, true);
			this.notifyPrintUser.TabIndex = 3;
			this.notifyPrintUser.UseVisualStyleBackColor = true;
			// 
			// FtpJobConfigControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.notifyOnSuccess);
			this.Controls.Add(this.notifyOnFailure);
			this.Controls.Add(this.notificationGroupFindBox);
			this.Controls.Add(this.notifyPrintUser);
			this.Name = "FtpJobConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.notificationGroupFindBox.ResumeLayout(true);
			this.notificationGroupFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZCheckBox notifyPrintUser;
		internal ZArchitecture.GUI.ZCheckBox notifyOnSuccess;
		internal ZArchitecture.GUI.ZCheckBox notifyOnFailure;
		internal ZArchitecture.GUI.ZGuidFindBox notificationGroupFindBox;

		#endregion
	}
}
