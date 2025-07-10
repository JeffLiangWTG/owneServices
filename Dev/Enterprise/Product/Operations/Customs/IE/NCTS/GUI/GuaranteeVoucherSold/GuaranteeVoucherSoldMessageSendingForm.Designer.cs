namespace Enterprise.Customs.IE.NCTS.GUI
{
	partial class GuaranteeVoucherSoldMessageSendingForm
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.HolderOfTransitProcedureFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TIRCarnetCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CustomsOfficeOfGuaranteeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VoucherAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.messageSendingObjectsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
            this.MessageSendingObjectsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ButtonsPanel.SuspendLayout();
            this.HolderOfTransitProcedureFindBox.SuspendLayout();
			this.TIRCarnetCheckBox.SuspendLayout();
			this.CustomsOfficeOfGuaranteeFindBox.SuspendLayout();
			this.VoucherAmountCalcEdit.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendButton
            // 
            this.SendButton.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("9BCDE3DE-C62E-4ED8-8BFF-CD59FB1B73B2", "Send");
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 2, true);
            this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.SendButton.TabIndex = 9;
            // 
            // CancelButton2
            // 
            this.CancelButton2.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("0033E17A-347B-4CE8-85CF-2F9C476669E0", "Cancel");
            this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 2, true);
            this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.CancelButton2.TabIndex = 10;
            // 
            // messageSendingObjectsGroupBox
            // 
            this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 124, true);
            this.messageSendingObjectsGroupBox.Visible = false;
            // 
            // MessageSendingObjectsGrid
            // 
            this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 107, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 23, true);
            this.MainStatusBar.TabIndex = 1;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent);
            // 
            // ButtonsPanel
            // 
            this.ButtonsPanel.Controls.Add(this.CancelButton2);
            this.ButtonsPanel.Controls.Add(this.SendButton);
            this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 102, true);
            this.ButtonsPanel.Name = "ButtonsPanel";
            this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 29, true);
            this.ButtonsPanel.TabIndex = 3;
            this.ButtonsPanel.Controls.SetChildIndex(this.SendButton, 0);
            this.ButtonsPanel.Controls.SetChildIndex(this.CancelButton2, 0);
			// 
			// HolderOfTransitProcedureFindBox
			// 
			this.HolderOfTransitProcedureFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HolderOfTransitProcedureFindBox, "SendingObjectsCollection.HolderOfTransitProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).HolderOfTransitProcedure)));
            this.HolderOfTransitProcedureFindBox.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("1F48BB61-9AB5-4CC3-ABF0-E2F9A1E8EF2E", "Transit Holder");
            this.HolderOfTransitProcedureFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 17, true);
            this.HolderOfTransitProcedureFindBox.Name = "HolderOfTransitProcedureFindBox";
            this.HolderOfTransitProcedureFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.HolderOfTransitProcedureFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.HolderOfTransitProcedureFindBox.TabIndex = 2;
			// 
			// TIRCarnetCheckBox
			// 
			this.TIRCarnetCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TIRCarnetCheckBox, "SendingObjectsCollection.TIRCarnet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).TIRCarnet)));
			this.TIRCarnetCheckBox.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("7FDAF458-053E-425B-91D3-C01BBAA331F5", "TIR Carnet");
			this.TIRCarnetCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 47, true);
			this.TIRCarnetCheckBox.Name = "TIRCarnetCheckBox";
			this.TIRCarnetCheckBox.TabIndex = 3;
			// 
			// CustomsOfficeOfGuaranteeFindBox
			// 
			this.CustomsOfficeOfGuaranteeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeOfGuaranteeFindBox, "SendingObjectsCollection.CustomsOfficeOfGuarantee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsOfficeOfGuarantee)));
			this.CustomsOfficeOfGuaranteeFindBox.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("8EDF7FB1-4FCF-4E8E-9A4E-C230BAF2012D", "Guarantee Office");
			this.CustomsOfficeOfGuaranteeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 17, true);
			this.CustomsOfficeOfGuaranteeFindBox.Name = "CustomsOfficeOfGuaranteeFindBox";
			this.CustomsOfficeOfGuaranteeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsOfficeOfGuaranteeFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.CustomsOfficeOfGuaranteeFindBox.TabIndex = 4;
			// 
			// VoucherAmountCalcEdit
			// 
			this.VoucherAmountCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VoucherAmountCalcEdit, "SendingObjectsCollection.VoucherAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.GuaranteeVoucherSoldSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).VoucherAmount)));
			this.VoucherAmountCalcEdit.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("86544A94-97EE-478A-A88D-AD03BDC5ABD8", "Voucher Amount");
			this.VoucherAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 47, true);
			this.VoucherAmountCalcEdit.Name = "VoucherAmountCalcEdit";
			this.VoucherAmountCalcEdit.TabIndex = 5;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainGroupBox.Controls.Add(this.HolderOfTransitProcedureFindBox);
			this.MainGroupBox.Controls.Add(this.TIRCarnetCheckBox);
			this.MainGroupBox.Controls.Add(this.CustomsOfficeOfGuaranteeFindBox);
			this.MainGroupBox.Controls.Add(this.VoucherAmountCalcEdit);
			this.MainGroupBox.Controls.Add(this.ButtonsPanel);
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
            this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 133, true);
            this.MainGroupBox.TabIndex = 11;
            this.MainGroupBox.TabStop = false;
            // 
            // GuaranteeVoucherSoldMessageSendingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 156, true);
            this.Controls.Add(this.MainGroupBox);
            this.DataSourceType = typeof(Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 193, true);
            this.Name = "GuaranteeVoucherSoldMessageSendingForm";
            this.Text = "Guarantee Voucher Sold";
            this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.MainGroupBox, 0);
            this.messageSendingObjectsGroupBox.ResumeLayout(false);
            this.messageSendingObjectsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
            this.MessageSendingObjectsGrid.ResumeLayout(false);
            this.MessageSendingObjectsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ButtonsPanel.ResumeLayout(false);
            this.ButtonsPanel.PerformLayout();
            this.HolderOfTransitProcedureFindBox.ResumeLayout(true);
            this.HolderOfTransitProcedureFindBox.PerformLayout();
            this.TIRCarnetCheckBox.ResumeLayout(true);
            this.TIRCarnetCheckBox.PerformLayout();
			this.CustomsOfficeOfGuaranteeFindBox.ResumeLayout(false);
			this.CustomsOfficeOfGuaranteeFindBox.PerformLayout();
			this.VoucherAmountCalcEdit.ResumeLayout(true);
			this.VoucherAmountCalcEdit.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		internal ZArchitecture.GUI.ZCodeFindBox HolderOfTransitProcedureFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox TIRCarnetCheckBox;
		internal Enterprise.ZArchitecture.ZCalcEdit VoucherAmountCalcEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeOfGuaranteeFindBox;
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
	}
}
