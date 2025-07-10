#if DEBUG

namespace Enterprise.ZArchitecture.GUI
{
	partial class InfoDiggerForm
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
		public new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zTreeView1 = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.OpenGuiSlnButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.propertiesGridView = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OpenBusSlnButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.performLayoutButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.invalidateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.propertiesGridView)).BeginInit();
			this.propertiesGridView.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 458, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger);
			// 
			// zTreeView1
			// 
			this.zTreeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTreeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.zTreeView1.HideSelection = false;
			this.zTreeView1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTreeView1.Name = "zTreeView1";
			this.zTreeView1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 432, true);
			this.zTreeView1.TabIndex = 0;
			this.zTreeView1.TreeViewSearcher = null;
			this.zTreeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterExpand);
			this.zTreeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterSelect);
			// 
			// OpenGuiSlnButton
			// 
			this.OpenGuiSlnButton.IsCaptionOverridden = true;
			this.OpenGuiSlnButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
			this.OpenGuiSlnButton.Name = "OpenGuiSlnButton";
			this.OpenGuiSlnButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenGuiSlnButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 22, true);
			this.OpenGuiSlnButton.TabIndex = 5;
			this.OpenGuiSlnButton.Text = "GUI Solution";
			this.OpenGuiSlnButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OpenGuiSlnButton.ToolTipCaption = null;
			this.OpenGuiSlnButton.Click += new System.EventHandler(this.OpenGuiSlnButton_Click);
			this.OpenGuiSlnButton.MouseHover += new System.EventHandler(this.OpenGuiSlnButton_Hover);
			// 
			// propertiesGridView
			// 
			this.propertiesGridView.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.propertiesGridView, "PropertiesValues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger)(null)).PropertiesValues)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Internal.ControlProperty)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger)(null)).PropertiesValues)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Internal.ControlProperty)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger)(null)).PropertiesValues)).SyncRoot)).Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.ControlProperty)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger)(null)).PropertiesValues)).SyncRoot)).ValueList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Internal.ControlProperty)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger)(null)).PropertiesValues)).SyncRoot)).PropertyType)));
			this.propertiesGridView.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.BindToList = "ValueList";
			zDropEditColumnStyleInfo1.ColumnName = "Value";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "PropertyType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.propertiesGridView.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.propertiesGridView.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.propertiesGridView.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.propertiesGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertiesGridView.GridId = "5fe58799-be17-4693-991e-814ee413029e";
			this.propertiesGridView.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.propertiesGridView.LayoutKey = "propertiesGridView";
			this.propertiesGridView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.propertiesGridView.Name = "propertiesGridView";
			this.propertiesGridView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 432, true);
			this.propertiesGridView.TabIndex = 1;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.zTreeView1);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.propertiesGridView);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 432, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(345);
			this.SplitContainer.TabIndex = 3;
			// 
			// OpenBusSlnButton
			// 
			this.OpenBusSlnButton.IsCaptionOverridden = true;
			this.OpenBusSlnButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 2, true);
			this.OpenBusSlnButton.Name = "OpenBusSlnButton";
			this.OpenBusSlnButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenBusSlnButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.OpenBusSlnButton.TabIndex = 6;
			this.OpenBusSlnButton.Text = "Business Solution";
			this.OpenBusSlnButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OpenBusSlnButton.ToolTipCaption = null;
			this.OpenBusSlnButton.Click += new System.EventHandler(this.OpenBusSlnButton_Click);
			this.OpenBusSlnButton.MouseHover += new System.EventHandler(this.OpenBusSlnButton_Hover);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.performLayoutButton);
			this.bottomPanel.Controls.Add(this.invalidateButton);
			this.bottomPanel.Controls.Add(this.OpenBusSlnButton);
			this.bottomPanel.Controls.Add(this.OpenGuiSlnButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 432, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 26, true);
			this.bottomPanel.TabIndex = 7;
			// 
			// performLayoutButton
			// 
			this.performLayoutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.performLayoutButton.IsCaptionOverridden = true;
			this.performLayoutButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(670, 2, true);
			this.performLayoutButton.Name = "performLayoutButton";
			this.performLayoutButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.performLayoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.performLayoutButton.TabIndex = 7;
			this.performLayoutButton.Text = "PerformLayout";
			this.performLayoutButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.performLayoutButton.ToolTipCaption = null;
			this.performLayoutButton.Click += new System.EventHandler(this.PerformLayoutButton_Click);
			// 
			// invalidateButton
			// 
			this.invalidateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.invalidateButton.IsCaptionOverridden = true;
			this.invalidateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(772, 2, true);
			this.invalidateButton.Name = "invalidateButton";
			this.invalidateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.invalidateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.invalidateButton.TabIndex = 8;
			this.invalidateButton.Text = "Invalidate";
			this.invalidateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.invalidateButton.ToolTipCaption = null;
			this.invalidateButton.Click += new System.EventHandler(this.InvalidateButton_Click);
			// 
			// InfoDiggerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 482, true);
			this.Controls.Add(this.SplitContainer);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.ControlInformationDigger);
			this.Name = "InfoDiggerForm";
			this.Text = "InfoDigger";
#if !WINZOR
			this.TopMost = true;
#endif
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.propertiesGridView)).EndInit();
			this.propertiesGridView.ResumeLayout(false);
			this.propertiesGridView.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

#endregion

		protected ZButton OpenGuiSlnButton;
		protected ZButton OpenBusSlnButton;
		private ZPanel bottomPanel;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.ZGrid propertiesGridView;
		private ZTreeView zTreeView1;
		protected ZButton invalidateButton;
		protected ZButton performLayoutButton;
	}
}

#endif
