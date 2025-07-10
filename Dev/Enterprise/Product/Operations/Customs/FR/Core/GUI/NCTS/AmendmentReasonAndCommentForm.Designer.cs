
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	partial class AmendmentReasonAndCommentForm
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
		protected new void InitializeComponent()
		{
			this.CommentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommentTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("F0DB9133-BC8E-45C7-89FE-46F729B27996", "Please enter a reason for the amendment or withdrawal:");
			this.ReasonLabel.TabIndex = 1;
			// 
			// CancelButton
			// 
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 296, true);
			this.CancelButton.TabIndex = 6;
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 296, true);
			this.OKButton.TabIndex = 5;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 337, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.AmendmentWithdrawalReasonAndComment);
			// 
			// CommentLabel
			// 
			this.CommentLabel.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1348D906-F2A8-48AA-B703-0BAFECA7776F", "Please enter a comment about the amendment or withdrawal:");
			this.CommentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 140, true);
			this.CommentLabel.Name = "CommentLabel";
			this.CommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 23, true);
			this.CommentLabel.TabIndex = 3;
			// 
			// CommentTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommentTextTextBox, "CommentText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.AmendmentWithdrawalReasonAndComment)(null)).CommentText)));
			this.CommentTextTextBox.CaptionResourceString = null;
			this.CommentTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CommentTextTextBox, false);
			this.CommentTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 163, true);
			this.CommentTextTextBox.Multiline = true;
			this.CommentTextTextBox.Name = "CommentTextTextBox";
			this.CommentTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CommentTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 104, true);
			this.CommentTextTextBox.TabIndex = 4;
			// 
			// AmendmentReasonAndCommentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 361, true);
			this.Controls.Add(this.CommentTextTextBox);
			this.Controls.Add(this.CommentLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.FR.Business.MessageManager";
			this.DataSourceType = typeof(Enterprise.Customs.FR.Business.AmendmentWithdrawalReasonAndComment);
			this.DataSourceTypeName = "Enterprise.Customs.FR.Business.MessageManager.AmendmentWithdrawalReasonAndComment";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "AmendmentReasonAndCommentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.ReasonLabel, 0);
			this.Controls.SetChildIndex(this.ReasonTextTextBox, 0);
			this.Controls.SetChildIndex(this.CommentLabel, 0);
			this.Controls.SetChildIndex(this.CommentTextTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected Enterprise.ZArchitecture.ZLabel CommentLabel;
		protected Enterprise.ZArchitecture.ZTextBox CommentTextTextBox;

		#endregion
	}
}
