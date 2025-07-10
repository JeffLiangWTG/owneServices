using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobProviderForInvoicingTest : TestCaseWithFactory
	{
		public void TestParentDefaultChargeCostReferenceChanged()
		{
			var provider = (IJobProviderForInvoicing)new JobProviderForInvoicing();

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			var jobHeaderInvoiceSupporter = (DummyJobHeaderParentJobInvoicingSupporter)jobHeaderParent.InvoicingSupporter;
			jobHeaderInvoiceSupporter.OperationalJobRef = "ABC";
			jobHeaderInvoiceSupporter.ShowOperationalJobRefFilter = true;
			var job = new Job.Loader(jobHeaderParent).TryCreateWithoutMutexForTestOnly();
			var chargeNone = job.Charges.AddNew();
			var chargeABC = job.Charges.AddNew();
			var chargeDEF = job.Charges.AddNew();
			chargeNone.JR_CostReference = "";
			chargeABC.JR_CostReference = "ABC";
			chargeDEF.JR_CostReference = "DEF";
			AssertContainsExactElementsInAnyOrder("Should contain the ABC charge only.", new[] { chargeABC }, job.FilteredCharges);

			jobHeaderInvoiceSupporter.OperationalJobRef = "HIJ";
			provider.ParentOperationalJobRefChanged("ABC", jobHeaderParent, false);
			AssertEquals("chargeABC should have been updated to HIJ.", "HIJ", chargeABC.JR_CostReference);
			AssertContainsExactElementsInAnyOrder("Filtering on HIJ, should contain the ABC(now HIJ) charge only.", new[] { chargeABC }, job.FilteredCharges);

			jobHeaderInvoiceSupporter.OperationalJobRef = "DEF";
			provider.ParentOperationalJobRefChanged("XXX", jobHeaderParent, false);
			AssertContainsExactElementsInAnyOrder("Should contain the DEF charge only.", new[] { chargeDEF }, job.FilteredCharges);

			jobHeaderInvoiceSupporter.OperationalJobRef = "";
			provider.ParentOperationalJobRefChanged("XXX", jobHeaderParent, false);
			AssertContainsExactElementsInAnyOrder("Should be filtering on chargeNone.", new[] { chargeNone }, job.FilteredCharges);
		}
	}
}
