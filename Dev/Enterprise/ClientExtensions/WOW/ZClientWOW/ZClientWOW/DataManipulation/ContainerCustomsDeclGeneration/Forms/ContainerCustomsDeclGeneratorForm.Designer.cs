using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Wow
{
	public partial class ContainerCustomsDeclGeneratorForm : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		protected Enterprise.Client.Wow.ThreadSafeTextBox ProgressTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton ExecuteButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ProgressTextBox = new Enterprise.Client.Wow.ThreadSafeTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExecuteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 529, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(332);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.ProgressTextBox.Multiline = true;
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 453, true);
			this.ProgressTextBox.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 501, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Text = "Close";
			// 
			// ExecuteButton
			// 
			this.ExecuteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.ExecuteButton.Name = "ExecuteButton";
			this.ExecuteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.ExecuteButton.TabIndex = 0;
			this.ExecuteButton.Text = "Execute";
			this.ExecuteButton.Click += new System.EventHandler(this.OnExecute_Click);
			// 
			// ContainerCustomsDeclGeneratorForm
			// 

			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Controls.Add(this.ExecuteButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ProgressTextBox);
			this.Name = "ContainerCustomsDeclGeneratorForm";
			this.Controls.SetChildIndex(this.ProgressTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ExecuteButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
