using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AddLogCommentPopupForm
	{

		#region Windows Form Designer generated code

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
		protected ZButton CloseButton;
		protected ZButton CancelButtonX;
		protected ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZLabel zLabel1;
		private readonly System.ComponentModel.IContainer components = null;

		new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.CancelButtonX = new ZButton();
			this.zTextBox1 = new ZArchitecture.ZTextBox();
			this.zLabel1 = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccQueryClaimLogAdder);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddLogCommentPopupForm|203e02aa-f9c3-4e43-a746-3347b375e307", "OK");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 224, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddLogCommentPopupForm|941c05a1-9ae0-46d3-8e61-713eac541dde", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 224, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 2;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new EventHandler(this.CancelButtonX_Click);
			// 
			// zTextBox1
			// 
			this.zTextBox1.AcceptsReturn = true;
			this.zTextBox1.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.zTextBox1, "LogComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaimLogAdder)(null)).LogComment)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 51, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 165, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddLogCommentPopupForm|1064863a-503f-4d83-9da9-4dc90f743a3b", "", "Please specify a descriptive comment to add to the log. All emails and file attachments should be added through the eDocs tab.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 39, true);
			this.zLabel1.TabIndex = 5;
			// 
			// AddLogCommentPopupForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 277, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddLogCommentPopupForm|df7bc6a1-61b0-42c2-b152-314288383f2a", "Add to Log");
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(AccQueryClaimLogAdder);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AddLogCommentPopupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
