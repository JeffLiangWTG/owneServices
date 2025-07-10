namespace Enterprise.BufferManagement.GUI
{
	partial class ChannelAssignmentControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ChannelsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AddChannelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowUnchanneledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OverrideChannelsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChannelByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SortCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel);
			// 
			// ChannelsPanel
			// 
			this.ChannelsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ChannelsPanel.AutoScroll = true;
			this.ChannelsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.ChannelsPanel.Name = "ChannelsPanel";
			this.ChannelsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 330, true);
			this.ChannelsPanel.TabIndex = 0;
			// 
			// AddChannelButton
			// 
			this.AddChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.AddChannelButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4994a72b-fa7b-49ea-93e5-081e2b5656d7", "Add");
			this.AddChannelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.AddChannelButton.Name = "AddChannelButton";
			this.AddChannelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 25, true);
			this.AddChannelButton.TabIndex = 1;
			this.AddChannelButton.UseVisualStyleBackColor = true;
			this.AddChannelButton.Click += new System.EventHandler(this.AddChannelButton_Click);
			// 
			// ShowUnchanneledCheckBox
			// 
			this.ShowUnchanneledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowUnchanneledCheckBox, "ShowUnchanneled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel)(null)).ShowUnchanneled)));
			this.ShowUnchanneledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowUnchanneledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 29, true);
			this.ShowUnchanneledCheckBox.Name = "ShowUnchanneledCheckBox";
			this.ShowUnchanneledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 16, true);
			this.ShowUnchanneledCheckBox.TabIndex = 6;
			this.ShowUnchanneledCheckBox.UseVisualStyleBackColor = true;
			// 
			// OverrideChannelsCheckBox
			// 
			this.OverrideChannelsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideChannelsCheckBox, "OverrideChannels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel)(null)).OverrideChannels)));
			this.OverrideChannelsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideChannelsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 29, true);
			this.OverrideChannelsCheckBox.Name = "OverrideChannelsCheckBox";
			this.OverrideChannelsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 16, true);
			this.OverrideChannelsCheckBox.TabIndex = 5;
			this.OverrideChannelsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChannelByDropEdit
			// 
			this.ChannelByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChannelByDropEdit, "ChannelBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel)(null)).ChannelBy)));
			this.ChannelByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.ChannelByDropEdit.Name = "ChannelByDropEdit";
			this.ChannelByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 17, true);
			this.ChannelByDropEdit.TabIndex = 4;
			// 
			// SortCheckBox
			// 
			this.SortCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SortCheckBox, "SortChannels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBoardSectionChannelsViewModel)(null)).SortChannels)));
			this.SortCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SortCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 29, true);
			this.SortCheckBox.Name = "SortCheckBox";
			this.SortCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 16, true);
			this.SortCheckBox.TabIndex = 7;
			this.SortCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChannelAssignmentControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SortCheckBox);
			this.Controls.Add(this.ShowUnchanneledCheckBox);
			this.Controls.Add(this.OverrideChannelsCheckBox);
			this.Controls.Add(this.ChannelByDropEdit);
			this.Controls.Add(this.AddChannelButton);
			this.Controls.Add(this.ChannelsPanel);
			this.Name = "ChannelAssignmentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZPanel ChannelsPanel;
		public ZArchitecture.GUI.ZButton AddChannelButton;
		private ZArchitecture.GUI.ZCheckBox ShowUnchanneledCheckBox;
		private ZArchitecture.GUI.ZCheckBox OverrideChannelsCheckBox;
		private ZArchitecture.GUI.ZDropEdit ChannelByDropEdit;
		private ZArchitecture.GUI.ZCheckBox SortCheckBox;

	}
}
