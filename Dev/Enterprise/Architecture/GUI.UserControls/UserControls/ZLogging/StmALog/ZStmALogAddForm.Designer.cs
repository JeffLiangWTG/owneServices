using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmALogAddForm
	{
		#region Windows Form Designer generated code

		CargoWise.Windows.UI.KFlowLayoutPanel FlowLayoutPanel;
		ZButton AddButton;
		ZStmALogAddUserControl EventAddUserControl;
		ZButton CancelAddButton;
		System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.FlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.AddButton = new ZButton();
			this.EventAddUserControl = new ZStmALogAddUserControl();
			this.CancelAddButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(183);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(183);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(StmALogAsAddedByUser);
			// 
			// FlowLayoutPanel
			// 
			this.FlowLayoutPanel.AutoSize = true;
			this.FlowLayoutPanel.Controls.Add(this.CancelAddButton);
			this.FlowLayoutPanel.Controls.Add(this.AddButton);
			this.FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.FlowLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 12, 4);
			this.FlowLayoutPanel.Name = "FlowLayoutPanel";
			// 
			// AddButton
			// 
			this.AddButton.AutoSize = true;
			this.AddButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogAddForm|73d7ec0a-952c-4907-bc30-44b7b8a802e3", "&Add Event");
			this.AddButton.Name = "AddButton";
			this.AddButton.TabIndex = 6;
			this.AddButton.Click += new EventHandler(this.AddButton_Click);
			// 
			// CancelAddButton
			// 
			this.CancelAddButton.AutoSize = true;
			this.CancelAddButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogAddForm|a01613a5-862e-4ebe-aca3-caa4aa2f6831", "&Cancel");
			this.CancelAddButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAddButton.Name = "CancelAddButton";
			this.CancelAddButton.TabIndex = 7;
			this.CancelAddButton.Click += new EventHandler(this.CancelAddButton_Click);
			// 
			// EventAddUserControl
			// 
			this.BindingSource.SetBindingMember(this.EventAddUserControl, ".");
			this.EventAddUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 8, true);
			this.EventAddUserControl.Name = "EventAddUserControl";
			this.EventAddUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 92, true);
			this.EventAddUserControl.TabIndex = 1;
			// 
			// ZStmALogAddForm
			// 
			this.AcceptButton = this.AddButton;

			this.CancelButton = this.CancelAddButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogAddForm|c29613be-3437-480b-9ddb-e61ce0c64f8a", "Event");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 136, true);
			this.Controls.Add(this.FlowLayoutPanel);
			this.Controls.Add(this.EventAddUserControl);
			this.DataSourceType = typeof(StmALogAsAddedByUser);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ZStmALogAddForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.EventAddUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FlowLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
