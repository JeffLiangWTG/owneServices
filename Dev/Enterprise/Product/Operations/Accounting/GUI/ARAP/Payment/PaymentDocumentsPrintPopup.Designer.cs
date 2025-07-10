using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Payment
{
	public partial class PaymentDocumentsPrintPopup
	{


		#region Windows Form Designer generated code

		internal ZButton PrintButton;
		ZGroupBox zGroupBox1;
		internal ZButton CloseButton;
		protected ZCheckBox PrintChequeCheckBox;
		protected ZCheckBox RemittanceAdviceCheckBox;
		protected ZCheckBox PaymentVoucherCheckBox;
		ZLabel PaymentDescriptionLabel;
		protected ZLabel ChequeIsAutoPrintedZLabel;
		protected ZCheckBox PrintPaymentBatchListingCheckBox;
		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			this.PrintButton = new ZButton();
			this.CloseButton = new ZButton();
			this.zGroupBox1 = new ZGroupBox();
			this.PrintPaymentBatchListingCheckBox = new ZCheckBox();
			this.ChequeIsAutoPrintedZLabel = new ZLabel();
			this.PaymentVoucherCheckBox = new ZCheckBox();
			this.RemittanceAdviceCheckBox = new ZCheckBox();
			this.PrintChequeCheckBox = new ZCheckBox();
			this.PaymentDescriptionLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|Print", "Print");
			this.PrintButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.PrintButton.IsCaptionOverridden = false;
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 155, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 1;
			this.PrintButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PrintButton.ToolTipCaption = null;
			this.PrintButton.Click += new EventHandler(this.PrintButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|Cancel", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 155, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|PrintOptions", "Print Options");
			this.zGroupBox1.Controls.Add(this.PrintPaymentBatchListingCheckBox);
			this.zGroupBox1.Controls.Add(this.ChequeIsAutoPrintedZLabel);
			this.zGroupBox1.Controls.Add(this.PaymentVoucherCheckBox);
			this.zGroupBox1.Controls.Add(this.RemittanceAdviceCheckBox);
			this.zGroupBox1.Controls.Add(this.PrintChequeCheckBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 23, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 126, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// PrintPaymentBatchListingCheckBox
			//
			this.PrintPaymentBatchListingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|PrintPaymentBatch", "Print Payment Batch Listing");
			this.PrintPaymentBatchListingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintPaymentBatchListingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 97, true);
			this.PrintPaymentBatchListingCheckBox.Name = "PrintPaymentBatchListingCheckBox";
			this.PrintPaymentBatchListingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 22, true);
			this.PrintPaymentBatchListingCheckBox.TabIndex = 4;
			// 
			// ChequeIsAutoPrintedZLabel
			// 
			this.ChequeIsAutoPrintedZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|CheckIsAutoPrintedWhenPostingPayment", "Check Is Auto Printed when posting Payment");
			this.ChequeIsAutoPrintedZLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChequeIsAutoPrintedZLabel.ForeColor = System.Drawing.Color.Red;
			this.ChequeIsAutoPrintedZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 14, true);
			this.ChequeIsAutoPrintedZLabel.Name = "ChequeIsAutoPrintedZLabel";
			this.ChequeIsAutoPrintedZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 31, true);
			this.ChequeIsAutoPrintedZLabel.TabIndex = 3;
			this.ChequeIsAutoPrintedZLabel.Visible = false;
			// 
			// PaymentVoucherCheckBox
			// 
			this.PaymentVoucherCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|PrintPaymentVoucher", "Print Payment Voucher");
			this.PaymentVoucherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PaymentVoucherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 71, true);
			this.PaymentVoucherCheckBox.Name = "PaymentVoucherCheckBox";
			this.PaymentVoucherCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 22, true);
			this.PaymentVoucherCheckBox.TabIndex = 2;
			// 
			// RemittanceAdviceCheckBox
			// 
			this.RemittanceAdviceCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|PrintRemittanceAdvice", "Print Remittance Advice");
			this.RemittanceAdviceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RemittanceAdviceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.RemittanceAdviceCheckBox.Name = "RemittanceAdviceCheckBox";
			this.RemittanceAdviceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 22, true);
			this.RemittanceAdviceCheckBox.TabIndex = 1;
			// 
			// PrintChequeCheckBox
			// 
			this.PrintChequeCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|PrintCheck", "Print Check");
			this.PrintChequeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintChequeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21, true);
			this.PrintChequeCheckBox.Name = "PrintChequeCheckBox";
			this.PrintChequeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 22, true);
			this.PrintChequeCheckBox.TabIndex = 0;
			// 
			// PaymentDescriptionLabel
			// 
			this.PaymentDescriptionLabel.AutoSize = true;
			this.PaymentDescriptionLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("RemittanceAdvicePrintForm|Payment", "Payment:");
			this.PaymentDescriptionLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PaymentDescriptionLabel.IsFontBold = true;
			this.PaymentDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.PaymentDescriptionLabel.Name = "PaymentDescriptionLabel";
			this.PaymentDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.PaymentDescriptionLabel.TabIndex = 0;
			// 
			// PaymentDocumentsPrintPopup
			// 
			this.AcceptButton = this.PrintButton;
			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 182, true);
			this.Controls.Add(this.PaymentDescriptionLabel);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PrintButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 176, true);
			this.Name = "PaymentDocumentsPrintPopup";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PaymentDocumentsPrintPopup";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}