using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	partial class AutoRefreshForm
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

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule", Justification = "Embedding a refresh icon, not a product icon")]
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoRefreshForm));
			this.TimeOutDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoBackButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProceedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label2 = new Enterprise.ZArchitecture.ZLabel();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TimeOutDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 208, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Modules.AutoRefresh.AutoRefreshBizO);
			// 
			// TimeOutDropEdit
			// 
			this.TimeOutDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TimeOutDropEdit, "AutoRefreshTimeOutDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.Modules.AutoRefresh.AutoRefreshBizO)(null)).AutoRefreshTimeOutDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Modules.AutoRefresh.AutoRefreshBizO)(null)).AutoRefreshTimeOutDescription_List)));
			this.TimeOutDropEdit.BindToList = "AutoRefreshTimeOutDescription_List";
			this.TimeOutDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("AutoRefreshForm|9b0ab182-b0d3-4b63-982b-569f24cb22ae", "Auto Refresh every");
			this.TimeOutDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 138, true);
			this.TimeOutDropEdit.Name = "TimeOutDropEdit";
			this.TimeOutDropEdit.PreBoundMaxLength = 10;
			this.TimeOutDropEdit.ShowDescriptionBox = false;
			this.TimeOutDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.TimeOutDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.TimeOutDropEdit.TabIndex = 4;
			// 
			// GoBackButton
			// 
			this.GoBackButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("AutoRefreshForm|638745ec-5aa9-42c4-a200-27f0eaebd655", "Cancel Auto Refresh");
			this.GoBackButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GoBackButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 180, true);
			this.GoBackButton.Name = "GoBackButton";
			this.GoBackButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 23, true);
			this.GoBackButton.TabIndex = 6;
			this.GoBackButton.UseVisualStyleBackColor = true;
			// 
			// ProceedButton
			// 
			this.ProceedButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("AutoRefreshForm|59714fd6-5d1c-43eb-8b9a-a4b6e0f78876", "Enable Auto Refresh");
			this.ProceedButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ProceedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 180, true);
			this.ProceedButton.Name = "ProceedButton";
			this.ProceedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 23, true);
			this.ProceedButton.TabIndex = 5;
			this.ProceedButton.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("AutoRefreshForm|310f99d6-0cd0-42a6-80a4-ff6cd87ca125", "", "You have chosen to automatically refresh this module.\r\n\r\nWide use of this feature on large systems or on a large number of workstations could negatively affect performance.");
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 70, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 39, true);
			this.label2.TabIndex = 2;
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("AutoRefreshForm|5bbc768d-0df4-4ea6-b2c5-79f5005ac07b", "*** WARNING - USE AUTO REFRESH CAREFULLY TO AVOID PERFORMANCE PROBLEMS ***");
			this.label1.ForeColor = System.Drawing.Color.Red;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 24, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 13, true);
			this.label1.TabIndex = 1;
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// AutoRefreshForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("AutoRefreshForm|cbcd58a3-ba8f-4e9c-a951-a9c7639ea401", "Auto Refresh");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 232, true);
			this.Controls.Add(this.TimeOutDropEdit);
			this.Controls.Add(this.GoBackButton);
			this.Controls.Add(this.ProceedButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Modules.AutoRefresh.AutoRefreshBizO);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Modules.AutoRefresh.AutoRefreshBizO";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.Name = "AutoRefreshForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.ProceedButton, 0);
			this.Controls.SetChildIndex(this.GoBackButton, 0);
			this.Controls.SetChildIndex(this.TimeOutDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TimeOutDropEdit.ResumeLayout(true);
			this.TimeOutDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZLabel label1;
		private ZLabel label2;
		private ZButton ProceedButton;
		private ZButton GoBackButton;
		private ZDropEdit TimeOutDropEdit;
	}
}
