using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	public partial class BulkStatusUpdatingForm : ZChildForm
	{
		protected Enterprise.ZArchitecture.GUI.ZButton BulkUpdateButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.MasterFiles.GUI.CurrentQueueUserControl currentQueueUserControl1;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.BulkUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.currentQueueUserControl1 = new Enterprise.MasterFiles.GUI.CurrentQueueUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 224, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			// 
			// BulkUpdateButton
			// 
			this.BulkUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BulkUpdateButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BulkUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 196, true);
			this.BulkUpdateButton.Name = "BulkUpdateButton";
			this.BulkUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.BulkUpdateButton.TabIndex = 1;
			this.BulkUpdateButton.Text = "Update Now";
			this.BulkUpdateButton.Click += new System.EventHandler(this.OnBulkUpdate_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 196, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.Click += new System.EventHandler(this.OnCancel_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.currentQueueUserControl1);
			this.zGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 184, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Enter new details to bulk update";
			// 
			// currentQueueUserControl1
			// 
			this.currentQueueUserControl1.AlignControlsIntoSingleColumn = true;
			this.currentQueueUserControl1.BindToPrefix = "NonPersistentQueue.";
			this.currentQueueUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.currentQueueUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.currentQueueUserControl1.Name = "currentQueueUserControl1";
			this.currentQueueUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 165, true);
			this.currentQueueUserControl1.TabIndex = 0;
			// 
			// BulkStatusUpdatingForm
			// 

			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 248, true);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.BulkUpdateButton);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Module.BulkStatusUpdating";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "BulkStatusUpdatingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.BulkUpdateButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
