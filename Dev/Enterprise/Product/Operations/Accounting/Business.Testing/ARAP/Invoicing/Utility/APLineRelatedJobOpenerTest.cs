using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class APLineRelatedJobOpenerTest : TestCaseWithFactory
	{
		public void TestTryToGetReOpenRestirctionDateCount()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
				apInvoice.AddRelatedJobsForReversing_ForTestOnly(job);
				AssertEquals("Precondition", 1, apInvoice.RelatedJobsForReversing.Count());

				job.JH_Status = JobHeaderStatus.Closed.Code;
				job.TryToGetReOpenRestirctionDateCount_ForTestOnly = 0;
				APLineRelatedJobOpener.ReopenClosedJobwithSuspendedValidation(apInvoice);
				AssertEquals(2, job.TryToGetReOpenRestirctionDateCount_ForTestOnly);

				job.JH_Status = JobHeaderStatus.Working.Code;
				job.TryToGetReOpenRestirctionDateCount_ForTestOnly = 0;
				APLineRelatedJobOpener.ReopenClosedJobwithSuspendedValidation(apInvoice);
				AssertEquals(0, job.TryToGetReOpenRestirctionDateCount_ForTestOnly);
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
