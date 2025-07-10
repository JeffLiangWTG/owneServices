using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitDocumentPrintingForm
	{


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.PrintButton = new ZButton();
			this.PrintChargeSummaryCheckBox = new ZCheckBox();
			this.PrintChargeDetailCheckBox = new ZCheckBox();
			this.PrintARInvoiceAnalysisCheckBox = new ZCheckBox();
			this.PrintAPInvoiceAnalysisCheckBox = new ZCheckBox();
			this.PrintProfitRecognitionByDateSummary = new ZCheckBox();
			this.PrintJobRevenueJournalAnalysisCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 241, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 8;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDocumentPrinter);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|fae4071d-427d-4176-a424-9e08842bfa1e", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 205, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			// 
			// PrintButton
			// 
			this.PrintButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|15af3e71-0629-4b7d-9f14-96e6a842cd4f", "&Print");
			this.PrintButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.PrintButton.IsCaptionOverridden = false;
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 205, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.PrintButton.TabIndex = 6;
			this.PrintButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PrintButton.ToolTipCaption = null;
			// 
			// PrintChargeSummaryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PrintChargeSummaryCheckBox, "PrintChargeSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDocumentPrinter)(null)).PrintChargeSummary)));
			this.PrintChargeSummaryCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|c369631f-9ac7-4aa0-8e8b-3fc3880830b6", "Print Charge Summary");
			this.PrintChargeSummaryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintChargeSummaryCheckBox.Checked = true;
			this.PrintChargeSummaryCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintChargeSummaryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintChargeSummaryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.PrintChargeSummaryCheckBox.Name = "PrintChargeSummaryCheckBox";
			this.PrintChargeSummaryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.PrintChargeSummaryCheckBox.TabIndex = 0;
			// 
			// PrintChargeDetailCheckBox
			// 
			this.PrintChargeDetailCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintChargeDetailCheckBox, "PrintChargeDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDocumentPrinter)(null)).PrintChargeDetail)));
			this.PrintChargeDetailCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|ee38a052-cbe4-43dd-8691-157b40c9bd52", "Print Charge Detail");
			this.PrintChargeDetailCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintChargeDetailCheckBox.Checked = true;
			this.PrintChargeDetailCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintChargeDetailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintChargeDetailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 76, true);
			this.PrintChargeDetailCheckBox.Name = "PrintChargeDetailCheckBox";
			this.PrintChargeDetailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 22, true);
			this.PrintChargeDetailCheckBox.TabIndex = 2;
			// 
			// PrintARInvoiceAnalysisCheckBox
			// 
			this.PrintARInvoiceAnalysisCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintARInvoiceAnalysisCheckBox, "PrintARInvoiceAnalysis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDocumentPrinter)(null)).PrintARInvoiceAnalysis)));
			this.PrintARInvoiceAnalysisCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|65df7166-49b2-40c5-97ea-b026f82519c2", "Print Receivables Invoice Analysis");
			this.PrintARInvoiceAnalysisCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintARInvoiceAnalysisCheckBox.Checked = true;
			this.PrintARInvoiceAnalysisCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintARInvoiceAnalysisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintARInvoiceAnalysisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 106, true);
			this.PrintARInvoiceAnalysisCheckBox.Name = "PrintARInvoiceAnalysisCheckBox";
			this.PrintARInvoiceAnalysisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 22, true);
			this.PrintARInvoiceAnalysisCheckBox.TabIndex = 3;
			// 
			// PrintAPInvoiceAnalysisCheckBox
			// 
			this.PrintAPInvoiceAnalysisCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintAPInvoiceAnalysisCheckBox, "PrintAPInvoiceAnalysis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDocumentPrinter)(null)).PrintAPInvoiceAnalysis)));
			this.PrintAPInvoiceAnalysisCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|37c4e999-b80e-4620-9a03-f1ba1e408300", "Print Payables Invoice Analysis");
			this.PrintAPInvoiceAnalysisCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintAPInvoiceAnalysisCheckBox.Checked = true;
			this.PrintAPInvoiceAnalysisCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintAPInvoiceAnalysisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintAPInvoiceAnalysisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 136, true);
			this.PrintAPInvoiceAnalysisCheckBox.Name = "PrintAPInvoiceAnalysisCheckBox";
			this.PrintAPInvoiceAnalysisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 22, true);
			this.PrintAPInvoiceAnalysisCheckBox.TabIndex = 4;
			// 
			// PrintProfitRecognitionByDateSummary
			// 
			this.PrintProfitRecognitionByDateSummary.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintProfitRecognitionByDateSummary, "PrintProfitRecognitionByDateSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDocumentPrinter)(null)).PrintProfitRecognitionByDateSummary)));
			this.PrintProfitRecognitionByDateSummary.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|d013c25b-ab61-4bd7-830a-d60b94436c6a", "Print Profit Recognition by Date Summary");
			this.PrintProfitRecognitionByDateSummary.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintProfitRecognitionByDateSummary.Checked = true;
			this.PrintProfitRecognitionByDateSummary.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintProfitRecognitionByDateSummary.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintProfitRecognitionByDateSummary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 46, true);
			this.PrintProfitRecognitionByDateSummary.Name = "PrintProfitRecognitionByDateSummary";
			this.PrintProfitRecognitionByDateSummary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.PrintProfitRecognitionByDateSummary.TabIndex = 1;
			// 
			// PrintJobRevenueJournalAnalysisCheckBox
			// 
			this.PrintJobRevenueJournalAnalysisCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintJobRevenueJournalAnalysisCheckBox, "PrintJobRevenueJournalAnalysis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDocumentPrinter)(null)).PrintJobRevenueJournalAnalysis)));
			this.PrintJobRevenueJournalAnalysisCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|ed741b38-2ce2-4d0d-a8b7-697001fce879", "Print Job Revenue Journal Analysis");
			this.PrintJobRevenueJournalAnalysisCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintJobRevenueJournalAnalysisCheckBox.Checked = true;
			this.PrintJobRevenueJournalAnalysisCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintJobRevenueJournalAnalysisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintJobRevenueJournalAnalysisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 164, true);
			this.PrintJobRevenueJournalAnalysisCheckBox.Name = "PrintJobRevenueJournalAnalysisCheckBox";
			this.PrintJobRevenueJournalAnalysisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 22, true);
			this.PrintJobRevenueJournalAnalysisCheckBox.TabIndex = 5;
			// 
			// JobProfitDocumentPrintingForm
			// 
			this.AcceptButton = this.PrintButton;
			this.AutoSize = true;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitDocumentPrintingForm|089d7067-2a78-46a8-9af0-8a293bcd8e5c", "Print Job Profit Document");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 263, true);
			this.ControlBox = false;
			this.Controls.Add(this.PrintJobRevenueJournalAnalysisCheckBox);
			this.Controls.Add(this.PrintProfitRecognitionByDateSummary);
			this.Controls.Add(this.PrintAPInvoiceAnalysisCheckBox);
			this.Controls.Add(this.PrintARInvoiceAnalysisCheckBox);
			this.Controls.Add(this.PrintChargeDetailCheckBox);
			this.Controls.Add(this.PrintChargeSummaryCheckBox);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(JobDocumentPrinter);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.JobDocumentPrinter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "JobProfitDocumentPrintingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.PrintChargeSummaryCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintChargeDetailCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintARInvoiceAnalysisCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintAPInvoiceAnalysisCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintProfitRecognitionByDateSummary, 0);
			this.Controls.SetChildIndex(this.PrintJobRevenueJournalAnalysisCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}