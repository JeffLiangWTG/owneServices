using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class JobManagementForm
	{
		#region Windows Form Designer generated code

		private JobInvoicing.JobProfitLossControl ProfitLossControl;
		private ZPanel BottomPanel;
		private ZButton CloseButton;
		private ZButton OpenOperationalDetailsButton;
		private readonly System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.ProfitLossControl = new JobInvoicing.JobProfitLossControl();
			this.BottomPanel = new ZPanel();
			this.CloseButton = new ZButton();
			this.OpenOperationalDetailsButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 519, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(482);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(483);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobProfitLoss);
			// 
			// ProfitLossControl
			// 
			this.BindingSource.SetBindingMember(this.ProfitLossControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Integration.IJobProfitLoss)(((JobProfitLoss)(null)))));
			this.ProfitLossControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProfitLossControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProfitLossControl.Name = "ProfitLossControl";
			this.ProfitLossControl.PluginSecurity = null;
			this.ProfitLossControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 484, true);
			this.ProfitLossControl.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Controls.Add(this.OpenOperationalDetailsButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 484, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 35, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobManagementForm|3f7cb8cb-ddb0-4413-a668-ecc013be23f1", "Close");
			this.CloseButton.EditableInViewMode = true;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 6, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// OpenOperationalDetailsButton
			// 
			this.OpenOperationalDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenOperationalDetailsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobManagementForm|3cc24b33-ddd4-442c-9720-ca09c40474ce", "Open Operational Details");
			this.OpenOperationalDetailsButton.EditableInViewMode = true;
			this.OpenOperationalDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(743, 6, true);
			this.OpenOperationalDetailsButton.Name = "OpenOperationalDetailsButton";
			this.OpenOperationalDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.OpenOperationalDetailsButton.TabIndex = 0;
			this.OpenOperationalDetailsButton.Click += new EventHandler(this.OpenButton_Click);
			// 
			// JobManagementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 543, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobManagementForm|cdbb8afd-02ac-4f3f-8570-4a65fdb48acc", "Job Profit/Loss");
			this.Controls.Add(this.ProfitLossControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(JobProfitLoss);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.JobProfitLoss";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 420, true);
			this.Name = "JobManagementForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ProfitLossControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion


	}
}
