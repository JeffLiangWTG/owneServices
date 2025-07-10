using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.JobInvoicePrintingFilter;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(LightWeightTransactionHeaderCollectionView))]
	public class LightWeightTransactionHeaderCollectionViewTest : BusinessObjectCollectionViewTestCase<LightWeightTransactionHeaderCollectionView>
	{
		public void TestIsThisPartOfTheCollectionNotInitializingJob()
		{
			var parent = Factory.NewWithValidTestData<CommonCartage>();
			var job = new Job.Loader(parent).TryCreateWithoutMutexForTestOnly();
			job.JH_JobNum = "C001";
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100m;

			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			var postedInvoices = postManager.Poster.PostedInvoices;
			Factory.Save();
			AssertEquals("Precondition: one invoice was posted", 1, postedInvoices.Count);

			var printingFilter = job.PrintingFilter;
			var printingFilterFactory = printingFilter.Transactions.Factory;
			var dBhitCountBeforeRefresh = printingFilterFactory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			printingFilter.RefreshInvoiceList();
			AssertEquals("Transactions should contain 1 invoice", 1, printingFilter.FilteredTransactions.Count);

			var jobInPrintingFilterFactory = printingFilterFactory.Load<Job>(postedInvoices[0].AH_JH);
			AssertEquals("Should not initialize the job details", false, jobInPrintingFilterFactory.DefaultValuesHasBeenAssigned);

			var dBhitCountAfterRefresh = printingFilterFactory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			AssertEquals("Refreshing invoice list should not load JobCharge", 0, dBhitCountAfterRefresh - dBhitCountBeforeRefresh);
		}

		#region Implementation

		protected override LightWeightTransactionHeaderCollectionView GetCollectionToTest()
		{
			var transactions = new LightWeightTransactionHeaderCollection(Factory, true);
			return new LightWeightTransactionHeaderCollectionView(transactions);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ARInvoice>();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
