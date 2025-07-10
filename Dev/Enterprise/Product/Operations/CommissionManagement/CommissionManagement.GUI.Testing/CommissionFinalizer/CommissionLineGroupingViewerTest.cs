using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionLineGroupingViewerTest : TestCaseWithFactory
	{
		public void TestShowViewForm()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			Factory.Save();

			var jobLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var jobFinalizerItem = new CommissionFinalizerLineItem(jobLine);
			var invoiceLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			invoiceLine.VCL_GroupingSourceID = invoice.PK;
			invoiceLine.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			var invoiceFinalizerItem = new CommissionFinalizerLineItem(invoiceLine);

			var jobGrouping = new CommissionFinalizerLineItemGrouping(Factory);
			jobGrouping.Init(new[] { jobFinalizerItem });
			var invoiceGrouping = new CommissionFinalizerLineItemGrouping(Factory);
			invoiceGrouping.Init(new[] { invoiceFinalizerItem });

			using (var jobForm = CommissionLineGroupingViewer.ShowViewForm(jobGrouping))
			{
				AssertNotNull(jobForm);
				AssertEquals(ControllerIDs.JobManagement, jobForm.ControllerID);
			}

			using (var invoiceForm = CommissionLineGroupingViewer.ShowViewForm(invoiceGrouping))
			{
				AssertNotNull(invoiceForm);
				AssertEquals(ControllerIDs.ARInvoice, invoiceForm.ControllerID);
			}
		}

		public void TestShowViewForm_JobRevenueJournal()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			var jrj = Factory.NewWithValidTestData<JobRevenueJournal>();

			Factory.Save();

			var jobLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var jobFinalizerItem = new CommissionFinalizerLineItem(jobLine);
			var jrjLine = Factory.NewWithValidTestData<ViewCommissionLine>();
			jrjLine.VCL_GroupingSourceID = jrj.PK;
			jrjLine.VCL_GroupingSourceTableCode = jrj.TablePrefix;
			var jrjFinalizerItem = new CommissionFinalizerLineItem(jrjLine);

			var jobGrouping = new CommissionFinalizerLineItemGrouping(Factory);
			jobGrouping.Init(new[] { jobFinalizerItem });
			var jrjGrouping = new CommissionFinalizerLineItemGrouping(Factory);
			jrjGrouping.Init(new[] { jrjFinalizerItem });

			using (var jobForm = CommissionLineGroupingViewer.ShowViewForm(jobGrouping))
			{
				AssertNotNull(jobForm);
				AssertEquals(ControllerIDs.JobManagement, jobForm.ControllerID);
			}

			using (var invoiceForm = CommissionLineGroupingViewer.ShowViewForm(jrjGrouping))
			{
				AssertNotNull(invoiceForm);
				AssertEquals(ControllerIDs.JobRevenueJournal, invoiceForm.ControllerID);
			}
		}
	}
}
