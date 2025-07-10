using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	[TestsSubclassesOf(typeof(IUnmatchOnReversing))]
	public abstract class BaseIUnmatchOnReversingTestCase : BaseITransactionTestCase
	{
		protected new IUnmatchOnReversing TestObject
		{
			get { return base.TestObject as IUnmatchOnReversing; }
		}

		public void TestCalculateMaxMatchDate()
		{
			var matchLink = TestObject.UnmatchingData.MatchLinksToUnmatch.AddNew();
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday.AddDays(-1);
			matchLink.AP_AH = TestObject.PK;
			matchLink = TestObject.UnmatchingData.MatchLinksToUnmatch.AddNew();
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday.AddDays(-2);
			matchLink.AP_AH = TestObject.PK;
			AssertEquals("Precondition: New objects should be loaded by collection", 2, TestObject.UnmatchingData.MatchLinksToUnmatch.Count);
			TestObject.UnmatchingData.CalculateMaxMatchDate();
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-1), TestObject.UnmatchingData.MaxMatchDate);
		}

		public void TestMatchLinksToUnmatch()
		{
			int matchLinkCount = TestObject.UnmatchingData.MatchLinksToUnmatch.Count;
			var matchLink = TestObject.UnmatchingData.MatchLinksToUnmatch.AddNew();
			matchLink.AP_AH = TestObject.PK;
			AssertEquals("New object should be loaded by collection", matchLinkCount + 1, TestObject.UnmatchingData.MatchLinksToUnmatch.Count);
		}

		public override void TestUnmatchDateDefaulting()
		{
			AssertEquals("Default value should be empty date.", ZDateTime.Empty, TestObject.UnmatchDate);
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.BrettsBirthday });
			AssertEquals("UnmatchDate should be set to today's date.", ZDateTime.Today, TestObject.UnmatchDate.Date);
			ZDateTime expectedDate = ZDateTime.BrettsBirthday.AddDays(10);
			TestObject.UnmatchDate = expectedDate;
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Today });
			AssertEquals("UnmatchDate should not be changed if the property was populated before.", expectedDate, TestObject.UnmatchDate);
		}

		public override void TestUnmatchDateReadonly()
		{
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.BrettsBirthday, AllowBackPosting = false });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.BrettsBirthday, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", false, TestObject.UnmatchingData.AllowBackPosting);
			AssertEquals(true, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestObject.UnmatchDate_ReadOnly);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.BrettsBirthday, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.BrettsBirthday, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", true, TestObject.UnmatchingData.AllowBackPosting);
			AssertEquals(false, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestObject.UnmatchDate_ReadOnly);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = false });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Empty, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", false, TestObject.UnmatchingData.AllowBackPosting);
			AssertEquals(true, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestObject.UnmatchDate_ReadOnly);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Empty, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", true, TestObject.UnmatchingData.AllowBackPosting);
			AssertEquals(true, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestObject.UnmatchDate_ReadOnly);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Today, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Today, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", true, TestObject.UnmatchingData.AllowBackPosting);
			AssertEquals(false, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestObject.UnmatchDate_ReadOnly);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Today, AllowBackPosting = false });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Today, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", false, TestObject.UnmatchingData.AllowBackPosting);
			AssertEquals(true, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestObject.UnmatchDate_ReadOnly);
		}

		public override void TestUnmatchDateValidation()
		{
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Empty, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: UnmatchDateInfo.ReadOnly", true, TestObject.UnmatchDateInfo.ReadOnly);
			TestObject.UnmatchDate = ZDateTime.Empty;
			AssertNoErrors(TestObject.UnmatchDateInfo);

			var someDate = ZDateTime.Today.AddMonths(-2);
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = someDate, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", someDate, TestObject.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: UnmatchDateInfo.ReadOnly", false, TestObject.UnmatchDateInfo.ReadOnly);
			TestObject.UnmatchDate = someDate.AddMonths(-2);
			AssertHasErrors(TestObject.UnmatchDateInfo);

			TestObject.UnmatchDate = someDate;
			AssertHasError(TestObject.UnmatchDateInfo, "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.");

			TestObjectCreator.CreateTestPeriods(someDate);
			TestObject.RunPreSaveValidation();
			AssertNoErrors(TestObject.UnmatchDateInfo);

			TestObject.UnmatchDate = ZDateTime.Invalid;
			AssertHasError("Unmatch date is invalid", TestObject.UnmatchDateInfo, "Enter a valid selection.");

			TestObject.UnmatchDate = ZDateTime.BrettsBirthday;
			var unmatchDateInsideInvalidZDateTimeRange = string.Concat(TestObject.UnmatchDate.ToShortDateString().Remove(7), TestObject.UnmatchDate.Year);
			AssertHasError("Unmatch date is inside invalid ZDateTime range", TestObject.UnmatchDateInfo, string.Format("The date '{0}' is more than 10 years old and thus is not valid.", unmatchDateInsideInvalidZDateTimeRange));

			TestObject.UnmatchDate = someDate;
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement period = periodCalculator.GetPeriodManagementFromDate(TestObject.UnmatchDate);
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			TestObject.RunPreSaveValidation();
			AssertHasError(TestObject.UnmatchDateInfo, "This date falls into a period where the sub-ledger is closed");

			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = true;
			TestObject.RunPreSaveValidation();
			AssertHasError(TestObject.UnmatchDateInfo, "This date falls into a period where the sub-ledger is closed");

			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = false;
			TestObject.RunPreSaveValidation();
			AssertNoErrors(TestObject.UnmatchDateInfo);

			TestObject.UnmatchDate = ZDateTime.Empty;
			AssertHasErrors(TestObject.UnmatchDateInfo);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObject.UnmatchDate = ZDateTime.Today;
			AssertNoErrors(TestObject.UnmatchDateInfo);

			TestObject.UnmatchDate = ZDateTime.Today.AddDays(1);
			AssertHasError(TestObject.UnmatchDateInfo, "The date must be less or equal today's date.");
		}

		public void TestWasUnmatched()
		{
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty });
			AssertEquals(false, TestObject.UnmatchingData.WasUnmatched);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.BrettsBirthday });
			AssertEquals(true, TestObject.UnmatchingData.WasUnmatched);
		}

		public void TestAllowBackPosting()
		{
			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { AllowBackPosting = false });
			AssertEquals(false, TestObject.UnmatchingData.AllowBackPosting);

			TestObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { AllowBackPosting = true });
			AssertEquals(true, TestObject.UnmatchingData.AllowBackPosting);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
