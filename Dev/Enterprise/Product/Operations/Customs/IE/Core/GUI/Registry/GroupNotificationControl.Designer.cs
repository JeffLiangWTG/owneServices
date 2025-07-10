namespace Enterprise.Customs.IE.GUI
{
	partial class GroupNotificationControl
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
			this.GroupNotificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SendModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupNotificationGroupBox.SuspendLayout();
			this.SendGroupGuidFindBox.SuspendLayout();
			this.SendModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Customs.GroupNotification);
			// 
			// GroupNotificationGroupBox
			// 
			this.GroupNotificationGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("285582D1-BADC-4629-B60E-FDF588DDD5A0", "Send Notifications");
			this.GroupNotificationGroupBox.Controls.Add(this.SendGroupGuidFindBox);
			this.GroupNotificationGroupBox.Controls.Add(this.SendModeDropEdit);
			this.GroupNotificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupNotificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupNotificationGroupBox.Name = "GroupNotificationGroupBox";
			this.GroupNotificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 76, true);
			this.GroupNotificationGroupBox.TabIndex = 2;
			this.GroupNotificationGroupBox.TabStop = false;
			// 
			// SendGroupGuidFindBox
			// 
			this.SendGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendGroupGuidFindBox, "SendGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.Customs.GroupNotification)(null)).SendGroupPK)));
			this.SendGroupGuidFindBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("B4F60E26-AC25-4F8B-938B-504892B84B45", "Send Group");
			this.SendGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 45, true);
			this.SendGroupGuidFindBox.Name = "SendGroupGuidFindBox";
			this.SendGroupGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SendGroupGuidFindBox.ParentType = null;
			this.SendGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.SendGroupGuidFindBox.TabIndex = 2;
			// 
			// SendModeDropEdit
			// 
			this.SendModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendModeDropEdit, "SendMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Customs.GroupNotification)(null)).SendMode)));
			this.SendModeDropEdit.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("BA5A3B86-467F-42FB-BB61-83355D64C207", "Send Mode");
			this.SendModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 19, true);
			this.SendModeDropEdit.Name = "SendModeDropEdit";
			this.SendModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.SendModeDropEdit.TabIndex = 1;
			// 
			// GroupNotificationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupNotificationGroupBox);
			this.Name = "GroupNotificationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupNotificationGroupBox.ResumeLayout(false);
			this.GroupNotificationGroupBox.PerformLayout();
			this.SendGroupGuidFindBox.ResumeLayout(true);
			this.SendGroupGuidFindBox.PerformLayout();
			this.SendModeDropEdit.ResumeLayout(true);
			this.SendModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		private ZArchitecture.GUI.ZGroupBox GroupNotificationGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox SendGroupGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit SendModeDropEdit;
	}
}
