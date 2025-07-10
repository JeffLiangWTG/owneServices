using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	partial class ProfileMemoryForm
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
		private new void InitializeComponent()
		{
			this.btnStart = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnFinish = new Enterprise.ZArchitecture.GUI.ZButton();
			this.txtLogs = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Main.Startup.Tools.JetBrains.ProfileMemoryModel);
			// 
			// btnSnapshot
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.CaptionResourceString = CargoWise.Main.Res.GetData("f308bd76-7ff1-4fc1-a705-30c9dbeb6ad7", "Start");
			this.btnStart.Enabled = true;
			this.btnStart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(552, 12, true);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 35, true);
			this.btnStart.TabIndex = 0;
			this.btnStart.ToolTipCaption = null;
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
			// 
			// btnFinish
			// 
			this.btnFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFinish.CaptionResourceString = CargoWise.Main.Res.GetData("f5cebec0-1a53-4f3b-b0e3-673389c9792f", "Finish");
			this.btnFinish.Enabled = true;
			this.btnFinish.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(675, 12, true);
			this.btnFinish.Name = "btnFinish";
			this.btnFinish.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 35, true);
			this.btnFinish.TabIndex = 1;
			this.btnFinish.ToolTipCaption = null;
			this.btnFinish.UseVisualStyleBackColor = true;
			this.btnFinish.Click += new System.EventHandler(this.BtnFinishClick);
			// 
			// txtLogs
			// 
			this.txtLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.txtLogs, "Output");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Main.Startup.Tools.JetBrains.ProfileMemoryModel)(null)).Output)));
			this.txtLogs.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtLogs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 56, true);
			this.txtLogs.Multiline = true;
			this.txtLogs.Name = "txtLogs";
			this.txtLogs.ReadOnly = true;
			this.txtLogs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 358, true);
			this.txtLogs.TabIndex = 2;
			// 
			// ProfileMemoryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("4788864b-356e-4911-ac4b-ce86451b80a6", "Profile Memory");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 450, true);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtLogs);
			this.Controls.Add(this.btnFinish);
			this.DataSourceType = typeof(CargoWise.Main.Startup.Tools.JetBrains.ProfileMemoryModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 250, true);
			this.Name = "ProfileMemoryForm";
			this.Text = "Profile Memory";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Controls.SetChildIndex(this.btnFinish, 0);
			this.Controls.SetChildIndex(this.txtLogs, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.btnStart, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton btnStart;
		private ZButton btnFinish;
		private ZTextBox txtLogs;
	}
}
