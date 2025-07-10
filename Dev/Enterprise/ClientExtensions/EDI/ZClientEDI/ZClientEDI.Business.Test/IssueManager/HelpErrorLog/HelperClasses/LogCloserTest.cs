using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class LogCloserTest : TestCaseWithFactory
	{
		public void TestClose_NoError()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			Factory.Save();

			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.RunPreSaveValidation();
			Assert("Save without Error", !log1.HasNotifications());

			ZDateTime dateTime = ZDateTime.UtcNow;
			dateTime = dateTime.AddSeconds(-dateTime.Second);
			dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.RunPreSaveValidation();
			Assert("Save without Error", !log2.HasNotifications());

			Factory.Save();

			AssertEquals("Fixed Date should be empty", ZDate.Empty, log1.HE_FixedDate);
			AssertEquals("Fixed Date should be empty", ZDate.Empty, log2.HE_FixedDate);

			LogCloser closer = new LogCloser(new BusinessObject[] { log1, log2 });
			closer.Close();

			AssertEquals("Should close with no errors", "", closer.Errors);
			AssertEquals("Should not return as changed in DB", false, closer.HasDBChanged);

			AssertNotEquals("Fixed Date should not be empty", ZDate.Empty, log1.HE_FixedDate);

			AssertNotEquals("Fixed Date should not be empty", ZDate.Empty, log2.HE_FixedDate);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			log1 = newFactory.Load<EdiHelpErrorLog>(log1.PK);
			AssertNotEquals("FixedDate", ZDate.Empty, log1.HE_FixedDate);

			log2 = newFactory.Load<EdiHelpErrorLog>(log2.PK);
			AssertNotEquals("FixedDate", ZDate.Empty, log2.HE_FixedDate);
		}

		public void TestClose_ChangedInDB()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.RunPreSaveValidation();
			Assert("Save without Error", !log1.HasNotifications());

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.RunPreSaveValidation();
			Assert("Save without Error", !log2.HasNotifications());

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			BusinessObject otherLog2 = otherFactory.Load<EdiHelpErrorLog>(log2.PK);
			otherLog2.Delete();
			otherFactory.Save();

			LogCloser closer = new LogCloser(new BusinessObject[] { log1, log2 });
			closer.Close();

			AssertEquals("Should return no errors", "", closer.Errors);
			AssertEquals("Should return as changed in DB", true, closer.HasDBChanged);
		}

		public void TestClose_FixedDateIsSetToLatestExe()
		{
			int year = ZDateTime.UtcNow.Year;
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = log1.Occurrences.AddNew();
			occurrence1.HO_EXEDateTime = new ZDateTime(year - 3, 10, 10, 10, 10, 10);
			HelpErrorLogOccurrence occurrence2 = log1.Occurrences.AddNew();
			occurrence2.HO_EXEDateTime = new ZDateTime(year - 2, 10, 10, 10, 10, 10);

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FixedDate = new ZDateTime(year - 1, 10, 10, 10, 10, 10);
			Factory.Save();

			AssertEquals("Pre-condition: Fixed Date should be empty", ZDate.Empty, log1.HE_FixedDate);
			AssertEquals("Pre-condition: Fixed Date should be filled", new ZDateTime(year - 1, 10, 10, 10, 10, 10), log2.HE_FixedDate);

			LogCloser closer = new LogCloser(new BusinessObject[] { log1 });
			closer.Close();

			AssertEquals("Fixed date should be set to the latest exe, with seconds rounded off", new ZDateTime(year - 2, 10, 10, 10, 10, 00), log1.HE_FixedDate);
			AssertEquals("Fixed date should not be reset - let sleeping dogs lie", new ZDateTime(year - 1, 10, 10, 10, 10, 10), log2.HE_FixedDate);
		}

		public void TestErrorLogReturnError()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			Factory.Save();
			LogCloser closer = new LogCloser(new BusinessObject[] { log1, log2 });
			AssertNoExceptionThrown("Expected not throw ToString() called on IEnumerable<INotification> when concat string in LogCloser.Close",
				() => closer.Close());
		}
	}
}
