namespace Enterprise.BufferManagement.GUI
{
	partial class SecondaryChannelsTabPageControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SecondaryChannelsControl = new Enterprise.BufferManagement.GUI.ChannelAssignmentControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// SecondaryChannelsControl
			// 
			this.SecondaryChannelsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecondaryChannelsControl, "SecondaryChannelsViewModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).SecondaryChannelsViewModel)));
			this.SecondaryChannelsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryChannelsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecondaryChannelsControl.Name = "SecondaryChannelsControl";
			this.SecondaryChannelsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 412, true);
			this.SecondaryChannelsControl.TabIndex = 1;
			// 
			// SecondaryChannelsTabPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SecondaryChannelsControl);
			this.Name = "SecondaryChannelsTabPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 412, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ChannelAssignmentControl SecondaryChannelsControl;
	}
}
