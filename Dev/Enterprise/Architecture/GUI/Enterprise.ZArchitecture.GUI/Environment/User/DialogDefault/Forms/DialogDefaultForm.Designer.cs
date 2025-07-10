using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DialogDefault
{
	partial class DialogDefaultForm
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
		new private void InitializeComponent()
		{
			this.saveDefaultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.iconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.whyReadOnlyLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Core.DialogDefault.DialogDefaultSaveOptions);
			// 
			// saveDefaultCheckBox
			// 
			this.saveDefaultCheckBox.AutoSize = true;
			this.saveDefaultCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.saveDefaultCheckBox, "SaveNewDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).SaveNewDefaults)));
			this.saveDefaultCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("471f7092-e509-4f05-9997-a8cb1a6772bd", "Save as default");
			this.saveDefaultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveDefaultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 102, true);
			this.saveDefaultCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 20, 3, true);
			this.saveDefaultCheckBox.Name = "saveDefaultCheckBox";
			this.saveDefaultCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.saveDefaultCheckBox.TabIndex = 0;
			this.saveDefaultCheckBox.UseVisualStyleBackColor = false;
			// 
			// iconPictureBox
			// 
			this.iconPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.iconPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.iconPictureBox.Name = "iconPictureBox";
			this.iconPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.iconPictureBox.TabIndex = 13;
			this.iconPictureBox.TabStop = false;
			// 
			// whyReadOnlyLabel
			// 
			this.whyReadOnlyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.whyReadOnlyLabel.AutoSize = true;
			this.whyReadOnlyLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("c0d2cea0-4a6a-4be7-a08a-ae3ca3f399ec", "Why is this read only?", "A default exists that has \"Override Lower Levels\" set to true, not allowing you to modify this dialog");
			this.whyReadOnlyLabel.IsFontBold = false;
			this.whyReadOnlyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 103, true);
			this.whyReadOnlyLabel.Name = "whyReadOnlyLabel";
			this.whyReadOnlyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 13, true);
			this.whyReadOnlyLabel.TabIndex = 1;
			// 
			// DialogDefaultForm
			// 
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 149, true);
			this.Controls.Add(this.whyReadOnlyLabel);
			this.Controls.Add(this.iconPictureBox);
			this.Controls.Add(this.saveDefaultCheckBox);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.DataSourceType = typeof(Enterprise.Core.DialogDefault.DialogDefaultSaveOptions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "DialogDefaultForm";
			this.Controls.SetChildIndex(this.saveDefaultCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.iconPictureBox, 0);
			this.Controls.SetChildIndex(this.whyReadOnlyLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox saveDefaultCheckBox;
		private KPictureBox iconPictureBox;
		private ZLinkLabel whyReadOnlyLabel;

	}
}
