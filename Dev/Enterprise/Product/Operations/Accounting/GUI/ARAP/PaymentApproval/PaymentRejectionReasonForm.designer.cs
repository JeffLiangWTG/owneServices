namespace Enterprise.Accounting.GUI.ARAP
{
	partial class PaymentRejectionReasonForm
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
		new void InitializeComponent()
		{
			this.OKReasonButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelReasonButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReversingReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.paymentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.bottomLabel = new Enterprise.ZArchitecture.ZLabel();
			this.topLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReversingReasonCodeDropEdit.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 196, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentRejectionReasonHolder);
			// 
			// OKReasonButton
			// 
			this.OKReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKReasonButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|638a3fe3-cd9f-4b54-a56a-33172994aabe", "OK", "&OK", "");
			this.OKReasonButton.IsCaptionOverridden = false;
			this.OKReasonButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 167, true);
			this.OKReasonButton.Name = "OKReasonButton";
			this.OKReasonButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKReasonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKReasonButton.TabIndex = 4;
			this.OKReasonButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKReasonButton.ToolTipCaption = null;
			this.OKReasonButton.Click += new System.EventHandler(this.OKReasonButton_Click);
			// 
			// CancelReasonButton
			// 
			this.CancelReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelReasonButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionReasonForm|4256b02a-9e2a-4a0a-aba4-802bd648d732", "Cancel", "Cancel", "");
			this.CancelReasonButton.IsCaptionOverridden = false;
			this.CancelReasonButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 167, true);
			this.CancelReasonButton.Name = "CancelReasonButton";
			this.CancelReasonButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelReasonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelReasonButton.TabIndex = 5;
			this.CancelReasonButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelReasonButton.ToolTipCaption = null;
			this.CancelReasonButton.Click += new System.EventHandler(this.CancelReasonButton_Click);
			// 
			// ReasonTextBox
			// 
			this.ReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).Reason)));
			this.ReasonTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("76fdce92-b9d3-4afc-8370-a8549870d34e", "Rejection Details");
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 141, true);
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 20, true);
			this.ReasonTextBox.TabIndex = 3;
			// 
			// ReversingReasonCodeDropEdit
			// 
			this.ReversingReasonCodeDropEdit.AllowDrop = true;
			this.ReversingReasonCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReversingReasonCodeDropEdit, "Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).Code)));
			this.ReversingReasonCodeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2535a7e2-124b-4c9c-912f-076c39768a37", "Rejection Description");
			this.ReversingReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 115, true);
			this.ReversingReasonCodeDropEdit.Name = "ReversingReasonCodeDropEdit";
			this.ReversingReasonCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ReversingReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 20, true);
			this.ReversingReasonCodeDropEdit.TabIndex = 2;
			// 
			// paymentsLabel
			// 
			this.paymentsLabel.AutoSize = true;
			this.paymentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.paymentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.paymentsLabel.Name = "paymentsLabel";
			this.paymentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.paymentsLabel.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.AutoScroll = true;
			this.zPanel1.Controls.Add(this.paymentsLabel);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 36, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 50, true);
			this.zPanel1.TabIndex = 6;
			// 
			// bottomLabel
			// 
			this.bottomLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.bottomLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 89, true);
			this.bottomLabel.Name = "bottomLabel";
			this.bottomLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 23, true);
			this.bottomLabel.TabIndex = 7;
			// 
			// topLabel
			// 
			this.topLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d71ab09b-473a-46b9-b9ae-321bb767806e", "The following payments will be rejected:");
			this.topLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.topLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.topLabel.Name = "topLabel";
			this.topLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 23, true);
			this.topLabel.TabIndex = 8;
			// 
			// PaymentRejectionReasonForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 196, true);
			this.ControlBox = false;
			this.Controls.Add(this.topLabel);
			this.Controls.Add(this.bottomLabel);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.ReversingReasonCodeDropEdit);
			this.Controls.Add(this.ReasonTextBox);
			this.Controls.Add(this.OKReasonButton);
			this.Controls.Add(this.CancelReasonButton);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 235, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 235, true);
			this.Name = "PaymentRejectionReasonForm";
			this.Text = "ReversingReasonForm";
			this.Controls.SetChildIndex(this.CancelReasonButton, 0);
			this.Controls.SetChildIndex(this.OKReasonButton, 0);
			this.Controls.SetChildIndex(this.ReasonTextBox, 0);
			this.Controls.SetChildIndex(this.ReversingReasonCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.bottomLabel, 0);
			this.Controls.SetChildIndex(this.topLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReversingReasonCodeDropEdit.ResumeLayout(true);
			this.ReversingReasonCodeDropEdit.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton OKReasonButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelReasonButton;
		private Enterprise.ZArchitecture.ZTextBox ReasonTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ReversingReasonCodeDropEdit;
		private ZArchitecture.ZLabel paymentsLabel;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.ZLabel bottomLabel;
		private ZArchitecture.ZLabel topLabel;
	}
}
