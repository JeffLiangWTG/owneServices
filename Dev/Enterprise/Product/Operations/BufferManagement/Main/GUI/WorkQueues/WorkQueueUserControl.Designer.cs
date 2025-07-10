namespace Enterprise.BufferManagement.GUI
{
	partial class WorkQueueUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkQueueUserControl));
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NudgeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OwnerGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CreatingUserFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VisualisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApplyToBackgroundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ApplyBorderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BorderStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.ColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.MembersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RemoveMemberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddMemberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MembersGrid = new Enterprise.BufferManagement.GUI.TagGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.OwnerGroupFindBox.SuspendLayout();
			this.CreatingUserFindBox.SuspendLayout();
			this.VisualisationGroupBox.SuspendLayout();
			this.BorderStyleDropEdit.SuspendLayout();
			this.ColorDropEdit.SuspendLayout();
			this.MembersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MembersGrid)).BeginInit();
			this.MembersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkQueue);
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "TGM_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).TGM_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 19, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "TGM_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).TGM_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 45, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// NudgeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NudgeCalcEdit, "TGM_NudgeAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).TGM_NudgeAmount)));
			this.NudgeCalcEdit.DecimalPlaces = 0;
			this.NudgeCalcEdit.Decimals = 0;
			this.NudgeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 71, true);
			this.NudgeCalcEdit.Name = "NudgeCalcEdit";
			this.NudgeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.NudgeCalcEdit.TabIndex = 2;
			this.NudgeCalcEdit.Text = "0";
			this.NudgeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActiveCheckBox
			// 
			this.ActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ActiveCheckBox, "TGM_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).TGM_IsActive)));
			this.ActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 21, true);
			this.ActiveCheckBox.Name = "ActiveCheckBox";
			this.ActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ActiveCheckBox.TabIndex = 3;
			this.ActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("370b200a-8475-4d2d-afb5-ab8a6adfc2d7", "Queue Details");
			this.DetailsGroupBox.Controls.Add(this.OwnerGroupFindBox);
			this.DetailsGroupBox.Controls.Add(this.CreatingUserFindBox);
			this.DetailsGroupBox.Controls.Add(this.CodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.ActiveCheckBox);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.NudgeCalcEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 101, true);
			this.DetailsGroupBox.TabIndex = 4;
			this.DetailsGroupBox.TabStop = false;
			// 
			// OwnerGroupFindBox
			// 
			this.OwnerGroupFindBox.AllowDrop = true;
			this.OwnerGroupFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OwnerGroupFindBox, "TGM_GG_OwnerGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).TGM_GG_OwnerGroup)));
			this.OwnerGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 71, true);
			this.OwnerGroupFindBox.Name = "OwnerGroupFindBox";
			this.OwnerGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.OwnerGroupFindBox.TabIndex = 5;
			// 
			// CreatingUserFindBox
			// 
			this.CreatingUserFindBox.AllowDrop = true;
			this.CreatingUserFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CreatingUserFindBox, "TGM_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).TGM_SystemCreateUser)));
			this.CreatingUserFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 18, true);
			this.CreatingUserFindBox.Name = "CreatingUserFindBox";
			this.CreatingUserFindBox.PreBoundMaxLength = 3;
			this.CreatingUserFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.CreatingUserFindBox.TabIndex = 4;
			// 
			// VisualisationGroupBox
			// 
			this.VisualisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.VisualisationGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("5bcd32b3-5c60-4900-b78e-b9902451f22a", "Visualization Details");
			this.VisualisationGroupBox.Controls.Add(this.ApplyToBackgroundCheckBox);
			this.VisualisationGroupBox.Controls.Add(this.ApplyBorderCheckBox);
			this.VisualisationGroupBox.Controls.Add(this.BorderStyleDropEdit);
			this.VisualisationGroupBox.Controls.Add(this.ColorDropEdit);
			this.VisualisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 110, true);
			this.VisualisationGroupBox.Name = "VisualisationGroupBox";
			this.VisualisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 75, true);
			this.VisualisationGroupBox.TabIndex = 5;
			this.VisualisationGroupBox.TabStop = false;
			// 
			// ApplyToBackgroundCheckBox
			// 
			this.ApplyToBackgroundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ApplyToBackgroundCheckBox, "ApplyColorToBackground");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).ApplyColorToBackground)));
			this.ApplyToBackgroundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApplyToBackgroundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 21, true);
			this.ApplyToBackgroundCheckBox.Name = "ApplyToBackgroundCheckBox";
			this.ApplyToBackgroundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ApplyToBackgroundCheckBox.TabIndex = 7;
			this.ApplyToBackgroundCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApplyBorderCheckBox
			// 
			this.ApplyBorderCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ApplyBorderCheckBox, "ApplyColorToBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).ApplyColorToBorder)));
			this.ApplyBorderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApplyBorderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 47, true);
			this.ApplyBorderCheckBox.Name = "ApplyBorderCheckBox";
			this.ApplyBorderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ApplyBorderCheckBox.TabIndex = 8;
			this.ApplyBorderCheckBox.UseVisualStyleBackColor = true;
			// 
			// BorderStyleDropEdit
			// 
			this.BorderStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderStyleDropEdit, "BorderStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).BorderStyle)));
			this.BorderStyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BorderStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 45, true);
			this.BorderStyleDropEdit.Name = "BorderStyleDropEdit";
			this.BorderStyleDropEdit.ShowDescriptionBox = false;
			this.BorderStyleDropEdit.UseFullWidthForCodeBox = true;
			this.BorderStyleDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.BorderStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.BorderStyleDropEdit.TabIndex = 6;
			// 
			// ColorDropEdit
			// 
			this.ColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ColorDropEdit, "Color");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Color)));
			this.ColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 19, true);
			this.ColorDropEdit.Name = "ColorDropEdit";
			this.ColorDropEdit.ShowDescriptionBox = false;
			this.ColorDropEdit.UseFullWidthForCodeBox = true;
			this.ColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ColorDropEdit.TabIndex = 5;
			// 
			// MembersGroupBox
			// 
			this.MembersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MembersGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8e29ec19-ee5c-45dc-8bae-c82bba8a6072", "Queue Members");
			this.MembersGroupBox.Controls.Add(this.RemoveMemberButton);
			this.MembersGroupBox.Controls.Add(this.AddMemberButton);
			this.MembersGroupBox.Controls.Add(this.MembersGrid);
			this.MembersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 191, true);
			this.MembersGroupBox.Name = "MembersGroupBox";
			this.MembersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 204, true);
			this.MembersGroupBox.TabIndex = 6;
			this.MembersGroupBox.TabStop = false;
			// 
			// RemoveMemberButton
			// 
			this.RemoveMemberButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveMemberButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("302d8615-9018-4c52-8e14-2c1a9d1598e4", "Remove");
			this.RemoveMemberButton.Image = ((System.Drawing.Image)(resources.GetObject("RemoveMemberButton.Image")));
			this.RemoveMemberButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.RemoveMemberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 175, true);
			this.RemoveMemberButton.Name = "RemoveMemberButton";
			this.RemoveMemberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.RemoveMemberButton.TabIndex = 6;
			this.RemoveMemberButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RemoveMemberButton.UseVisualStyleBackColor = true;
			this.RemoveMemberButton.Click += new System.EventHandler(this.RemoveMemberButton_Click);
			// 
			// AddMemberButton
			// 
			this.AddMemberButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddMemberButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("a96600c2-c352-4e8c-a615-2810195d43a7", "Add");
			this.AddMemberButton.Image = ((System.Drawing.Image)(resources.GetObject("AddMemberButton.Image")));
			this.AddMemberButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AddMemberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 175, true);
			this.AddMemberButton.Name = "AddMemberButton";
			this.AddMemberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.AddMemberButton.TabIndex = 5;
			this.AddMemberButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AddMemberButton.UseVisualStyleBackColor = true;
			this.AddMemberButton.Click += new System.EventHandler(this.AddMemberButton_Click);
			// 
			// MembersGrid
			// 
			this.MembersGrid.AllowNavigation = false;
			this.MembersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MembersGrid, "Members");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).TGL_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).Parent.ProviderJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).Parent.ProviderJobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).WorkflowDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).Parent.PlannedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).QueueStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).Parent.FH_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).Parent.IsReleased)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).RunningTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).LocalTimeAddedToQueue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).AddedByName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).Parent.CurrentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).StaffAssignedToNextStartableTask)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).CapabilityAssignedToNextStartableTask)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCreatedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCreatedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCriteria1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCriteria2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCriteria3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCriteria4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).JobCriteria5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).DescriptionOfTheFirstOpenWorkflowInTheJob)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkQueueMembershipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkQueue)(null)).Members)).SyncRoot)).CurrentComponentForTheFirstOpenWorkflowInTheJob)));
			this.MembersGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TGL_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo1.ColumnName = "Parent+ProviderJobNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Parent+ProviderJobDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "WorkflowDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "Parent+PlannedDuration";
			zTimeEditExColumnStyleInfo1.IsReadOnly = true;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "QueueStatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "Parent+FH_StatusDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo1.ColumnName = "Parent+IsReleased";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTimeEditExColumnStyleInfo2.AllowNegative = false;
			zTimeEditExColumnStyleInfo2.ColumnName = "RunningTotal";
			zTimeEditExColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "LocalTimeAddedToQueue";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo6.ColumnName = "AddedByName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "Parent+CurrentStatus";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo7.ColumnComparer = null;
			zTextBoxColumnStyleInfo7.ColumnName = "StaffAssignedToNextStartableTask";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo8.ColumnComparer = null;
			zTextBoxColumnStyleInfo8.ColumnName = "CapabilityAssignedToNextStartableTask";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zTextBoxColumnStyleInfo9.ColumnComparer = null;
			zTextBoxColumnStyleInfo9.ColumnName = "JobCreatedBy";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnComparer = null;
			zDateEditColumnStyleInfo2.ColumnName = "JobCreatedDate";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfo10.ColumnComparer = null;
			zTextBoxColumnStyleInfo10.ColumnName = "JobCriteria1";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo11.ColumnComparer = null;
			zTextBoxColumnStyleInfo11.ColumnName = "JobCriteria2";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.ColumnComparer = null;
			zTextBoxColumnStyleInfo12.ColumnName = "JobCriteria3";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo13.ColumnComparer = null;
			zTextBoxColumnStyleInfo13.ColumnName = "JobCriteria4";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo14.ColumnComparer = null;
			zTextBoxColumnStyleInfo14.ColumnName = "JobCriteria5";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo15.ColumnComparer = null;
			zTextBoxColumnStyleInfo15.ColumnName = "DescriptionOfTheFirstOpenWorkflowInTheJob";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo16.ColumnComparer = null;
			zTextBoxColumnStyleInfo16.ColumnName = "CurrentComponentForTheFirstOpenWorkflowInTheJob";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(305);
			this.MembersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MembersGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MembersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MembersGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo2);
			this.MembersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MembersGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MembersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.MembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.MembersGrid.GridId = "1bedff11-6489-4397-8de1-f628b6753ed8";
			this.MembersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MembersGrid.LayoutKey = "MembersGrid";
			this.MembersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.MembersGrid.Name = "MembersGrid";
			this.MembersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 150, true);
			this.MembersGrid.TabIndex = 0;
			this.MembersGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MembersGrid_DoubleClick);
			// 
			// WorkQueueUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MembersGroupBox);
			this.Controls.Add(this.VisualisationGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "WorkQueueUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 398, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.OwnerGroupFindBox.ResumeLayout(true);
			this.OwnerGroupFindBox.PerformLayout();
			this.CreatingUserFindBox.ResumeLayout(true);
			this.CreatingUserFindBox.PerformLayout();
			this.VisualisationGroupBox.ResumeLayout(false);
			this.VisualisationGroupBox.PerformLayout();
			this.BorderStyleDropEdit.ResumeLayout(true);
			this.BorderStyleDropEdit.PerformLayout();
			this.ColorDropEdit.ResumeLayout(true);
			this.ColorDropEdit.PerformLayout();
			this.MembersGroupBox.ResumeLayout(false);
			this.MembersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MembersGrid)).EndInit();
			this.MembersGrid.ResumeLayout(false);
			this.MembersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.ZCalcEdit NudgeCalcEdit;
		private ZArchitecture.GUI.ZCheckBox ActiveCheckBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox VisualisationGroupBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth BorderStyleDropEdit;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth ColorDropEdit;
		private ZArchitecture.GUI.ZCheckBox ApplyBorderCheckBox;
		private ZArchitecture.GUI.ZCheckBox ApplyToBackgroundCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox CreatingUserFindBox;
		private ZArchitecture.GUI.ZGroupBox MembersGroupBox;
		private TagGrid MembersGrid;
		private ZArchitecture.GUI.ZButton AddMemberButton;
		private ZArchitecture.GUI.ZButton RemoveMemberButton;
		private ZArchitecture.GUI.ZGuidFindBox OwnerGroupFindBox;
	}
}
