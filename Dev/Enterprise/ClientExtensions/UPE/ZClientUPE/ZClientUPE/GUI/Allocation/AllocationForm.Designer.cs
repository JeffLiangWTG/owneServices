using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class AllocationForm : ZChildForm
	{
		private ZDropEdit QueueDropEdit;
		private ZLabel QueueLabel;
		internal ZButton ReAllocateButton;
		private ZLabel ReasonLabel;
		private ZDropEdit ReasonDropEdit;
		private ZGroupBox QueueReasonGroupBox;
		internal ZButton CloseButton;
		private ZPanel BottomPanel;
		private ZGroupBox ClassifierAllocationGroupBox;
		internal ZButton RefreshButton;
		private ZGrid ClassifierAllocationGrid;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.QueueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QueueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReAllocateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QueueReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClassifierAllocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClassifierAllocationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.QueueReasonGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.ClassifierAllocationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClassifierAllocationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 461, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(332);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// QueueDropEdit
			// 
			this.QueueDropEdit.BindTo = "Queue";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Allocation)(null)).QueueInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Allocation)(null)).Queue)));
			this.QueueDropEdit.BindToList = "QueueList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Allocation)(null)).QueueList)));
			this.QueueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 27, true);
			this.QueueDropEdit.Name = "QueueDropEdit";
			this.QueueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.QueueDropEdit.TabIndex = 1;
			// 
			// QueueLabel
			// 
			this.QueueLabel.AutoSize = true;
			this.QueueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 27, true);
			this.QueueLabel.Name = "QueueLabel";
			this.QueueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 14, true);
			this.QueueLabel.TabIndex = 4;
			this.QueueLabel.Text = "Queue:";
			// 
			// ReAllocateButton
			// 
			this.ReAllocateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReAllocateButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReAllocateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 6, true);
			this.ReAllocateButton.Name = "ReAllocateButton";
			this.ReAllocateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ReAllocateButton.TabIndex = 5;
			this.ReAllocateButton.Text = "Reallocate";
			this.ReAllocateButton.UseVisualStyleBackColor = true;
			this.ReAllocateButton.Click += new System.EventHandler(this.ReAllocateButton_Click);
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.AutoSize = true;
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 53, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 14, true);
			this.ReasonLabel.TabIndex = 7;
			this.ReasonLabel.Text = "Reason:";
			// 
			// ReasonDropEdit
			// 
			this.ReasonDropEdit.BindTo = "Reason";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Allocation)(null)).ReasonInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Allocation)(null)).Reason)));
			this.ReasonDropEdit.BindToList = "ReasonList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Allocation)(null)).ReasonList)));
			this.ReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 50, true);
			this.ReasonDropEdit.Name = "ReasonDropEdit";
			this.ReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ReasonDropEdit.TabIndex = 6;
			// 
			// QueueReasonGroupBox
			// 
			this.QueueReasonGroupBox.Controls.Add(this.QueueLabel);
			this.QueueReasonGroupBox.Controls.Add(this.ReasonLabel);
			this.QueueReasonGroupBox.Controls.Add(this.QueueDropEdit);
			this.QueueReasonGroupBox.Controls.Add(this.ReasonDropEdit);
			this.QueueReasonGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.QueueReasonGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QueueReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QueueReasonGroupBox.Name = "QueueReasonGroupBox";
			this.QueueReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 100, true);
			this.QueueReasonGroupBox.TabIndex = 8;
			this.QueueReasonGroupBox.TabStop = false;
			this.QueueReasonGroupBox.Text = "Queue/Reason Selection";
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 6, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ReAllocateButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 423, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 38, true);
			this.BottomPanel.TabIndex = 10;
			// 
			// ClassifierAllocationGroupBox
			// 
			this.ClassifierAllocationGroupBox.Controls.Add(this.RefreshButton);
			this.ClassifierAllocationGroupBox.Controls.Add(this.ClassifierAllocationGrid);
			this.ClassifierAllocationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClassifierAllocationGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClassifierAllocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.ClassifierAllocationGroupBox.Name = "ClassifierAllocationGroupBox";
			this.ClassifierAllocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 323, true);
			this.ClassifierAllocationGroupBox.TabIndex = 11;
			this.ClassifierAllocationGroupBox.TabStop = false;
			this.ClassifierAllocationGroupBox.Text = "Classifier Allocation";
			// 
			// RefreshButton
			// 
			this.RefreshButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 25, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RefreshButton.TabIndex = 0;
			this.RefreshButton.Text = "&Refresh";
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// ClassifierAllocationGrid
			// 
			this.ClassifierAllocationGrid.AllowNavigation = false;
			this.ClassifierAllocationGrid.BindTo = "ClassifierAllocationList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)));
			this.ClassifierAllocationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Allocated To";
			zTextBoxColumnStyleInfo1.ColumnName = "AllocatedTo";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Allocated To Full Name";
			zTextBoxColumnStyleInfo2.ColumnName = "AllocatedToFullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Number Allocated";
			zCalcEditColumnStyleInfo1.ColumnName = "NumberAllocated";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo1.Caption = "Next Working Day";
			zDateEditColumnStyleInfo1.ColumnName = "NextWorkingDay";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.Caption = "Include For Allocation";
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeForAllocation";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.ClassifierAllocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ClassifierAllocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ClassifierAllocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ClassifierAllocationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ClassifierAllocationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ClassifierAllocationGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ClassifierAllocationGrid.EnableToolTips = false;
			this.ClassifierAllocationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClassifierAllocationGrid.LayoutKey = "zGrid1";
			this.ClassifierAllocationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 64, true);
			this.ClassifierAllocationGrid.Name = "ClassifierAllocationGrid";
			this.ClassifierAllocationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 256, true);
			this.ClassifierAllocationGrid.TabIndex = 2;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).AllocatedToInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).AllocatedTo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).AllocatedToFullNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).AllocatedToFullName)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).NumberAllocated)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).NumberAllocatedInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).NextWorkingDay)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).NextWorkingDayInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).IncludeForAllocation)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ClassifierAllocation)(((object)(((Enterprise.Client.UPE.Business.Allocation)(null)).ClassifierAllocationList)))).IncludeForAllocationInfo)));
			// 
			// AllocationForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 485, true);
			this.Controls.Add(this.ClassifierAllocationGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.QueueReasonGroupBox);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.Allocation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AllocationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.QueueReasonGroupBox, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ClassifierAllocationGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.QueueReasonGroupBox.ResumeLayout(false);
			this.QueueReasonGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.ClassifierAllocationGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ClassifierAllocationGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
