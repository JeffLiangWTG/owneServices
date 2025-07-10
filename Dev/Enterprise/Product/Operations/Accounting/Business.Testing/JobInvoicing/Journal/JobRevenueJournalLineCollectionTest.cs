using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalLineCollection))]
	class JobRevenueJournalLineCollectionTest : DependentTransactionLineCollectionTest
	{
		public void TestCopyDownPreviousItemDetails()
		{
			Job job = (new BusinessObjectFactory()).NewJobWithValidTestDataForTesting<Job>();
			job.Factory.Save();

			Journal.AH_Desc = "Journal Desc";
			TestCollection.AddNew();

			TestCollection[0].AL_AC = Env.Registry.GetFreightChargeCode(Env.CurrentCompany.PK);
			TestCollection[0].AL_JH = job.PK;
			TestCollection[0].AL_Desc = "Description";
			TestCollection[0].AL_GB = GlbBranch.CurrentBranch.PK;
			TestCollection[0].AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestCollection[0].AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.UnitedStates;
			TestCollection[0].AL_ExchangeRate = 2M;
			TestCollection[0].OSUnsignedLineAmount = 50.00M;
			TestCollection[0].DebitCreditSign = nameof(DebitCredit.DR);

			TestCollection.SuspendValidation();
			TestCollection.AddNew();
			TestCollection.ResumeValidation();
			AssertEquals("Validation must be suspended, if no on reversing we have errors because after AddNew in GetReversedTransaction we set SuspendValidation. So errors added during adding new collection element can't be cleared then.",
				false, TestCollection[1].HasNotifications());

			AssertEquals("New Row should carry forward Charge Code", Env.Registry.GetFreightChargeCode(Env.CurrentCompany.PK), TestCollection[1].AL_AC);
			AssertEquals("New Row should carry forward Job", job.PK, TestCollection[1].AL_JH);
			AssertEquals("New Row should carry forward Description", "Description", TestCollection[1].AL_Desc);
			AssertEquals("New Row should carry forward Branch", GlbBranch.CurrentBranch.PK.ToGuid(), TestCollection[1].AL_GB);
			AssertEquals("New Row should carry forward Department", GlbDepartment.CurrentDepartment.PK.ToGuid(), TestCollection[1].AL_GE);
			AssertEquals("New Row should carry forward Currency", Constants.CurrencyCodes.UnitedStates, TestCollection[1].AL_RX_NKTransactionCurrency);
			AssertEquals("New Row should carry forward Exchange Rate", 2M, TestCollection[1].AL_ExchangeRate);
			AssertEquals("New Row should carry forward total amount", 50.00M, TestCollection[1].OSUnsignedLineAmount);
			AssertEquals("New Row should carry forward opposite DR/CR", nameof(DebitCredit.CR), TestCollection[1].DebitCreditSign);

			TestCollection.AddNew();
			AssertEquals("New Row should carry forward Charge Code", Env.Registry.GetFreightChargeCode(Env.CurrentCompany.PK), TestCollection[2].AL_AC);
			AssertEquals("New Row should carry forward Job", job.PK, TestCollection[2].AL_JH);
			AssertEquals("New Row should carry forward Description", "Description", TestCollection[2].AL_Desc);
			AssertEquals("New Row should carry forward Branch", GlbBranch.CurrentBranch.PK.ToGuid(), TestCollection[2].AL_GB);
			AssertEquals("New Row should carry forward Department", GlbDepartment.CurrentDepartment.PK.ToGuid(), TestCollection[2].AL_GE);
			AssertEquals("New Row should carry forward Currency", Constants.CurrencyCodes.UnitedStates, TestCollection[2].AL_RX_NKTransactionCurrency);
			AssertEquals("New Row should carry forward Exchange Rate", 2M, TestCollection[2].AL_ExchangeRate);
			AssertEquals("New Row should carry forward total amount", 0.00M, TestCollection[2].OSUnsignedLineAmount);
			AssertEquals("New Row should DR if total amount is zero.", nameof(DebitCredit.DR), TestCollection[2].DebitCreditSign);
		}

		protected JobRevenueJournalLineCollection TestCollection;
		protected JobRevenueJournal Journal;

		protected override void SetUp()
		{
			base.SetUp();
			Journal = Factory.New<JobRevenueJournal>();
			TestCollection = Journal.JournalLines;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<JobRevenueJournal>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new JobRevenueJournalLineCollection(parent);
		}
	}
}
