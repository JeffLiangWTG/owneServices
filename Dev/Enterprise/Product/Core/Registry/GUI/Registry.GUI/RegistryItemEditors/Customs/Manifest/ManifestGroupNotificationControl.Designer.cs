namespace Enterprise.Registry.GUI
{
	partial class ManifestGroupNotificationControl
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
			this.ManifestGroupNotificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SendModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SendErrorOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManifestGroupNotificationGroupBox.SuspendLayout();
			this.SendGroupGuidFindBox.SuspendLayout();
			this.SendModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Customs.ManifestGroupNotification);
			// 
			// ManifestGroupNotificationGroupBox
			// 
			this.ManifestGroupNotificationGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("596857d1-f7af-443d-840f-65d0387572dc", "Send Notifications");
			this.ManifestGroupNotificationGroupBox.Controls.Add(this.SendGroupGuidFindBox);
			this.ManifestGroupNotificationGroupBox.Controls.Add(this.SendModeDropEdit);
			this.ManifestGroupNotificationGroupBox.Controls.Add(this.SendErrorOnlyCheckBox);
			this.ManifestGroupNotificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestGroupNotificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestGroupNotificationGroupBox.Name = "ManifestGroupNotificationGroupBox";
			this.ManifestGroupNotificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 100, true);
			this.ManifestGroupNotificationGroupBox.TabIndex = 2;
			this.ManifestGroupNotificationGroupBox.TabStop = false;
			// 
			// SendGroupGuidFindBox
			// 
			this.SendGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendGroupGuidFindBox, "SendGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.Customs.ManifestGroupNotification)(null)).SendGroupPK)));
			this.SendGroupGuidFindBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7f348ef6-6077-480d-99b3-02489f617ef4", "Send Group");
			this.SendGroupGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.SendGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 45, true);
			this.SendGroupGuidFindBox.Name = "SendGroupGuidFindBox";
			this.SendGroupGuidFindBox.ShouldResize = true;
			this.SendGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.SendGroupGuidFindBox.TabIndex = 2;
			// 
			// SendModeDropEdit
			// 
			this.SendModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendModeDropEdit, "SendMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Customs.ManifestGroupNotification)(null)).SendMode)));
			this.SendModeDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d6b6dbaf-8d1c-4b4b-b8cf-a4c4dc013051", "Send Mode");
			this.SendModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 19, true);
			this.SendModeDropEdit.Name = "SendModeDropEdit";
			this.SendModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.SendModeDropEdit.TabIndex = 1;
			// 
			// SendErrorOnlyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SendErrorOnlyCheckBox, "SendErrorOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.Customs.ManifestGroupNotification)(null)).SendErrorOnly)));
			this.SendErrorOnlyCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("00834221-2b83-4fb0-8ed4-226ba2d24516", "Send Error Status Emails Only");
			this.SendErrorOnlyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SendErrorOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendErrorOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 70, true);
			this.SendErrorOnlyCheckBox.Name = "SendErrorOnlyCheckBox";
			this.SendErrorOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 24, true);
			this.SendErrorOnlyCheckBox.TabIndex = 3;
			this.SendErrorOnlyCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SendErrorOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// ManifestGroupNotificationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestGroupNotificationGroupBox);
			this.Name = "ManifestGroupNotificationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestGroupNotificationGroupBox.ResumeLayout(false);
			this.ManifestGroupNotificationGroupBox.PerformLayout();
			this.SendGroupGuidFindBox.ResumeLayout(true);
			this.SendGroupGuidFindBox.PerformLayout();
			this.SendModeDropEdit.ResumeLayout(true);
			this.SendModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ManifestGroupNotificationGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox SendGroupGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit SendModeDropEdit;
		internal ZArchitecture.GUI.ZCheckBox SendErrorOnlyCheckBox;
	}
}
