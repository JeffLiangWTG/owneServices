using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class LVXFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestFilterGridColorContextKey()
		{
			var decs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new LVXModule())
			using (var filterControl = new LVXFilterStripControl(decs, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals(ModuleIDs.Customs.CA.CALVXJobs.Name, filterControl.FilteredGrid.ColorContextKey);
			}
		}

		public void TestFilteredGridFields()
		{
			var decs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new LVXFilterStripBusinessObject();

			using (var filterControl = new LVXFilterStripControl(decs, filterBO))
			{
				filterControl.Show();
				AssertNotNull("Period", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_Period));
				AssertNotNull("Release Date", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_EntryAuthorisationDate));
				AssertNotNull("Branch", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_GB));
				AssertNotNull("Broker", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_GS_NKCusAgent));
				AssertNotNull("Transaction Number", filterControl.FilteredGrid.GetColumnStyle("DeclarationNumber"));
				AssertNotNull("LVS ID", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+JZ_InvoiceNumber"));
				AssertNotNull("Importer", filterControl.FilteredGrid.GetColumnStyle("LVSImporter+PK"));
				AssertNotNull("Vendor", filterControl.FilteredGrid.GetColumnStyle("LVSSupplier+PK"));
				AssertNotNull("Direct Shipment Date", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+JZ_ValuationDateOverride"));
				AssertNotNull("Port of Clearance", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+CA_PortOfClearance"));
				AssertNotNull("Country/Region of Origin", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+JZ_RN_NKDefaultOrigin"));
				AssertNotNull("INCO Terms", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+JZ_IncoTerm"));
				AssertNotNull("Treatment Code", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+CA_TreatmentCode"));
				AssertNotNull("Next Milestone Event", filterControl.FilteredGrid.GetColumnStyle("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent"));
				AssertNotNull("Next Milestone Desc.", filterControl.FilteredGrid.GetColumnStyle("WorkflowItems+Milestones+NextMilestone+P9_Description"));
				AssertNotNull("Next Milestone ETD", filterControl.FilteredGrid.GetColumnStyle("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding"));
				AssertNotNull("Last Milestone Event", filterControl.FilteredGrid.GetColumnStyle("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent"));
				AssertNotNull("Last Milestone Desc.", filterControl.FilteredGrid.GetColumnStyle("WorkflowItems+Milestones+LastMilestone+P9_Description"));
				AssertNotNull("Last Milestone ATD", filterControl.FilteredGrid.GetColumnStyle("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding"));
				AssertNotNull("Customs Value", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalCustomsValueInLocalCurrency));
				AssertNotNull("Declaration Currency", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+JZ_RX_NKInvoice_Currency"));
				AssertNotNull("Duty Amount", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalNormalDuty));
				AssertNotNull("Excise Tax", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalExciseTax));
				AssertNotNull("GST", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalGST));
				AssertNotNull("SIMA", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalSimaDuty));
				AssertNotNull("Total Billed (Duty and tax)", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalBilledAmount));
				AssertNotNull("Total Duty and Tax", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalDutyAndTax));
				AssertNotNull("Total Invoiced (DSB)", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalInvoicedAmount));
				AssertNotNull("Total Outstanding", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalOutstandingAmount));
				AssertNotNull("Total Payable", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalAmountPayable));
				AssertNotNull("Ready for Consolidation?", filterControl.FilteredGrid.GetColumnStyle("LVXInvoiceHeader+CA_ReadyForConsolidation"));
			}
		}
	}
}
