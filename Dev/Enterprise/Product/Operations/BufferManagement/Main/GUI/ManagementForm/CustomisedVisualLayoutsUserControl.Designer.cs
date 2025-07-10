namespace Enterprise.BufferManagement.GUI
{
	partial class CustomisedVisualLayoutsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SystemGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SystemCustomisedLayoutsControl = new Enterprise.BufferManagement.GUI.CustomisedLayoutLinksControl();
			this.SystemFieldsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReleaseGroupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReleaseGroupCustomisedLayoutsControl = new Enterprise.BufferManagement.GUI.CustomisedLayoutLinksControl();
			this.ReleaseGroupHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReleaseGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ControlUsageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LayoutUsageSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LayoutsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LayoutUsagePreviewSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LayoutUsageHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LayoutUsageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReleaseGroupLayoutSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SystemGroupBox.SuspendLayout();
			this.SystemCustomisedLayoutsControl.SuspendLayout();
			this.ReleaseGroupGroupBox.SuspendLayout();
			this.ReleaseGroupCustomisedLayoutsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupsGrid)).BeginInit();
			this.ReleaseGroupsGrid.SuspendLayout();
			this.ControlUsageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutUsageSplitContainer)).BeginInit();
			this.LayoutUsageSplitContainer.Panel1.SuspendLayout();
			this.LayoutUsageSplitContainer.Panel2.SuspendLayout();
			this.LayoutUsageSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutsGrid)).BeginInit();
			this.LayoutsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutUsagePreviewSplitContainer)).BeginInit();
			this.LayoutUsagePreviewSplitContainer.Panel1.SuspendLayout();
			this.LayoutUsagePreviewSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutUsageGrid)).BeginInit();
			this.LayoutUsageGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupLayoutSplitContainer)).BeginInit();
			this.ReleaseGroupLayoutSplitContainer.Panel1.SuspendLayout();
			this.ReleaseGroupLayoutSplitContainer.Panel2.SuspendLayout();
			this.ReleaseGroupLayoutSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMSystem);
			// 
			// SystemGroupBox
			// 
			this.SystemGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SystemGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d47ea05c-b617-47d1-8bd9-4fba4ce04341", "System");
			this.SystemGroupBox.Controls.Add(this.SystemCustomisedLayoutsControl);
			this.SystemGroupBox.Controls.Add(this.SystemFieldsHintLabel);
			this.SystemGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SystemGroupBox.Name = "SystemGroupBox";
			this.SystemGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(931, 146, true);
			this.SystemGroupBox.TabIndex = 0;
			this.SystemGroupBox.TabStop = false;
			// 
			// SystemCustomisedLayoutsControl
			// 
			this.SystemCustomisedLayoutsControl.AllowDrop = true;
			this.SystemCustomisedLayoutsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SystemCustomisedLayoutsControl, "CustomisedLayoutLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.BMControlCustomisationLinkCollection)(((Enterprise.BufferManagement.Business.BMSystem)(null)).CustomisedLayoutLinks)));
			this.SystemCustomisedLayoutsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 34, true);
			this.SystemCustomisedLayoutsControl.Name = "SystemCustomisedLayoutsControl";
			this.SystemCustomisedLayoutsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(919, 106, true);
			this.SystemCustomisedLayoutsControl.TabIndex = 5;
			// 
			// SystemFieldsHintLabel
			// 
			this.SystemFieldsHintLabel.AutoSize = true;
			this.SystemFieldsHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ecdcad9f-588c-46e6-96b7-2356781a7e77", "Use these fields to override the default summary and default cards used on Visual Boards throughout this Buffer Management system.");
			this.SystemFieldsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 18, true);
			this.SystemFieldsHintLabel.Name = "SystemFieldsHintLabel";
			this.SystemFieldsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 13, true);
			this.SystemFieldsHintLabel.TabIndex = 4;
			// 
			// ReleaseGroupGroupBox
			// 
			this.ReleaseGroupGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8cef39cc-1b5f-4535-8056-fdfa134ec471", "Release Groups");
			this.ReleaseGroupGroupBox.Controls.Add(this.ReleaseGroupCustomisedLayoutsControl);
			this.ReleaseGroupGroupBox.Controls.Add(this.ReleaseGroupHintLabel);
			this.ReleaseGroupGroupBox.Controls.Add(this.ReleaseGroupsGrid);
			this.ReleaseGroupGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReleaseGroupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReleaseGroupGroupBox.Name = "ReleaseGroupGroupBox";
			this.ReleaseGroupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(931, 161, true);
			this.ReleaseGroupGroupBox.TabIndex = 1;
			this.ReleaseGroupGroupBox.TabStop = false;
			// 
			// ReleaseGroupCustomisedLayoutsControl
			// 
			this.ReleaseGroupCustomisedLayoutsControl.AllowDrop = true;
			this.ReleaseGroupCustomisedLayoutsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseGroupCustomisedLayoutsControl, "ReleaseGroups.CustomisedLayoutLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.BMControlCustomisationLinkCollection)(((Enterprise.BufferManagement.Business.BMSystemReleaseGroup)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)).SyncRoot)).CustomisedLayoutLinks)));
			this.ReleaseGroupCustomisedLayoutsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 38, true);
			this.ReleaseGroupCustomisedLayoutsControl.Name = "ReleaseGroupCustomisedLayoutsControl";
			this.ReleaseGroupCustomisedLayoutsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 117, true);
			this.ReleaseGroupCustomisedLayoutsControl.TabIndex = 7;
			// 
			// ReleaseGroupHintLabel
			// 
			this.ReleaseGroupHintLabel.AutoSize = true;
			this.ReleaseGroupHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4ea557ad-d9ad-4883-9c4b-8516c34c99fd", "Use these fields to override the system-wide or the default card layouts for boards with the specified Release Group.");
			this.ReleaseGroupHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 22, true);
			this.ReleaseGroupHintLabel.Name = "ReleaseGroupHintLabel";
			this.ReleaseGroupHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 13, true);
			this.ReleaseGroupHintLabel.TabIndex = 6;
			// 
			// ReleaseGroupsGrid
			// 
			this.ReleaseGroupsGrid.AllowNavigation = false;
			this.ReleaseGroupsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ReleaseGroupsGrid, "ReleaseGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMSystemReleaseGroup)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)).SyncRoot)).ReleaseGroupDesc)));
			this.ReleaseGroupsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ReleaseGroupDesc";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(221);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReleaseGroupsGrid.CopySelectedRowsAllowed = true;
			this.ReleaseGroupsGrid.GridId = "3750aad3-a23e-4dc9-89ae-7718113f4b6f";
			this.ReleaseGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseGroupsGrid.LayoutKey = "ReleaseGroupsGrid";
			this.ReleaseGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ReleaseGroupsGrid.Name = "ReleaseGroupsGrid";
			this.ReleaseGroupsGrid.ReadOnly = true;
			this.ReleaseGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 136, true);
			this.ReleaseGroupsGrid.TabIndex = 0;
			// 
			// ControlUsageGroupBox
			// 
			this.ControlUsageGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b3ea0031-fa6c-47f4-8917-a8f90faa6dda", "Layout Usage");
			this.ControlUsageGroupBox.Controls.Add(this.LayoutUsageSplitContainer);
			this.ControlUsageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ControlUsageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ControlUsageGroupBox.Name = "ControlUsageGroupBox";
			this.ControlUsageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(931, 222, true);
			this.ControlUsageGroupBox.TabIndex = 2;
			this.ControlUsageGroupBox.TabStop = false;
			// 
			// LayoutUsageSplitContainer
			// 
			this.LayoutUsageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LayoutUsageSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LayoutUsageSplitContainer.Name = "LayoutUsageSplitContainer";
			// 
			// LayoutUsageSplitContainer.Panel1
			// 
			this.LayoutUsageSplitContainer.Panel1.Controls.Add(this.LayoutsGrid);
			// 
			// LayoutUsageSplitContainer.Panel2
			// 
			this.LayoutUsageSplitContainer.Panel2.Controls.Add(this.LayoutUsagePreviewSplitContainer);
			this.LayoutUsageSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 203, true);
			this.LayoutUsageSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(264);
			this.LayoutUsageSplitContainer.TabIndex = 4;
			// 
			// LayoutsGrid
			// 
			this.LayoutsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LayoutsGrid, "CustomisedControls");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).CustomisedControls)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMControlCustomisation)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).CustomisedControls)).SyncRoot)).FM_ControlType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMControlCustomisation)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).CustomisedControls)).SyncRoot)).FM_Name)));
			this.LayoutsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "FM_ControlType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "FM_Name";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(171);
			this.LayoutsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LayoutsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LayoutsGrid.CopySelectedRowsAllowed = true;
			this.LayoutsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LayoutsGrid.GridId = "3750aad3-a23e-4dc9-89ae-7718113f4b6f";
			this.LayoutsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LayoutsGrid.LayoutKey = "ReleaseGroupsGrid";
			this.LayoutsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LayoutsGrid.Name = "LayoutsGrid";
			this.LayoutsGrid.ReadOnly = true;
			this.LayoutsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 203, true);
			this.LayoutsGrid.TabIndex = 1;
			this.LayoutsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LayoutsGrid_DoubleClick);
			// 
			// LayoutUsagePreviewSplitContainer
			// 
			this.LayoutUsagePreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LayoutUsagePreviewSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LayoutUsagePreviewSplitContainer.Name = "LayoutUsagePreviewSplitContainer";
			// 
			// LayoutUsagePreviewSplitContainer.Panel1
			// 
			this.LayoutUsagePreviewSplitContainer.Panel1.Controls.Add(this.LayoutUsageHintLabel);
			this.LayoutUsagePreviewSplitContainer.Panel1.Controls.Add(this.LayoutUsageGrid);
			this.LayoutUsagePreviewSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 203, true);
			this.LayoutUsagePreviewSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			this.LayoutUsagePreviewSplitContainer.TabIndex = 3;
			// 
			// LayoutUsageHintLabel
			// 
			this.LayoutUsageHintLabel.AutoSize = true;
			this.LayoutUsageHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("579cee13-c2e6-4fe5-b417-2c3426f989c0", "The selected layout is used in the below places:");
			this.LayoutUsageHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 5, true);
			this.LayoutUsageHintLabel.Name = "LayoutUsageHintLabel";
			this.LayoutUsageHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 13, true);
			this.LayoutUsageHintLabel.TabIndex = 7;
			// 
			// LayoutUsageGrid
			// 
			this.LayoutUsageGrid.AllowNavigation = false;
			this.LayoutUsageGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LayoutUsageGrid, "CustomisedControls.ControlUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMControlCustomisation)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).CustomisedControls)).SyncRoot)).ControlUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMControlCustomisationLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMControlCustomisation)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).CustomisedControls)).SyncRoot)).ControlUsages)).SyncRoot)).UsageDescription)));
			this.LayoutUsageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "UsageDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.LayoutUsageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LayoutUsageGrid.CopySelectedRowsAllowed = true;
			this.LayoutUsageGrid.GridId = "3750aad3-a23e-4dc9-89ae-7718113f4b6f";
			this.LayoutUsageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LayoutUsageGrid.LayoutKey = "ReleaseGroupsGrid";
			this.LayoutUsageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
			this.LayoutUsageGrid.Name = "LayoutUsageGrid";
			this.LayoutUsageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 182, true);
			this.LayoutUsageGrid.TabIndex = 2;
			// 
			// ReleaseGroupLayoutSplitContainer
			// 
			this.ReleaseGroupLayoutSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ReleaseGroupLayoutSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 155, true);
			this.ReleaseGroupLayoutSplitContainer.Name = "ReleaseGroupLayoutSplitContainer";
			this.ReleaseGroupLayoutSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ReleaseGroupLayoutSplitContainer.Panel1
			// 
			this.ReleaseGroupLayoutSplitContainer.Panel1.Controls.Add(this.ReleaseGroupGroupBox);
			this.ReleaseGroupLayoutSplitContainer.Panel1MinSize = 125;
			// 
			// ReleaseGroupLayoutSplitContainer.Panel2
			// 
			this.ReleaseGroupLayoutSplitContainer.Panel2.Controls.Add(this.ControlUsageGroupBox);
			this.ReleaseGroupLayoutSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(931, 387, true);
			this.ReleaseGroupLayoutSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			this.ReleaseGroupLayoutSplitContainer.TabIndex = 3;
			// 
			// CustomisedVisualLayoutsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReleaseGroupLayoutSplitContainer);
			this.Controls.Add(this.SystemGroupBox);
			this.Name = "CustomisedVisualLayoutsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(937, 545, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SystemGroupBox.ResumeLayout(false);
			this.SystemGroupBox.PerformLayout();
			this.SystemCustomisedLayoutsControl.ResumeLayout(true);
			this.SystemCustomisedLayoutsControl.PerformLayout();
			this.ReleaseGroupGroupBox.ResumeLayout(false);
			this.ReleaseGroupGroupBox.PerformLayout();
			this.ReleaseGroupCustomisedLayoutsControl.ResumeLayout(true);
			this.ReleaseGroupCustomisedLayoutsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupsGrid)).EndInit();
			this.ReleaseGroupsGrid.ResumeLayout(false);
			this.ReleaseGroupsGrid.PerformLayout();
			this.ControlUsageGroupBox.ResumeLayout(false);
			this.ControlUsageGroupBox.PerformLayout();
			this.LayoutUsageSplitContainer.Panel1.ResumeLayout(false);
			this.LayoutUsageSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LayoutUsageSplitContainer)).EndInit();
			this.LayoutUsageSplitContainer.ResumeLayout(false);
			this.LayoutUsageSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutsGrid)).EndInit();
			this.LayoutsGrid.ResumeLayout(false);
			this.LayoutsGrid.PerformLayout();
			this.LayoutUsagePreviewSplitContainer.Panel1.ResumeLayout(false);
			this.LayoutUsagePreviewSplitContainer.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutUsagePreviewSplitContainer)).EndInit();
			this.LayoutUsagePreviewSplitContainer.ResumeLayout(false);
			this.LayoutUsagePreviewSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayoutUsageGrid)).EndInit();
			this.LayoutUsageGrid.ResumeLayout(false);
			this.LayoutUsageGrid.PerformLayout();
			this.ReleaseGroupLayoutSplitContainer.Panel1.ResumeLayout(false);
			this.ReleaseGroupLayoutSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupLayoutSplitContainer)).EndInit();
			this.ReleaseGroupLayoutSplitContainer.ResumeLayout(false);
			this.ReleaseGroupLayoutSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SystemGroupBox;
		private ZArchitecture.GUI.ZGroupBox ReleaseGroupGroupBox;
		private ZArchitecture.GUI.ZGroupBox ControlUsageGroupBox;
		private CargoWise.Windows.UI.KSplitContainer ReleaseGroupLayoutSplitContainer;
		private ZArchitecture.ZGrid ReleaseGroupsGrid;
		public ZArchitecture.ZGrid LayoutsGrid;
		private ZArchitecture.ZGrid LayoutUsageGrid;
		private CargoWise.Windows.UI.KSplitContainer LayoutUsagePreviewSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer LayoutUsageSplitContainer;
		private ZArchitecture.ZLabel SystemFieldsHintLabel;
		private ZArchitecture.ZLabel ReleaseGroupHintLabel;
		private ZArchitecture.ZLabel LayoutUsageHintLabel;
		private CustomisedLayoutLinksControl SystemCustomisedLayoutsControl;
		private CustomisedLayoutLinksControl ReleaseGroupCustomisedLayoutsControl;
	}
}
