namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class AssignIncidentPopupForm
	{
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox StaffMemberFindbox;
		protected Enterprise.ZArchitecture.ZTextBox zTextBox1;
		protected Enterprise.ZArchitecture.ZLabel StaffMemberLabel;
		protected Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton AssignRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton AssignSelfRadioButton;

		protected override void InitializeComponent()
		{
			this.StaffMemberFindbox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.StaffMemberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.AssignRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AssignSelfRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAssignStaffAction);
			//
			// MessageLabel
			//
			this.MessageLabel.Text = "Please specify who you wish to assign this incident to, and enter a short, intern" +
	"al comment for the eConversation. ";
			//
			// CloseButton
			//
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 265, true);
			this.CloseButton.TabIndex = 7;
			//
			// CancelButtonX
			//
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 265, true);
			this.CancelButtonX.TabIndex = 8;
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 295, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 24, true);
			this.MainStatusBar.TabIndex = 9;
			//
			// StaffMemberFindbox
			//
			this.StaffMemberFindbox.AllowDrop = true;
			this.StaffMemberFindbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StaffMemberFindbox, "StaffPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAssignStaffAction)(null)).StaffPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAssignStaffAction)(null)).AllStaffList)));
			this.StaffMemberFindbox.BindToList = "AllStaffList";
			this.StaffMemberFindbox.CaptionResourceString = null;
			this.StaffMemberFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 80, true);
			this.StaffMemberFindbox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.StaffMemberFindbox.Name = "StaffMemberFindbox";
			this.StaffMemberFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 20, true);
			this.StaffMemberFindbox.TabIndex = 4;
			//
			// zTextBox1
			//
			this.zTextBox1.AcceptsReturn = true;
			this.zTextBox1.AcceptsTab = true;
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAssignStaffAction)(null)).Comment)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 132, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 127, true);
			this.zTextBox1.TabIndex = 6;
			//
			// StaffMemberLabel
			//
			this.StaffMemberLabel.AutoSize = true;
			this.StaffMemberLabel.CaptionResourceString = null;
			this.StaffMemberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 84, true);
			this.StaffMemberLabel.Name = "StaffMemberLabel";
			this.StaffMemberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.StaffMemberLabel.TabIndex = 3;
			this.StaffMemberLabel.Text = "Staff Member:";
			//
			// zLabel2
			//
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = null;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 111, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 13, true);
			this.zLabel2.TabIndex = 5;
			this.zLabel2.Text = "Internal Comment (not sent to client)";
			//
			// AssignRadioButton
			//
			this.AssignRadioButton.AutoCheck = false;
			this.AssignRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AssignRadioButton, "AssignToOther");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAssignStaffAction)(null)).AssignToOther)));
			this.AssignRadioButton.CaptionResourceString = null;
			this.AssignRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AssignRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 54, true);
			this.AssignRadioButton.Name = "AssignRadioButton";
			this.AssignRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.AssignRadioButton.TabIndex = 1;
			this.AssignRadioButton.TabStop = true;
			this.AssignRadioButton.Text = "Assign";
			this.AssignRadioButton.UseVisualStyleBackColor = true;
			//
			// AssignSelfRadioButton
			//
			this.AssignSelfRadioButton.AutoCheck = false;
			this.AssignSelfRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AssignSelfRadioButton, "AssignToSelf");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAssignStaffAction)(null)).AssignToSelf)));
			this.AssignSelfRadioButton.CaptionResourceString = null;
			this.AssignSelfRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AssignSelfRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 54, true);
			this.AssignSelfRadioButton.Name = "AssignSelfRadioButton";
			this.AssignSelfRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.AssignSelfRadioButton.TabIndex = 2;
			this.AssignSelfRadioButton.TabStop = true;
			this.AssignSelfRadioButton.Text = "Assign to Self";
			this.AssignSelfRadioButton.UseVisualStyleBackColor = true;
			//
			// AssignIncidentPopupForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 319, true);
			this.Controls.Add(this.AssignSelfRadioButton);
			this.Controls.Add(this.AssignRadioButton);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.StaffMemberLabel);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.StaffMemberFindbox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "AssignIncidentPopupForm";
			this.Text = "Assign Incident";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.StaffMemberFindbox, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.StaffMemberLabel, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.AssignRadioButton, 0);
			this.Controls.SetChildIndex(this.AssignSelfRadioButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
