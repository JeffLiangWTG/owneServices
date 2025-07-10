using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class LogSplitterTest : TestCaseWithFactory
	{
		public void TestSplit()
		{
			EdiHelpErrorLog parentLog = Factory.New<EdiHelpErrorLog>();

			HelpErrorLogOccurrence occurrence1 = parentLog.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = parentLog.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence3 = parentLog.Occurrences.AddNew();

			ExceptionXml testXm1 = new ExceptionXmlTest.ValidXml();
			occurrence1.HO_XMLData = testXm1.OuterXml;
			occurrence3.HO_XMLData = testXm1.OuterXml;

			HelpErrorLogKey key1 = parentLog.Keys.AddNew();
			HelpErrorLogKey key2 = parentLog.Keys.AddNew();
			testXm1.PopulateFromXML();
			key1.HK_Key = testXm1.KeyFields.LogKey;
			key2.HK_Key = "SomeKeyUnknown";

			key1.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key1.HK_Key);
			key2.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key2.HK_Key);
			AssertNotEquals("Precondition; Different keys", key1.HK_HashCode, key2.HK_HashCode);

			parentLog.Factory.Save();
			int beforeCount = Factory.GetDatabaseCount(typeof(EdiHelpErrorLog));

			new LogSplitter(occurrence1).Process();

			Assert("Occurrence1 should be deleted", !parentLog.Occurrences.Contains(occurrence1));
			Assert("Occurrence2 should be unchanged", parentLog.Occurrences.Contains(occurrence2));
			Assert("Occurrence3 should be deleted", !parentLog.Occurrences.Contains(occurrence3));
			AssertEquals(true, occurrence1.IsDeleted);
			AssertEquals(false, occurrence2.IsDeleted);
			AssertEquals(true, occurrence3.IsDeleted);
			Assert("Key1 should be deleted", !parentLog.Keys.Contains(key1));
			Assert("Key2 should not be deleted", parentLog.Keys.Contains(key2));

			AssertEquals("New Record Added", beforeCount + 1, Factory.GetDatabaseCount(typeof(EdiHelpErrorLog)));
		}

		public void TestSplitAndAccessToDeletedObject()
		{
			EdiHelpErrorLog parentLog = Factory.New<EdiHelpErrorLog>();

			HelpErrorLogOccurrence occurrence1 = parentLog.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = parentLog.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence3 = parentLog.Occurrences.AddNew();

			ExceptionXml testXm1 = new ExceptionXmlTest.ValidXml();
			occurrence1.HO_XMLData = testXm1.OuterXml;
			occurrence2.HO_Company = "Test Company";

			HelpErrorLogKey key1 = parentLog.Keys.AddNew();
			HelpErrorLogKey key2 = parentLog.Keys.AddNew();
			testXm1.PopulateFromXML();
			key1.HK_Key = testXm1.KeyFields.LogKey;
			key2.HK_Key = "SomeKeyUnknown";

			key1.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key1.HK_Key);
			key2.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key2.HK_Key);
			AssertNotEquals("Precondition; Different keys", key1.HK_HashCode, key2.HK_HashCode);

			parentLog.Factory.Save();

			string clientName = ((IWorkTaskRelatedItem)parentLog).ClientName;

			new LogSplitter(occurrence1).Process();

			AssertNoExceptionThrown(delegate
			{ clientName = ((IWorkTaskRelatedItem)parentLog).ClientName; });
		}

		public void TestSplit_InvalidXml()
		{
			int beforeCount = Factory.GetDatabaseCount(typeof(EdiHelpErrorLog));
			EdiHelpErrorLog parentLog = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceToSplit = parentLog.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrenceToKeep = parentLog.Occurrences.AddNew();
			parentLog.Keys.AddNew();
			parentLog.Keys.AddNew();

			new LogSplitter(occurrenceToSplit).Process();

			Assert("Other Occurrence Not Split", parentLog.Occurrences.Contains(occurrenceToKeep));
			Assert("Occurrence Not Split due to Invalid Xml", parentLog.Occurrences.Contains(occurrenceToSplit));

			AssertEquals("No New Records Added", beforeCount, Factory.GetDatabaseCount(typeof(EdiHelpErrorLog)));
		}

		public void TestSplit_OneChild()
		{
			int beforeCount = Factory.GetDatabaseCount(typeof(EdiHelpErrorLog));
			EdiHelpErrorLog parentLog = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence = parentLog.Occurrences.AddNew();
			parentLog.Keys.AddNew();
			parentLog.Keys.AddNew();

			new LogSplitter(occurrence).Process();

			AssertEquals("No New Records Added", beforeCount, Factory.GetDatabaseCount(typeof(EdiHelpErrorLog)));
		}

		public void TestSplit_WrongParent()
		{
			int beforeCount = Factory.GetDatabaseCount(typeof(EdiHelpErrorLog));
			EdiHelpErrorLog parentLog = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence = Factory.New<HelpErrorLogOccurrence>();
			parentLog.Keys.AddNew();
			parentLog.Keys.AddNew();

			new LogSplitter(occurrence).Process();

			AssertEquals("No New Records Added", beforeCount, Factory.GetDatabaseCount(typeof(EdiHelpErrorLog)));
		}
	}
}
