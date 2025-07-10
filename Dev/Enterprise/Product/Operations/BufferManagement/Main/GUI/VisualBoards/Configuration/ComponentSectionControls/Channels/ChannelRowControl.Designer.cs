namespace Enterprise.BufferManagement.GUI
{
	partial class ChannelRowControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChannelPickerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ChannelTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChannelEntityFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EditPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoardSectionChannel);
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteButton.Image = global::Enterprise.BufferManagement.GUI.Properties.Resources.glyphicons_207_remove_2;
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 2, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.DeleteButton.TabIndex = 22;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// EditPanel
			// 
			this.EditPanel.Controls.Add(this.DescriptionLabel);
			this.EditPanel.Controls.Add(this.ChannelPickerPanel);
			this.EditPanel.Controls.Add(this.ChannelTypeDropEdit);
			this.EditPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.EditPanel.Name = "EditPanel";
			this.EditPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 25, true);
			this.EditPanel.TabIndex = 5;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionLabel, "BizoDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSectionChannel)(null)).BizoDescription)));
			this.DescriptionLabel.Cursor = System.Windows.Forms.Cursors.NoMoveVert;
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 5, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 15, true);
			this.DescriptionLabel.TabIndex = 3;
			// 
			// ChannelPickerPanel
			// 
			this.ChannelPickerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 0, true);
			this.ChannelPickerPanel.Name = "ChannelPickerPanel";
			this.ChannelPickerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 25, true);
			this.ChannelPickerPanel.TabIndex = 2;
			// 
			// ChannelTypeDropEdit
			// 
			this.ChannelTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChannelTypeDropEdit, "MSC_ChannelType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMBoardSectionChannel)(null)).MSC_ChannelType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChannelTypeDropEdit, false);
			this.ChannelTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.ChannelTypeDropEdit.Name = "ChannelTypeDropEdit";
			this.ChannelTypeDropEdit.PreBoundMaxLength = 3;
			this.ChannelTypeDropEdit.ShowDescriptionBox = false;
			this.ChannelTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ChannelTypeDropEdit.TabIndex = 1;
			// 
			// ChannelEntityFindBox
			// 
			this.ChannelEntityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChannelEntityFindBox, "MSC_ParentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoardSectionChannel)(null)).MSC_ParentID)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChannelEntityFindBox, false);
			this.ChannelEntityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 64, true);
			this.ChannelEntityFindBox.Name = "ChannelEntityFindBox";
			this.ChannelEntityFindBox.ShowDescriptionBox = false;
			this.ChannelEntityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ChannelEntityFindBox.TabIndex = 10;
			this.ChannelEntityFindBox.Visible = false;
			// 
			// ChannelRowControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChannelEntityFindBox);
			this.Controls.Add(this.EditPanel);
			this.Controls.Add(this.DeleteButton);
			this.Name = "ChannelRowControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 168, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EditPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public ZArchitecture.GUI.ZButton DeleteButton;
		private ZArchitecture.GUI.ZPanel EditPanel;
		private ZArchitecture.GUI.ZDropEdit ChannelTypeDropEdit;
		public ZArchitecture.GUI.ZGuidFindBox ChannelEntityFindBox;
		public ZArchitecture.GUI.ZPanel ChannelPickerPanel;
		public ZArchitecture.ZLabel DescriptionLabel;
	}
}
