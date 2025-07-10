using System;
using System.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	partial class ProfilePerformanceForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
		private new void InitializeComponent()
		{
			this.btnStartStop = new Enterprise.ZArchitecture.GUI.ZButton();
			this.txtLogs = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 375, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Main.Startup.Tools.JetBrains.ProfilePerformanceModel);
			// 
			// btnStartStop
			// 
			this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStartStop.CaptionResourceString = CargoWise.Main.Res.GetData("f308bd76-7ff1-4fc1-a705-30c9dbeb6ad7", "Start");
			this.btnStartStop.Enabled = true;
			this.btnStartStop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 10, true);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 35, true);
			this.btnStartStop.TabIndex = 0;
			this.btnStartStop.ToolTipCaption = null;
			this.btnStartStop.UseVisualStyleBackColor = false;
			this.btnStartStop.Click += new System.EventHandler(this.BtnStartClick);
			// 
			// txtLogs
			// 
			this.txtLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.txtLogs, "Output");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Main.Startup.Tools.JetBrains.ProfileModel)(null)).Output)));
			this.txtLogs.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtLogs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 58, true);
			this.txtLogs.Multiline = true;
			this.txtLogs.Name = "txtLogs";
			this.txtLogs.ReadOnly = true;
			this.txtLogs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 307, true);
			this.txtLogs.TabIndex = 8;
			// 
			// ProfilePerformanceForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("60a023f7-daf5-4094-a91d-362e194e97cb", "Profile Performance");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 399, true);
			this.Controls.Add(this.txtLogs);
			this.Controls.Add(this.btnStartStop);
			this.DataSourceType = typeof(CargoWise.Main.Startup.Tools.JetBrains.ProfileModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 250, true);
			this.Name = "ProfilePerformanceForm";
			this.Text = "Profile Performance";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Controls.SetChildIndex(this.btnStartStop, 0);
			this.Controls.SetChildIndex(this.txtLogs, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton btnStartStop;
		private ZTextBox txtLogs;
	}
}
