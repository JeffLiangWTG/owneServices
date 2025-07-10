using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProductRegistration.GUI
{
	partial class UpdateRegistrationForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (cancelTokenSource != null)
				{
					cancelTokenSource.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		new protected void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.statusBox = new Enterprise.ZArchitecture.ZLabel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.changeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.waitAnimationBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.productKeyBox = new Enterprise.ZArchitecture.ZTextBox();
			this.helpTextBox = new Enterprise.ZArchitecture.ZLabel();
			this.logoBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.statusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.waitAnimationBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 258, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 10, true);
			this.MainStatusBar.TabIndex = 6;
			this.MainStatusBar.Visible = false;
			// 
			// refreshButton
			// 
			this.refreshButton.Image = global::Enterprise.ProductRegistration.GUI.Properties.Resources.reload;
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 54, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.refreshButton.TabIndex = 1;
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// statusBox
			// 
			this.statusBox.IsFontBold = true;
			this.statusBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 56, true);
			this.statusBox.Name = "statusBox";
			this.statusBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 13, true);
			this.statusBox.TabIndex = 0;
			this.statusBox.Text = "...";
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.closeButton.CaptionResourceString = Enterprise.ProductRegistration.GUI.Res.GetData("d513cb5e-1547-472a-a0c4-56ff4cd4d8e4", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 224, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 27, true);
			this.closeButton.TabIndex = 5;
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// changeButton
			// 
			this.changeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.changeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 224, true);
			this.changeButton.Name = "changeButton";
			this.changeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 27, true);
			this.changeButton.TabIndex = 4;
			this.changeButton.UseVisualStyleBackColor = true;
			this.changeButton.Visible = false;
			this.changeButton.Click += new System.EventHandler(this.changeButton_Click);
			// 
			// waitAnimationBox
			// 
			this.waitAnimationBox.Image = global::Enterprise.ProductRegistration.GUI.Properties.Resources.snake_transparent;
			this.waitAnimationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 54, true);
			this.waitAnimationBox.Name = "waitAnimationBox";
			this.waitAnimationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.waitAnimationBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.waitAnimationBox.TabIndex = 54;
			this.waitAnimationBox.TabStop = false;
			// 
			// productKeyBox
			// 
			this.productKeyBox.CaptionResourceString = Enterprise.ProductRegistration.GUI.Res.GetData("fb331825-3b9d-48af-9cc1-c994c4af8e50", "Product Key");
			this.productKeyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 86, true);
			this.productKeyBox.Name = "productKeyBox";
			this.productKeyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.productKeyBox.TabIndex = 2;
			this.productKeyBox.TextChanged += new System.EventHandler(this.productKeyBox_TextChanged);
			// 
			// helpTextBox
			// 
			this.helpTextBox.BackColor = System.Drawing.Color.White;
			this.helpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.helpTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 125, true);
			this.helpTextBox.Name = "helpTextBox";
			this.helpTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 61, true);
			this.helpTextBox.TabIndex = 3;
			this.helpTextBox.Visible = false;
			// 
			// logoBox
			// 
			this.logoBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 5, true);
			this.logoBox.Name = "logoBox";
			this.logoBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 38, true);
			this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.logoBox.TabIndex = 55;
			this.logoBox.TabStop = false;
			// 
			// statusLabel
			// 
			this.statusLabel.CaptionResourceString = Enterprise.ProductRegistration.GUI.Res.GetData("84a43847-5771-48e3-86ad-8a540d5c2ddd", "Status:");
			this.statusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 56, true);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.statusLabel.TabIndex = 56;
			this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UpdateRegistrationForm
			// 
			this.AcceptButton = this.closeButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ProductRegistration.GUI.Res.GetData("1feca869-9bc9-4e0a-bd60-1784b80e5e1f", "CargoWise Registration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 268, true);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.logoBox);
			this.Controls.Add(this.helpTextBox);
			this.Controls.Add(this.productKeyBox);
			this.Controls.Add(this.changeButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.statusBox);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.waitAnimationBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "UpdateRegistrationForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.waitAnimationBox, 0);
			this.Controls.SetChildIndex(this.refreshButton, 0);
			this.Controls.SetChildIndex(this.statusBox, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.changeButton, 0);
			this.Controls.SetChildIndex(this.productKeyBox, 0);
			this.Controls.SetChildIndex(this.helpTextBox, 0);
			this.Controls.SetChildIndex(this.logoBox, 0);
			this.Controls.SetChildIndex(this.statusLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.waitAnimationBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KPictureBox waitAnimationBox;
		private ZButton refreshButton;
		private ZLabel statusBox;
		private ZButton closeButton;
		private ZButton changeButton;
		private ZTextBox productKeyBox;
		private ZLabel helpTextBox;
		private KPictureBox logoBox;
		private ZLabel statusLabel;
	}
}
