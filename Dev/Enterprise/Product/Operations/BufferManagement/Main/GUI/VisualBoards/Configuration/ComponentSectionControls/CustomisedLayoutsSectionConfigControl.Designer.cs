namespace Enterprise.BufferManagement.GUI
{
	partial class CustomisedLayoutsSectionConfigControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LayoutsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomisedLayoutsControl = new Enterprise.BufferManagement.GUI.CustomisedLayoutLinksControl();
			this.LayoutsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ApplicableLayoutsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RelevantLayoutsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RelevantLayoutsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LayoutsGroupBox.SuspendLayout();
			this.CustomisedLayoutsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.RelevantLayoutsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelevantLayoutsGrid)).BeginInit();
			this.RelevantLayoutsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// LayoutsGroupBox
			// 
			this.LayoutsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("571960ad-d817-47a0-98f1-e06fb3178738", "Customized Visual Layouts");
			this.LayoutsGroupBox.Controls.Add(this.CustomisedLayoutsControl);
			this.LayoutsGroupBox.Controls.Add(this.LayoutsHintLabel);
			this.LayoutsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LayoutsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LayoutsGroupBox.Name = "LayoutsGroupBox";
			this.LayoutsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 262, true);
			this.LayoutsGroupBox.TabIndex = 0;
			this.LayoutsGroupBox.TabStop = false;
			// 
			// CustomisedLayoutsControl
			// 
			this.CustomisedLayoutsControl.AllowDrop = true;
			this.CustomisedLayoutsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomisedLayoutsControl, "CustomisedLayoutLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.BMControlCustomisationLinkCollection)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).CustomisedLayoutLinks)));
			this.CustomisedLayoutsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 59, true);
			this.CustomisedLayoutsControl.Name = "CustomisedLayoutsControl";
			this.CustomisedLayoutsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 197, true);
			this.CustomisedLayoutsControl.TabIndex = 1;
			// 
			// LayoutsHintLabel
			// 
			this.LayoutsHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LayoutsHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("72c2303a-73f8-4ef4-9f8f-73c23e9e049d", "Specify the customized layouts that will be used for graphical elements on this section of the board. The layouts will replace those defined on the board, Release Group, Buffer Management System or the system-defined layouts for the relevant graphical element.");
			this.LayoutsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.LayoutsHintLabel.Name = "LayoutsHintLabel";
			this.LayoutsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 40, true);
			this.LayoutsHintLabel.TabIndex = 0;
			// 
			// ApplicableLayoutsHintLabel
			// 
			this.ApplicableLayoutsHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ApplicableLayoutsHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4684b14e-9872-4ba4-b9be-e6efab988680", "The layouts that will be used when drawing tickets on this board section are listed below. Double-click a row to open the layout.");
			this.ApplicableLayoutsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ApplicableLayoutsHintLabel.Name = "ApplicableLayoutsHintLabel";
			this.ApplicableLayoutsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 34, true);
			this.ApplicableLayoutsHintLabel.TabIndex = 2;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.LayoutsGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.RelevantLayoutsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 451, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(262);
			this.SplitContainer.TabIndex = 3;
			// 
			// RelevantLayoutsGroupBox
			// 
			this.RelevantLayoutsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("edf5682e-5c35-47f1-9f1e-dea169d5626a", "Layouts used by this Section");
			this.RelevantLayoutsGroupBox.Controls.Add(this.RelevantLayoutsGrid);
			this.RelevantLayoutsGroupBox.Controls.Add(this.ApplicableLayoutsHintLabel);
			this.RelevantLayoutsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelevantLayoutsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelevantLayoutsGroupBox.Name = "RelevantLayoutsGroupBox";
			this.RelevantLayoutsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 185, true);
			this.RelevantLayoutsGroupBox.TabIndex = 3;
			this.RelevantLayoutsGroupBox.TabStop = false;
			// 
			// RelevantLayoutsGrid
			// 
			this.RelevantLayoutsGrid.AllowNavigation = false;
			this.RelevantLayoutsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RelevantLayoutsGrid, "ApplicableLayouts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ApplicableLayouts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ApplicableCustomisedLayout)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ApplicableLayouts)).SyncRoot)).LayoutName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ApplicableCustomisedLayout)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ApplicableLayouts)).SyncRoot)).Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ApplicableCustomisedLayout)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ApplicableLayouts)).SyncRoot)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ApplicableCustomisedLayout)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ApplicableLayouts)).SyncRoot)).ControlType)));
			this.RelevantLayoutsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "LayoutName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "Source";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "JobType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "ControlType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RelevantLayoutsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelevantLayoutsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelevantLayoutsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelevantLayoutsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelevantLayoutsGrid.CopySelectedRowsAllowed = true;
			this.RelevantLayoutsGrid.GridId = "6beec191-dbae-43ce-8f16-2b877ca997ac";
			this.RelevantLayoutsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelevantLayoutsGrid.LayoutKey = "RelevantLayoutsGrid";
			this.RelevantLayoutsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 53, true);
			this.RelevantLayoutsGrid.Name = "RelevantLayoutsGrid";
			this.RelevantLayoutsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 126, true);
			this.RelevantLayoutsGrid.TabIndex = 3;
			this.RelevantLayoutsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RelevantLayoutsGrid_MouseDoubleClick);
			// 
			// CustomisedLayoutsSectionConfigControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "CustomisedLayoutsSectionConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 451, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LayoutsGroupBox.ResumeLayout(false);
			this.LayoutsGroupBox.PerformLayout();
			this.CustomisedLayoutsControl.ResumeLayout(true);
			this.CustomisedLayoutsControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.RelevantLayoutsGroupBox.ResumeLayout(false);
			this.RelevantLayoutsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelevantLayoutsGrid)).EndInit();
			this.RelevantLayoutsGrid.ResumeLayout(false);
			this.RelevantLayoutsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LayoutsGroupBox;
		private ZArchitecture.ZLabel LayoutsHintLabel;
		private CustomisedLayoutLinksControl CustomisedLayoutsControl;
		private ZArchitecture.ZLabel ApplicableLayoutsHintLabel;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZGroupBox RelevantLayoutsGroupBox;
		private ZArchitecture.ZGrid RelevantLayoutsGrid;
	}
}
