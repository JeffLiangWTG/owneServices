namespace Enterprise.BufferManagement.GUI
{
	partial class PrimaryChannelsTabPageControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PrimaryChannelsControl = new Enterprise.BufferManagement.GUI.ChannelAssignmentControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// PrimaryChannelsControl
			// 
			this.PrimaryChannelsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrimaryChannelsControl, "PrimaryChannelsViewModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).PrimaryChannelsViewModel)));
			this.PrimaryChannelsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrimaryChannelsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrimaryChannelsControl.Name = "PrimaryChannelsControl";
			this.PrimaryChannelsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 452, true);
			this.PrimaryChannelsControl.TabIndex = 1;
			// 
			// PrimaryChannelsTabPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrimaryChannelsControl);
			this.Name = "PrimaryChannelsTabPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 452, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ChannelAssignmentControl PrimaryChannelsControl;
	}
}
