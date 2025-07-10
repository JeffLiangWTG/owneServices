using System;

namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLineCopyDocumentsForm
	{
		private new void InitializeComponent()
		{
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyDocumentsLinesGridControl = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CopyDocumentsLinesGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CopyDocumentsLinesGridControl)).BeginInit();
			this.CopyDocumentsLinesGridControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.CopyDocumentsLinesGridGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 701, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CopyDocumentsSelectionHeader);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3fd0780b-7911-4222-a6d7-94c552dc0f45", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(930, 675, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectAllButton.TabIndex = 2;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("27c0f201-16fa-4830-b968-2e8e138b324b", "OK");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1020, 675, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 3;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9f1601a5-f4b2-4220-b050-c9f9e80785cc", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1110, 675, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// CopyDocumentsLinesGridControl
			// 
			this.CopyDocumentsLinesGridControl.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CopyDocumentsLinesGridControl, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CopyDocumentsSelectionHeader)(null)).Lines)));
			this.CopyDocumentsLinesGridControl.CaptionVisible = false;
			this.CopyDocumentsLinesGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CopyDocumentsLinesGridControl.GridId = "0ec04e8d-e96b-4e84-8759-46764cc5cba8";
			this.CopyDocumentsLinesGridControl.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CopyDocumentsLinesGridControl.LayoutKey = "CopyDocumentsLinesGridControl";
			this.CopyDocumentsLinesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CopyDocumentsLinesGridControl.Name = "CopyDocumentsLinesGridControl";
			this.CopyDocumentsLinesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 641, true);
			this.CopyDocumentsLinesGridControl.TabIndex = 1;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.CopyDocumentsLinesGridGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.DynamicLayoutPanel);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1190, 660, true);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(236);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(900);
			this.SplitContainer.TabIndex = 0;
			// 
			// CopyDocumentsLinesGridGroupBox
			// 
			this.CopyDocumentsLinesGridGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("18df21e2-9498-45ea-9bf4-cafacaf7ca1c", "Copy documents:");
			this.CopyDocumentsLinesGridGroupBox.Controls.Add(this.CopyDocumentsLinesGridControl);
			this.CopyDocumentsLinesGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CopyDocumentsLinesGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CopyDocumentsLinesGridGroupBox.Name = "CopyDocumentsLinesGridGroupBox";
			this.CopyDocumentsLinesGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 660, true);
			this.CopyDocumentsLinesGridGroupBox.TabIndex = 2;
			this.CopyDocumentsLinesGridGroupBox.TabStop = false;
			// 
			// DynamicLayoutPanel
			// 
			this.DynamicLayoutPanel.AllowDrop = true;
			this.DynamicLayoutPanel.AutoScroll = true;
			this.DynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicLayoutPanel.Name = "DynamicLayoutPanel";
			this.DynamicLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 660, true);
			this.DynamicLayoutPanel.TabIndex = 1;
			// 
			// InvoiceLineCopyDocumentsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("8da0cba1-6074-4c56-9be5-8b9dd26c54aa", "Copy Documents of Invoice Line");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.SelectAllButton);
			this.Controls.Add(this.SplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CopyDocumentsSelectionHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Name = "InvoiceLineCopyDocumentsForm";
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SelectAllButton, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CopyDocumentsLinesGridControl)).EndInit();
			this.CopyDocumentsLinesGridControl.ResumeLayout(false);
			this.CopyDocumentsLinesGridControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.CopyDocumentsLinesGridGroupBox.ResumeLayout(false);
			this.CopyDocumentsLinesGridGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZButton SelectAllButton;
		internal ZArchitecture.GUI.ZButton OkButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.ZGrid CopyDocumentsLinesGridControl;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicLayoutPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CopyDocumentsLinesGridGroupBox;
	}
}
