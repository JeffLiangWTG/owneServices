using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolJobProfitLossControl
	{


		#region Component Designer generated code

		private ZGuidFindBox SummaryJobNumberFindbox;
		private ZGuidFindBox JobNumberFindbox;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.SummaryJobNumberFindbox = new ZGuidFindBox();
			this.JobNumberFindbox = new ZGuidFindBox();
			this.SummaryRecognizedChargesDropEdit.SuspendLayout();
			this.DetailsRecognizedChargesDropEdit.SuspendLayout();
			this.SummaryJobProfitLossTotalsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossSummaryGrid)).BeginInit();
			this.ProfitLossSummaryGrid.SuspendLayout();
			this.DetailsJobProfitLossTotalsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossGrid)).BeginInit();
			this.ProfitLossGrid.SuspendLayout();
			this.GlobalJobProfitLossTotalsControl.SuspendLayout();
			this.SummaryTabPanel.SuspendLayout();
			this.DetailsTabPanel.SuspendLayout();
			this.SummaryChargeCodeFindBox.SuspendLayout();
			this.SummaryDepartmentFindBox.SuspendLayout();
			this.SummaryBranchFindBox.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.ChargeCodeFindBox.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.GlobalJobCostingTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryJobNumberFindbox.SuspendLayout();
			this.JobNumberFindbox.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsJobProfitReportButton
			// 
			this.DetailsJobProfitReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(882, 348, true);
			this.DetailsJobProfitReportButton.TabIndex = 10;
			// 
			// SummaryRecognizedChargesDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SummaryRecognizedChargesDropEdit, "ProfitLossContainer.Filter+RecognizedChargesFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.RecognizedChargesFilter)));
			this.SummaryRecognizedChargesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 52, true);
			this.SummaryRecognizedChargesDropEdit.TabIndex = 4;
			// 
			// JobProfitReportButton
			// 
			this.JobProfitReportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitLossControl|42ee9171-6f88-4b8e-9b7f-f1affe313503", "Print Job Profit");
			this.JobProfitReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(882, 350, true);
			this.JobProfitReportButton.TabIndex = 9;
			// 
			// DetailsRecognizedChargesDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DetailsRecognizedChargesDropEdit, "ProfitLossContainer.Filter+RecognizedChargesFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.RecognizedChargesFilter)));
			this.DetailsRecognizedChargesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 53, true);
			// 
			// SummaryJobProfitLossTotalsControl
			// 
			this.BindingSource.SetBindingMember(this.SummaryJobProfitLossTotalsControl, "ProfitLossContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IJobProfitLoss)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)))));
			this.SummaryJobProfitLossTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 320, true);
			// 
			// ProfitLossSummaryGrid
			// 
			this.BindingSource.SetBindingMember(this.ProfitLossSummaryGrid, "ProfitLossContainer.ProfitLossSummaryFilteredDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_JobLocalReferenceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_Revenue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_WIP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_Cost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_Accrual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_LineAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_MarginPercentage)));
			this.ProfitLossSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 98, true);
			this.ProfitLossSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 216, true);
			this.ProfitLossSummaryGrid.TabIndex = 7;
			// 
			// DetailsJobProfitLossTotalsControl
			// 
			this.BindingSource.SetBindingMember(this.DetailsJobProfitLossTotalsControl, "ProfitLossContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IJobProfitLoss)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)))));
			this.DetailsJobProfitLossTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 320, true);
			// 
			// ProfitLossGrid
			// 
			this.BindingSource.SetBindingMember(this.ProfitLossGrid, "ProfitLossContainer.ProfitLossFilteredDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_ConsolNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_LineAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_FullyPaidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_JobLocalReferenceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_RecognizedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_RecognitionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_ReversalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_SystemCreateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_AuditedBy)));
			this.ProfitLossGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 94, true);
			this.ProfitLossGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 220, true);
			this.ProfitLossGrid.TabIndex = 8;
			// 
			// GlobalJobProfitLossTotalsControl
			// 
			this.BindingSource.SetBindingMember(this.GlobalJobProfitLossTotalsControl, "ProfitLossContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IJobProfitLoss)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)))));
			this.GlobalJobProfitLossTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 318, true);
			// 
			// ChargeHidingMessageLabel
			// 
			this.ChargeHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("91e60f12-0242-48b5-9273-091ccc2b75f9", "Charges entered / posted to branch / dept outside your login permission are not listed.");
			// 
			// SummaryChargeHidingMessageLabel
			// 
			this.SummaryChargeHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("72cbdb47-6a63-4a9a-a383-7e1de0c68eec", "Charges entered / posted to branch / dept outside your login permission are not listed.");
			// 
			// SummaryTabPanel
			// 
			this.SummaryTabPanel.Controls.Add(this.SummaryJobNumberFindbox);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryChargeHidingMessageLabel, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryRecognizedChargesDropEdit, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryJobProfitLossTotalsControl, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.JobProfitReportButton, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryDepartmentFindBox, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryChargeCodeFindBox, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryJobNumberFindbox, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.ProfitLossSummaryGrid, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryFindButton, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryBranchFindBox, 0);
			this.SummaryTabPanel.Controls.SetChildIndex(this.SummaryClearButton, 0);
			// 
			// DetailsTabPanel
			// 
			this.DetailsTabPanel.Controls.Add(this.JobNumberFindbox);
			this.DetailsTabPanel.Controls.SetChildIndex(this.ChargeHidingMessageLabel, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.DetailsRecognizedChargesDropEdit, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.DetailsJobProfitLossTotalsControl, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.ShowReversedTransCheckBox, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.DetailsJobProfitReportButton, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.DepartmentFindBox, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.JobNumberFindbox, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.ProfitLossGrid, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.FindButton, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.ChargeCodeFindBox, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.BranchFindBox, 0);
			this.DetailsTabPanel.Controls.SetChildIndex(this.ClearButton, 0);
			// 
			// SummaryChargeCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.SummaryChargeCodeFindBox, "ProfitLossContainer.Filter+ChargeCodeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.ChargeCodeFilter)));
			this.SummaryChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 47, true);
			this.SummaryChargeCodeFindBox.TabIndex = 1;
			// 
			// SummaryDepartmentFindBox
			// 
			this.BindingSource.SetBindingMember(this.SummaryDepartmentFindBox, "ProfitLossContainer.Filter+DepartmentFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.DepartmentFilter)));
			this.SummaryDepartmentFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitLossControl|867c07ce-e08d-4af1-98b5-208e0e5467dd", "Department");
			this.SummaryDepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 25, true);
			this.SummaryDepartmentFindBox.TabIndex = 3;
			// 
			// SummaryBranchFindBox
			// 
			this.BindingSource.SetBindingMember(this.SummaryBranchFindBox, "ProfitLossContainer.Filter+BranchFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.BranchFilter)));
			this.SummaryBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 71, true);
			// 
			// ClearButton
			// 
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(886, 44, true);
			this.ClearButton.TabIndex = 7;
			// 
			// ShowReversedTransCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ShowReversedTransCheckBox, "ProfitLossContainer.Filter+ShowReversedFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.ShowReversedFilter)));
			this.ShowReversedTransCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitLossControl|b94aafb0-cc88-4970-8cf1-1e8f51220469", "Show Reversed Transactions");
			this.ShowReversedTransCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 22, true);
			this.ShowReversedTransCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 45, true);
			this.ShowReversedTransCheckBox.TabIndex = 5;
			// 
			// DepartmentFindBox
			// 
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "ProfitLossContainer.Filter+DepartmentFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.DepartmentFilter)));
			this.DepartmentFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitLossControl|cab3eb7f-c03f-4c56-9eed-ef04aada961f", "Department");
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 26, true);
			// 
			// BranchFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchFindBox, "ProfitLossContainer.Filter+BranchFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.BranchFilter)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 71, true);
			this.BranchFindBox.TabIndex = 2;
			// 
			// ChargeCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeCodeFindBox, "ProfitLossContainer.Filter+ChargeCodeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.ChargeCodeFilter)));
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 47, true);
			this.ChargeCodeFindBox.TabIndex = 1;
			// 
			// FindButton
			// 
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(885, 15, true);
			this.FindButton.TabIndex = 6;
			// 
			// SummaryFindButton
			// 
			this.SummaryFindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(886, 15, true);
			this.SummaryFindButton.TabIndex = 5;
			// 
			// SummaryClearButton
			// 
			this.SummaryClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(886, 39, true);
			this.SummaryClearButton.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IJobCostingPlugIn);
			// 
			// SummaryJobNumberFindbox
			// 
			this.SummaryJobNumberFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SummaryJobNumberFindbox, "ProfitLossContainer.Filter+JobNumberFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.JobNumberFilter)));
			this.SummaryJobNumberFindbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitLossControl|9877e35c-8da2-4885-893f-f3d3b4d25dfa", "Job Number");
			this.SummaryJobNumberFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 23, true);
			this.SummaryJobNumberFindbox.Name = "SummaryJobNumberFindbox";
			this.SummaryJobNumberFindbox.PopupCaption = null;
			this.SummaryJobNumberFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SummaryJobNumberFindbox.TabIndex = 0;
			// 
			// JobNumberFindbox
			// 
			this.JobNumberFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobNumberFindbox, "ProfitLossContainer.Filter+JobNumberFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((IJobProfitLoss)(((System.Collections.IList)(((IJobCostingPlugIn)(null)).ProfitLossContainer)).SyncRoot)).Filter.JobNumberFilter)));
			this.JobNumberFindbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolJobProfitLossControl|d13c8e0a-0cc5-4449-b979-48a8150dc47d", "Job Number");
			this.JobNumberFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 23, true);
			this.JobNumberFindbox.Name = "JobNumberFindbox";
			this.JobNumberFindbox.PopupCaption = null;
			this.JobNumberFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.JobNumberFindbox.TabIndex = 0;
			// 
			// ConsolJobProfitLossControl
			// 
			this.Name = "ConsolJobProfitLossControl";
			this.SummaryRecognizedChargesDropEdit.ResumeLayout(true);
			this.SummaryRecognizedChargesDropEdit.PerformLayout();
			this.DetailsRecognizedChargesDropEdit.ResumeLayout(true);
			this.DetailsRecognizedChargesDropEdit.PerformLayout();
			this.SummaryJobProfitLossTotalsControl.ResumeLayout(true);
			this.SummaryJobProfitLossTotalsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossSummaryGrid)).EndInit();
			this.ProfitLossSummaryGrid.ResumeLayout(false);
			this.ProfitLossSummaryGrid.PerformLayout();
			this.DetailsJobProfitLossTotalsControl.ResumeLayout(true);
			this.DetailsJobProfitLossTotalsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossGrid)).EndInit();
			this.ProfitLossGrid.ResumeLayout(false);
			this.ProfitLossGrid.PerformLayout();
			this.GlobalJobProfitLossTotalsControl.ResumeLayout(true);
			this.GlobalJobProfitLossTotalsControl.PerformLayout();
			this.SummaryTabPanel.ResumeLayout(false);
			this.SummaryTabPanel.PerformLayout();
			this.DetailsTabPanel.ResumeLayout(false);
			this.DetailsTabPanel.PerformLayout();
			this.SummaryChargeCodeFindBox.ResumeLayout(true);
			this.SummaryChargeCodeFindBox.PerformLayout();
			this.SummaryDepartmentFindBox.ResumeLayout(true);
			this.SummaryDepartmentFindBox.PerformLayout();
			this.SummaryBranchFindBox.ResumeLayout(true);
			this.SummaryBranchFindBox.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.ChargeCodeFindBox.ResumeLayout(true);
			this.ChargeCodeFindBox.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.SummaryTabPage.ResumeLayout(false);
			this.SummaryTabPage.PerformLayout();
			this.GlobalJobCostingTabPage.ResumeLayout(false);
			this.GlobalJobCostingTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SummaryJobNumberFindbox.ResumeLayout(true);
			this.SummaryJobNumberFindbox.PerformLayout();
			this.JobNumberFindbox.ResumeLayout(true);
			this.JobNumberFindbox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}