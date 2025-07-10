namespace Enterprise.BufferManagement.GUI
{
	partial class FilterStripsForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		public new void InitializeComponent()
		{
			this.ApplyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.workflowFilterStrips = new Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.workflowFiltersTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.taskFiltersTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.taskFilterStrips = new Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.workflowFiltersTab.SuspendLayout();
			this.taskFiltersTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.StmModuleFilterViewModel);
			// 
			// ApplyButton
			// 
			this.ApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApplyButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e735a76a-1526-4fbc-b71c-e469d511626b", "Apply");
			this.ApplyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 241, true);
			this.ApplyButton.Name = "ApplyButton";
			this.ApplyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApplyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ApplyButton.TabIndex = 3;
			this.ApplyButton.ToolTipCaption = null;
			this.ApplyButton.UseVisualStyleBackColor = true;
			this.ApplyButton.Click += new System.EventHandler(this.fApplyButton_Click);
			// 
			// zCancelButton
			// 
			this.zCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zCancelButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6c7784b3-d709-4b03-ab04-f85f109332ab", "Cancel");
			this.zCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 241, true);
			this.zCancelButton.Name = "zCancelButton";
			this.zCancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zCancelButton.TabIndex = 4;
			this.zCancelButton.ToolTipCaption = null;
			this.zCancelButton.UseVisualStyleBackColor = true;
			// 
			// zHintLabel
			// 
			this.zHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("cef2c64d-ffc6-4afb-917b-da683dcef525", "Enter conditions to filter the tasks that will be shown on the board section.");
			this.zHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 4, true);
			this.zHintLabel.Name = "zHintLabel";
			this.zHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 20, true);
			this.zHintLabel.TabIndex = 2;
			// 
			// workflowFilterStrips
			// 
			this.workflowFilterStrips.AllowDrop = true;
			this.workflowFilterStrips.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.workflowFilterStrips.AutoScroll = true;
			this.workflowFilterStrips.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.workflowFilterStrips, "WorkflowFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.StmModuleFilterViewModel)(null)).WorkflowFilter)));
			this.workflowFilterStrips.FilterControlIdentifier = null;
			this.workflowFilterStrips.IsPreviewAllowed = false;
			this.workflowFilterStrips.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.workflowFilterStrips.Name = "workflowFilterStrips";
			this.workflowFilterStrips.ReadOnly = false;
			this.workflowFilterStrips.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 170, true);
			this.workflowFilterStrips.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.workflowFiltersTab);
			this.zTabControl1.Controls.Add(this.taskFiltersTab);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 223, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// workflowFiltersTab
			// 
			this.workflowFiltersTab.BackColor = System.Drawing.Color.Transparent;
			this.workflowFiltersTab.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("fba06011-6ee2-4def-bc70-8de67851b07e", "Workflow Filters");
			this.workflowFiltersTab.Controls.Add(this.zLabel1);
			this.workflowFiltersTab.Controls.Add(this.workflowFilterStrips);
			this.workflowFiltersTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.workflowFiltersTab.Name = "workflowFiltersTab";
			this.workflowFiltersTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.workflowFiltersTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 201, true);
			this.workflowFiltersTab.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ec71317f-5220-4b7a-a990-56a045226858", "Enter conditions to filter the workflows that will be shown on the board section.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 23, true);
			this.zLabel1.TabIndex = 4;
			// 
			// taskFiltersTab
			// 
			this.taskFiltersTab.BackColor = System.Drawing.Color.Transparent;
			this.taskFiltersTab.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0665bd71-5ac2-4cf3-8cd8-99a53452cfe5", "Task Filters");
			this.taskFiltersTab.Controls.Add(this.zHintLabel);
			this.taskFiltersTab.Controls.Add(this.taskFilterStrips);
			this.taskFiltersTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.taskFiltersTab.Name = "taskFiltersTab";
			this.taskFiltersTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.taskFiltersTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 201, true);
			this.taskFiltersTab.TabIndex = 1;
			// 
			// taskFilterStrips
			// 
			this.taskFilterStrips.AllowDrop = true;
			this.taskFilterStrips.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.taskFilterStrips.AutoScroll = true;
			this.taskFilterStrips.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.taskFilterStrips, "TaskFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.StmModuleFilterViewModel)(null)).TaskFilter)));
			this.taskFilterStrips.FilterControlIdentifier = null;
			this.taskFilterStrips.IsPreviewAllowed = false;
			this.taskFilterStrips.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.taskFilterStrips.Name = "taskFilterStrips";
			this.taskFilterStrips.ReadOnly = false;
			this.taskFilterStrips.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 170, true);
			this.taskFilterStrips.TabIndex = 2;
			// 
			// FilterStripsForm
			// 
			this.AcceptButton = this.ApplyButton;
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.zCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("96221FFA-3CF4-4EE8-B98B-641532102B21", "Board Filters");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 299, true);
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.zCancelButton);
			this.Controls.Add(this.ApplyButton);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.StmModuleFilterViewModel);
			this.DoubleBuffered = true;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 337, true);
			this.Name = "FilterStripsForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ApplyButton, 0);
			this.Controls.SetChildIndex(this.zCancelButton, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.workflowFiltersTab.ResumeLayout(false);
			this.workflowFiltersTab.PerformLayout();
			this.taskFiltersTab.ResumeLayout(false);
			this.taskFiltersTab.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BMFilterStripWrapperControl workflowFilterStrips;
		public ZArchitecture.GUI.ZButton ApplyButton;
		private ZArchitecture.GUI.ZButton zCancelButton;
		private ZArchitecture.ZLabel zHintLabel;
		private ZArchitecture.GUI.ZTabControl zTabControl1;
		private ZArchitecture.GUI.ZTabPage workflowFiltersTab;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZTabPage taskFiltersTab;
		private BMFilterStripWrapperControl taskFilterStrips;
	}
}