using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoice.Loader))]
	class APInvoiceLoaderTest : LoaderTestCase
	{
		public void TestLoadTop1NotReversed()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			GlbBranch branch1 = GlbBranch.CurrentBranch;
			//branch1.GB_Code = "~12";
			GlbBranch branch2 = GlbBranch.CurrentBranch;
			//branch2.GB_Code = "~13";

			OrgHeader creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader creditor2 = Factory.NewWithValidTestData<OrgHeader>();

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			APInvoice invoice1 = CreateAPInvoice("1", creditor1.PK, branch1.PK, false, job1);

			APInvoice invoice2 = CreateAPInvoice("2", creditor1.PK, branch1.PK, false, job1);

			APInvoice invoice3 = CreateAPInvoice("3", creditor1.PK, branch2.PK, false, job1);

			APInvoice invoice4 = CreateAPInvoice("1", creditor2.PK, branch1.PK, false, job1);

			//reversed
			APInvoice invoice5 = CreateAPInvoice("3", creditor2.PK, branch1.PK, true, job1);

			ARInvoice arInvoice = Factory.New<ARInvoice>();
			arInvoice.AH_TransactionNum = "1";
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_OH = creditor1.PK;
			arInvoice.AH_GB = branch1.PK;

			ARInvoiceLine arInvoiceLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
			arInvoiceLine.AL_JH = job1.PK;
			arInvoiceLine.AL_GE = Env.CurrentDepartment.PK;
			arInvoiceLine.AL_GB = arInvoice.Company.Branches[0].PK;
			arInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(arInvoiceLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			AssertNotNull(new APInvoice.Loader(Factory).LoadNotReversed("1", creditor1.PK, job1.PK).Length > 0);
			AssertCollectionContains("Loaded PK should be either Invoice1 or Invoice3, as both match search criteria but order is non deterministic", new APInvoice.Loader(Factory).LoadTop1NotReversed("1", creditor1.PK, job1.PK).PK, new ZGuid[] { invoice1.PK, invoice3.PK });
			AssertEquals("Loaded PK should be Invoice4 as invoice4 is only match for criteria", invoice4.PK, new APInvoice.Loader(Factory).LoadTop1NotReversed("1", creditor2.PK, job1.PK).PK);
			AssertNull("Invoice5 matches search criteria but is reversed and so should not return", new APInvoice.Loader(Factory).LoadTop1NotReversed("3", creditor2.PK, job1.PK));

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertNotNull(new APInvoice.Loader(newFactory).LoadTop1NotReversed("1", creditor1.PK, job1.PK));
			AssertCollectionContains("Loaded PK should be either Invoice1 or Invoice3, as both match search criteria but order is non deterministic", new APInvoice.Loader(newFactory).LoadTop1NotReversed("1", creditor1.PK, job1.PK).PK, new ZGuid[] { invoice1.PK, invoice3.PK });
			AssertEquals("Loaded PK should be Invoice4 as invoice4 is only match for criteria", invoice4.PK, new APInvoice.Loader(newFactory).LoadTop1NotReversed("1", creditor2.PK, job1.PK).PK);
			AssertNull("Invoice5 matches search criteria but is reversed and so should not return", new APInvoice.Loader(newFactory).LoadTop1NotReversed("3", creditor2.PK, job1.PK));
		}

		public void TestLoadIncludingReversed()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			GlbBranch branch1 = GlbBranch.CurrentBranch;
			//branch1.GB_Code = "~12";
			//GlbBranch branch2 = company.Branches.AddNew();
			//branch2.GB_Code = "~13";

			OrgHeader creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader creditor2 = Factory.NewWithValidTestData<OrgHeader>();

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			APInvoice invoice1 = CreateAPInvoice("1", creditor1.PK, branch1.PK, true, job1);

			APInvoice invoice2 = CreateAPInvoice("1/1", creditor1.PK, branch1.PK, true, job1);

			APInvoice invoice4 = CreateAPInvoice("1", creditor2.PK, branch1.PK, false, job1);

			APInvoice invoice5 = CreateAPInvoice("1/3", creditor1.PK, branch1.PK, false, job2);

			APInvoice invoice6 = CreateAPInvoice("1/4", creditor1.PK, branch1.PK, false, job1);

			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			APInvoice[] invoices = new APInvoice.Loader(Factory).LoadIncludingReversed("1", creditor1.PK, job1.PK);

			AssertEquals(3, invoices.Length);

			bool hasSeeninvoice1 = false, hasSeeninvoice2 = false, hasSeeninvoice6 = false;
			foreach (APInvoice invoice in invoices)
			{
				if (invoice == invoice1)
				{
					hasSeeninvoice1 = true;
				}
				else if (invoice == invoice2)
				{
					hasSeeninvoice2 = true;
				}
				else if (invoice == invoice6)
				{
					hasSeeninvoice6 = true;
				}
			}

			Assert("hasSeeninvoice1", hasSeeninvoice1);
			Assert("hasSeeninvoice2", hasSeeninvoice2);
			Assert("hasSeeninvoice6", hasSeeninvoice6);
		}

		APInvoice CreateAPInvoice(ZString invoiceNumber, ZGuid creditor, ZGuid branch, bool isCancelled, JobHeader job)
		{
			APInvoice result = Factory.New<APInvoice>();
			result.AH_TransactionNum = invoiceNumber;
			result.AH_OH = creditor;
			result.AH_GB = branch;
			result.AH_GC = Env.CurrentCompany.PK;
			result.AH_IsCancelled = isCancelled;
			if (isCancelled)
			{
				TransactionMatchLink matchLink = ((IMatching)result).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
				matchLink.AP_AH = result.PK;
				matchLink.AP_Amount = result.AH_OutstandingAmount;
				TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			}

			APInvoiceLine invoiceLine = (APInvoiceLine)result.Lines.AddNew();
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_GE = Env.CurrentDepartment.PK;
			invoiceLine.AL_GB = branch;
			invoiceLine.AL_GC = Env.CurrentCompany.PK;
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;

			if (!result.AH_IsCancelled)
			{
				TestObjectCreator.CreateJobCharge(invoiceLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			}

			return result;
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new APInvoice.Loader(Factory);
		}

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
