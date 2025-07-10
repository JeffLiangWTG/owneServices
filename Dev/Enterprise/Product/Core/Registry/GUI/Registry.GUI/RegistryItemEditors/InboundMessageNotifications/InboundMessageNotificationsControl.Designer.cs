namespace Enterprise.Registry.GUI
{
	partial class InboundMessageNotificationsControl
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
			this.NotificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.NotifyUsersTypeDropBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotifyGroupWhenUserFoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UserLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.InboundMessageNotificationsRule);
			// 
			// NotificationGroupBox
			// 
			this.NotificationGroupBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotificationGroupBox, "NotifyGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.InboundMessageNotificationsRule)(null)).NotifyGroup)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NotificationGroupBox, false);
			this.NotificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 11, true);
			this.NotificationGroupBox.Name = "NotificationGroupBox";
			this.NotificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.NotificationGroupBox.TabIndex = 3;
			// 
			// NotifyUsersTypeDropBox
			// 
			this.NotifyUsersTypeDropBox.AllowDrop = true;
			this.NotifyUsersTypeDropBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.NotifyUsersTypeDropBox, "NotifyUserType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.InboundMessageNotificationsRule)(null)).NotifyUserType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.InboundMessageNotificationsRule)(null)).NotifyUserTypeList)));
			this.NotifyUsersTypeDropBox.BindToList = "NotifyUserTypeList";
			this.NotifyUsersTypeDropBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a36003ee-d1b9-4200-9e9e-220b9aa14a41", "Notify Users");
			this.NotifyUsersTypeDropBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 72, true);
			this.NotifyUsersTypeDropBox.Name = "NotifyUsersTypeDropBox";
			this.NotifyUsersTypeDropBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.NotifyUsersTypeDropBox.TabIndex = 4;
			// 
			// NotifyGroupWhenUserFoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyGroupWhenUserFoundCheckBox, "NotifyGroupWhenUserFound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.InboundMessageNotificationsRule)(null)).NotifyGroupWhenUserFound)));
			this.NotifyGroupWhenUserFoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NotifyGroupWhenUserFoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 108, true);
			this.NotifyGroupWhenUserFoundCheckBox.Name = "NotifyGroupWhenUserFoundCheckBox";
			this.NotifyGroupWhenUserFoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 24, true);
			this.NotifyGroupWhenUserFoundCheckBox.TabIndex = 5;
			this.NotifyGroupWhenUserFoundCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6B7C82CF-710A-4924-8C4E-881AC0B7F357", "Notify Group when Users Found");
			this.NotifyGroupWhenUserFoundCheckBox.UseVisualStyleBackColor = true;
			// 
			// UserLabel
			// 
			this.UserLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b55bd230-209d-47d7-88d2-393ac4171904", "Users to Notify");
			this.UserLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 43, true);
			this.UserLabel.Name = "UserLabel";
			this.UserLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 23, true);
			this.UserLabel.TabIndex = 6;
			// 
			// InboundMessageNotificationsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UserLabel);
			this.Controls.Add(this.NotifyGroupWhenUserFoundCheckBox);
			this.Controls.Add(this.NotifyUsersTypeDropBox);
			this.Controls.Add(this.NotificationGroupBox);
			this.Name = "InboundMessageNotificationsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 146, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox NotificationGroupBox;
		private ZArchitecture.GUI.ZDropEdit NotifyUsersTypeDropBox;
		private ZArchitecture.GUI.ZCheckBox NotifyGroupWhenUserFoundCheckBox;
		private ZArchitecture.ZLabel UserLabel;

	}
}
