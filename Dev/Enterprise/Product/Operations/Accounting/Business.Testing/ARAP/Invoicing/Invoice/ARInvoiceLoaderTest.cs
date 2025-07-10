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
	[TestedType(typeof(ARInvoice.Loader))]
	class ARInvoiceLoaderTest : LoaderTestCase
	{
		public void TestLoadTop1()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			GlbBranch branch1 = GlbBranch.CurrentBranch;
			//branch1.GB_Code = "~12";
			GlbBranch branch2 = GlbBranch.CurrentBranch;
			//branch2.GB_Code = "~13";

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			ARInvoice invoice1 = CreateARInvoice(job1, branch1.PK, chargeCode1, false);

			ARInvoice invoice2 = CreateARInvoice(job2, branch1.PK, chargeCode1, false);

			ARInvoice invoice3 = CreateARInvoice(job1, branch2.PK, chargeCode1, false);

			ARInvoice invoice4 = CreateARInvoice(job1, branch2.PK, chargeCode2, false);

			//reversed
			ARInvoice invoice5 = CreateARInvoice(job3, branch1.PK, chargeCode1, true);

			APInvoice apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_TransactionNum = "000001";
			apInvoice.AH_JH = job1.PK;
			apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			apInvoice.AH_GB = branch1.PK;
			apInvoice.AH_IsCancelled = false;

			APInvoiceLine apInvoiceLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			apInvoiceLine.AL_AC = chargeCode3.PK;
			apInvoiceLine.AL_JH = job1.PK;
			apInvoiceLine.AL_GE = Env.CurrentDepartment.PK;
			apInvoiceLine.AL_GB = apInvoice.Company.Branches[0].PK;
			TestObjectCreator.CreateJobCharge(apInvoiceLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			AssertNotNull(new ARInvoice.Loader(Factory).LoadTop1NotReversed(job1.PK, new ZGuid[] { chargeCode1.PK }));
			AssertNull(new ARInvoice.Loader(Factory).LoadTop1NotReversed(job3.PK, new ZGuid[] { chargeCode1.PK }));
			AssertNull(new ARInvoice.Loader(Factory).LoadTop1NotReversed(job1.PK, new ZGuid[] { chargeCode3.PK }));

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertNotNull(new ARInvoice.Loader(newFactory).LoadTop1NotReversed(job1.PK, new ZGuid[] { chargeCode1.PK }));
			AssertNull(new ARInvoice.Loader(newFactory).LoadTop1NotReversed(job3.PK, new ZGuid[] { chargeCode1.PK }));
			AssertNull(new ARInvoice.Loader(newFactory).LoadTop1NotReversed(job1.PK, new ZGuid[] { chargeCode3.PK }));
		}

		public void TestLoadIncludeCreditNotes()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			GlbBranch branch1 = GlbBranch.CurrentBranch;
			//branch1.GB_Code = "~12";
			GlbBranch branch2 = GlbBranch.CurrentBranch;
			//branch2.GB_Code = "~13";

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			ARInvoice invoice1 = CreateARInvoice(job1, branch1.PK, chargeCode1, false);

			ARInvoice invoice2 = CreateARInvoice(job2, branch1.PK, chargeCode1, false, TransactionTypes.CreditNote);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertNotNull(new ARInvoice.Loader(newFactory).LoadTop1NotReversed(job2.PK, new ZGuid[] { chargeCode1.PK }));
		}

		ARInvoice CreateARInvoice(Job job, ZGuid branchPK, AccChargeCode chargeCode, bool isCancelled)
		{
			return CreateARInvoice(job, branchPK, chargeCode, isCancelled, TransactionTypes.Invoice);
		}

		ARInvoice CreateARInvoice(Job job, ZGuid branchPK, AccChargeCode chargeCode, bool isCancelled, string transactionType)
		{
			ARInvoice result = Factory.New<ARInvoice>();
			result.AH_JH = job.PK;
			result.AH_Ledger = LedgerTypes.AccountsReceivable;
			result.AH_GB = branchPK;
			result.AH_IsCancelled = isCancelled;
			result.AH_TransactionType = transactionType;

			if (isCancelled)
			{
				TransactionMatchLink matchLink = ((IMatching)result).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
				matchLink.AP_AH = result.PK;
				matchLink.AP_Amount = result.AH_OutstandingAmount;
				TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			}

			ARInvoiceLine invoiceLine = (ARInvoiceLine)result.Lines.AddNew();
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_GE = Env.CurrentDepartment.PK;
			invoiceLine.AL_GB = branchPK;

			if (!isCancelled)
			{
				TestObjectCreator.CreateJobCharge(invoiceLine, job, chargeCode, TestObjectCreator.AUD);
			}

			return result;
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new ARInvoice.Loader(Factory);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		[TestedType(typeof(ARInvoice))]
		public class ARInvoiceMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<ARInvoice>();
			}
		}
	}
}
