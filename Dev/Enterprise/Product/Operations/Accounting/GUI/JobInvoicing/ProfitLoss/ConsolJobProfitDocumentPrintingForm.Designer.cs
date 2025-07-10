using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolJobProfitDocumentPrintingForm
	{


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.PrintJobByJobSummaryCheckBox = new ZCheckBox();
			this.PrintContainerPackingSummaryCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PrintChargeDetailCheckBox
			// 
			this.PrintChargeDetailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 75, true);
			// 
			// PrintARInvoiceAnalysisCheckBox
			// 
			this.PrintARInvoiceAnalysisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 104, true);
			// 
			// PrintAPInvoiceAnalysisCheckBox
			// 
			this.PrintAPInvoiceAnalysisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 133, true);
			// 
			// PrintButton
			// 
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 276, true);
			this.PrintButton.TabIndex = 8;
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 276, true);
			this.CloseButton.TabIndex = 9;
			// 
			// PrintProfitRecognitionByDateSummary
			// 
			this.PrintProfitRecognitionByDateSummary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 41, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 319, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ConsolJobDocumentPrinter);
			// 
			// PrintJobByJobSummaryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PrintJobByJobSummaryCheckBox, "PrintJobByJobSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolJobDocumentPrinter)(null)).PrintJobByJobSummary)));
			this.PrintJobByJobSummaryCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitDocumentPrintingForm|e27b572c-3b44-41ff-b2b0-6e55207cf02c", "Print Job By Job Summary");
			this.PrintJobByJobSummaryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintJobByJobSummaryCheckBox.Checked = true;
			this.PrintJobByJobSummaryCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintJobByJobSummaryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintJobByJobSummaryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 196, true);
			this.PrintJobByJobSummaryCheckBox.Name = "PrintJobByJobSummaryCheckBox";
			this.PrintJobByJobSummaryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.PrintJobByJobSummaryCheckBox.TabIndex = 6;
			// 
			// PrintContainerPackingSummaryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PrintContainerPackingSummaryCheckBox, "PrintContainerPackingSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolJobDocumentPrinter)(null)).PrintContainerPackingSummary)));
			this.PrintContainerPackingSummaryCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitDocumentPrintingForm|2bff03c9-4164-4eac-9fb2-3c4b1891c0a0", "Print Container Packing Summary");
			this.PrintContainerPackingSummaryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintContainerPackingSummaryCheckBox.Checked = true;
			this.PrintContainerPackingSummaryCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PrintContainerPackingSummaryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintContainerPackingSummaryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 223, true);
			this.PrintContainerPackingSummaryCheckBox.Name = "PrintContainerPackingSummaryCheckBox";
			this.PrintContainerPackingSummaryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.PrintContainerPackingSummaryCheckBox.TabIndex = 7;
			// 
			// ConsolJobProfitDocumentPrintingForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitDocumentPrintingForm|089d7067-2a78-46a8-9af0-8a293bcd8e5c", "Print Consol Job Profit Document");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 341, true);
			this.Controls.Add(this.PrintContainerPackingSummaryCheckBox);
			this.Controls.Add(this.PrintJobByJobSummaryCheckBox);
			this.DataSourceType = typeof(ConsolJobDocumentPrinter);
			this.Name = "ConsolJobProfitDocumentPrintingForm";
			this.Controls.SetChildIndex(this.PrintChargeSummaryCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintChargeDetailCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintARInvoiceAnalysisCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintAPInvoiceAnalysisCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintProfitRecognitionByDateSummary, 0);
			this.Controls.SetChildIndex(this.PrintJobRevenueJournalAnalysisCheckBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintJobByJobSummaryCheckBox, 0);
			this.Controls.SetChildIndex(this.PrintContainerPackingSummaryCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}