namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class TnnForArrivalForm
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
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AcceptanceDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClearanceDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AcceptanceDate.SuspendLayout();
			this.ClearanceDate.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 145, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 24, true);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("34636738-3779-4568-83E6-54AC9C1622F0", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 116, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("8178EABB-3003-4757-865C-C392D00D5A0E", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 116, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MessageLabel.TabIndex = 3;
			// 
			// AcceptanceDate
			// 
			this.AcceptanceDate.AllowDrop = true;
			this.AcceptanceDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptanceDate, "AcceptanceDate");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.NCTS.Business.TnnDataCodeInfo)(null)).AcceptanceDate)));
			this.AcceptanceDate.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("0EAC8F6D-F125-4E95-A720-A6AECC0DAF5C", "Acceptance Date");
			this.AcceptanceDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AcceptanceDate, true);
			this.AcceptanceDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 49, true);
			this.AcceptanceDate.Name = "AcceptanceDate";
			this.AcceptanceDate.TabIndex = 0;
			this.AcceptanceDate.TabStop = true;
			// 
			// ClearanceDate
			// 
			this.ClearanceDate.AllowDrop = true;
			this.ClearanceDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ClearanceDate, "ClearanceDate");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.NCTS.Business.TnnDataCodeInfo)(null)).ClearanceDate)));
			this.ClearanceDate.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("9C2E1BF0-BBD7-44BE-8FD9-869FD128F5B6", "Clearance Date");
			this.ClearanceDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClearanceDate, true);
			this.ClearanceDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 75, true);
			this.ClearanceDate.Name = "ClearanceDate";
			this.ClearanceDate.TabIndex = 1;
			this.ClearanceDate.TabStop = true;
			// 
			// TnnForArrivalForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 169, true);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.ClearanceDate);
			this.Controls.Add(this.AcceptanceDate);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "TnnForArrivalForm";
			this.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("2957F546-0FBB-465B-9648-9985D02C29B9", "Make TNN for this Arrival");
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.AcceptanceDate, 0);
			this.Controls.SetChildIndex(this.ClearanceDate, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AcceptanceDate.ResumeLayout(true);
			this.AcceptanceDate.PerformLayout();
			this.ClearanceDate.ResumeLayout(true);
			this.ClearanceDate.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.ZLabel MessageLabel;
		public ZArchitecture.GUI.ZDateEdit AcceptanceDate;
		public ZArchitecture.GUI.ZDateEdit ClearanceDate;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton Cancel_Button;
	}
}
