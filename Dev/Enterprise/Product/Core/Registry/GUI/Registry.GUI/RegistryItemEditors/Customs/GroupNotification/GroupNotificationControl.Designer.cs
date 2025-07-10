namespace Enterprise.Registry.GUI
{
	partial class GroupNotificationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.SendNotificationsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GroupToSendGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SendNotificationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendNotificationsDropEdit.SuspendLayout();
			this.GroupToSendGuidFindBox.SuspendLayout();
			this.SendNotificationsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Customs.GroupNotification);
			// 
			// SendNotificationsDropEdit
			// 
			this.SendNotificationsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendNotificationsDropEdit, "SendMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Customs.GroupNotification)(null)).SendMode)));
			this.SendNotificationsDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f79e8257-70a4-4eb2-8ac9-f8f1bdfc3714", "Send Mode");
			this.SendNotificationsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 19, true);
			this.SendNotificationsDropEdit.Name = "SendNotificationsDropEdit";
			this.SendNotificationsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.SendNotificationsDropEdit.TabIndex = 0;
			// 
			// GroupToSendGuidFindBox
			// 
			this.GroupToSendGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupToSendGuidFindBox, "SendGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.Customs.GroupNotification)(null)).SendGroupPK)));
			this.GroupToSendGuidFindBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e3e0ad3f-e87a-4a59-bd71-0fdd35f79c89", "Send Group");
			this.GroupToSendGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 45, true);
			this.GroupToSendGuidFindBox.Name = "GroupToSendGuidFindBox";
			this.GroupToSendGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GroupToSendGuidFindBox.ParentType = null;
			this.GroupToSendGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.GroupToSendGuidFindBox.TabIndex = 1;
			// 
			// SendNotificationsGroupBox
			// 
			this.SendNotificationsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("1a6900bf-c2ae-476b-b18c-0d2c35ba35c1", "Send Notifications");
			this.SendNotificationsGroupBox.Controls.Add(this.SendNotificationsDropEdit);
			this.SendNotificationsGroupBox.Controls.Add(this.GroupToSendGuidFindBox);
			this.SendNotificationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SendNotificationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SendNotificationsGroupBox.Name = "SendNotificationsGroupBox";
			this.SendNotificationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 76, true);
			this.SendNotificationsGroupBox.TabIndex = 2;
			this.SendNotificationsGroupBox.TabStop = false;
			// 
			// GroupNotificationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SendNotificationsGroupBox);
			this.Name = "GroupNotificationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendNotificationsDropEdit.ResumeLayout(true);
			this.SendNotificationsDropEdit.PerformLayout();
			this.GroupToSendGuidFindBox.ResumeLayout(true);
			this.GroupToSendGuidFindBox.PerformLayout();
			this.SendNotificationsGroupBox.ResumeLayout(false);
			this.SendNotificationsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit SendNotificationsDropEdit;
		internal ZArchitecture.GUI.ZGuidFindBox GroupToSendGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox SendNotificationsGroupBox;
	}
}
