namespace Enterprise.BufferManagement.GUI
{
	partial class BMBufferTimespanForm
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
		private new void InitializeComponent()
		{
			this.nameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.bufferTimespanEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 269, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.nameTextBox);
			this.MainTabPage.Controls.Add(this.isActiveCheckBox);
			this.MainTabPage.Controls.Add(this.bufferTimespanEdit);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 245, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 245, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 245, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 269, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBufferTimespan);
			// 
			// nameTextBox
			// 
			this.BindingSource.SetBindingMember(this.nameTextBox, "BMT_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBufferTimespan)(null)).BMT_Name)));
			this.nameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.nameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 3, true);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 15, true);
			this.nameTextBox.TabIndex = 0;
			// 
			// isActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "BMT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBufferTimespan)(null)).BMT_IsActive)));
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 30, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.isActiveCheckBox.TabIndex = 2;
			// 
			// bufferTimespanEdit
			// 
			this.BindingSource.SetBindingMember(this.bufferTimespanEdit, "BufferTimespan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.BMBufferTimespan)(null)).BufferTimespan)));
			this.bufferTimespanEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 30, true);
			this.bufferTimespanEdit.Name = "bufferTimespanEdit";
			this.bufferTimespanEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.bufferTimespanEdit.TabIndex = 1;
			// 
			// BMBufferTimespanForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("71d28437-cb25-442c-be35-5ee023b6918e", "Buffer Timespan");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 325, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBufferTimespan);
			this.Name = "BMBufferTimespanForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "BMBufferTimespanForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox nameTextBox;
		private ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
		private ZArchitecture.GUI.ZTimeEditEx bufferTimespanEdit;
	}
}
