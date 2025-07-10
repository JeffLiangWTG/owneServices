namespace Enterprise.Customs.FR.GUI.Registry
{
	partial class AutomatedModificationControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zAutomatedModificationGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zEnableAutomatedModificationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTimeByDefaultDateEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zAutomatedModificationGroupbox.SuspendLayout();
			this.zTimeByDefaultDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Registry.AutomatedModification);
			// 
			// zAutomatedModificationGroupbox
			// 
			this.zAutomatedModificationGroupbox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("AutomatedModificationControl|1FD4DB02-287C-47F5-99A3-08774CD56E5B", "Modify Date of Duty");
			this.zAutomatedModificationGroupbox.Controls.Add(this.zEnableAutomatedModificationCheckBox);
			this.zAutomatedModificationGroupbox.Controls.Add(this.zTimeByDefaultDateEdit);
			this.zAutomatedModificationGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zAutomatedModificationGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zAutomatedModificationGroupbox.Name = "zAutomatedModificationGroupbox";
			this.zAutomatedModificationGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.zAutomatedModificationGroupbox.TabIndex = 0;
			this.zAutomatedModificationGroupbox.TabStop = false;
			// 
			// zEnableAutomatedModificationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.zEnableAutomatedModificationCheckBox, "EnableAutomatedModification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Registry.AutomatedModification)(null)).EnableAutomatedModification)));
			this.zEnableAutomatedModificationCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("AutomatedModificationControl|977EF6A4-7BFA-44A8-AAE2-F6277ED86A50", "Enable automated Modification");
			this.zEnableAutomatedModificationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.zEnableAutomatedModificationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 36, true);
			this.zEnableAutomatedModificationCheckBox.Name = "zEnableAutomatedModificationCheckBox";
			this.zEnableAutomatedModificationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 24, true);
			this.zEnableAutomatedModificationCheckBox.TabIndex = 0;
			// 
			// zTimeByDefaultDateEdit
			// 
			this.zTimeByDefaultDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zTimeByDefaultDateEdit, "TimeByDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Registry.AutomatedModification)(null)).TimeByDefault)));
			this.zTimeByDefaultDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("AutomatedModificationControl|8A0C60FD-8F14-4BF0-8A55-6C8B95B982DE", "Time By Default");
			this.zTimeByDefaultDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 66, true);
			this.zTimeByDefaultDateEdit.Name = "zTimeByDefaultDateEdit";
			this.zTimeByDefaultDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zTimeByDefaultDateEdit.TabIndex = 1;
			// 
			// AutomatedModificationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zAutomatedModificationGroupbox);
			this.Name = "AutomatedModificationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zAutomatedModificationGroupbox.ResumeLayout(false);
			this.zAutomatedModificationGroupbox.PerformLayout();
			this.zTimeByDefaultDateEdit.ResumeLayout(true);
			this.zTimeByDefaultDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox zAutomatedModificationGroupbox;
		private ZArchitecture.GUI.ZCheckBox zEnableAutomatedModificationCheckBox;
		private ZArchitecture.GUI.ZTimeEdit zTimeByDefaultDateEdit;
	}
}
