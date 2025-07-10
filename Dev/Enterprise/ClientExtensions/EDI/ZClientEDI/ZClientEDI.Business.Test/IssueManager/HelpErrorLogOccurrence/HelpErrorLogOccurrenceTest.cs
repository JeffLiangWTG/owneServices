using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[TestedType(typeof(HelpErrorLogOccurrence))]
	class HelpErrorLogOccurrenceTest : EnterpriseBusinessObjectTestCase
	{
		HelpErrorLogOccurrence occurrence;

		public void TestDatabaseCode()
		{
			AssertEquals("DatabaseCode", ZString.Empty, Occurrence.DatabaseCode);
			LicenceHeader header = Factory.New<LicenceHeader>();
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			header.LA_LD = database.PK;
			database.LD_ServerCode = "MOO";
			Occurrence.HO_LD = database.PK;
			AssertEquals("DatabaseCode", "MOO", Occurrence.DatabaseCode);
		}

		public void TestIssue()
		{
			EdiHelpErrorLog issue = Factory.New<EdiHelpErrorLog>();
			Occurrence.HO_HE = issue.PK;
			AssertEquals("Issue", issue, Occurrence.Issue);
		}

		public void TestAddMissingWebInfoTags()
		{
			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			log.Occurrences.Add(Occurrence);
			Occurrence.HO_XMLData = "Hello";
			AssertEquals("Hello", Occurrence.HO_XMLData);

			Occurrence.HO_XMLData = "<WebInfo><EnvironmentInfo><RequestedURL>";
			AssertEquals("<WebInfo><EnvironmentInfo><RequestedURL>", Occurrence.HO_XMLData);

			Occurrence.HO_XMLData = "<EnvironmentInfo><RequestedURL><WebInfo><DatabaseInfo>";
			AssertEquals("<EnvironmentInfo><RequestedURL><WebInfo><DatabaseInfo>", Occurrence.HO_XMLData);

			Occurrence.HO_XMLData = "<EnvironmentInfo><WebInfo><RequestedURL><DatabaseInfo>";
			AssertEquals("<EnvironmentInfo><WebInfo><RequestedURL><DatabaseInfo>", Occurrence.HO_XMLData);

			Occurrence.HO_XMLData = "<EnvironmentInfo><RequestedURL>";
			AssertEquals("<EnvironmentInfo><RequestedURL>", Occurrence.HO_XMLData);

			Occurrence.HO_XMLData = "<EnvironmentInfo><RequestedURL>12345<DatabaseInfo>";
			AssertEquals("<EnvironmentInfo><WebInfo><RequestedURL>12345</WebInfo><DatabaseInfo>", Occurrence.HO_XMLData);
		}

		public void TestSavingALogDeletesXMLDataWhenAlreadyFixed()
		{
			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			log.Occurrences.Add(Occurrence);
			Occurrence.HO_XMLData = "Hello";
			Factory.Save();
			AssertEquals("HO_XMLData", "Hello", Occurrence.HO_XMLData);

			log.HE_FixedDate = ZDate.Today;
			Occurrence.HO_VersionNumber = "1.2.3.5";
			Factory.Save();
			AssertEquals("HO_XMLData", "Hello", Occurrence.HO_XMLData);

			HelpErrorLogOccurrence occurrence2 = log.Occurrences.AddNew();
			occurrence2.HO_XMLData = "Goodbye";
			AssertEquals("HO_XMLData", "Goodbye", occurrence2.HO_XMLData);
			Factory.Save();
			AssertEquals("HO_XMLData", "<EDI_Exception_Report> <ExceptionDescription>  --- Because this issue has been fixed this log has been cleared to save space. ---  </ExceptionDescription><ExceptionDetails/></EDI_Exception_Report>", occurrence2.HO_XMLData);
		}

		public void TestToString()
		{
			AssertEquals("ToString()", HelpErrorLogOccurrence.DefaultToString, Occurrence.ToString());
			ZDateTime dateTime = Occurrence.HO_ExceptionDateTime = new ZDateTime(2004, 1, 21);
			Assert("ToString() should start with HO_ExceptionDateTime.", Occurrence.ToString().StartsWith(dateTime.ToString()));
		}

		public void TestUserLoginName()
		{
			Occurrence.HO_XMLData = "<EDI_Exception_Report><LoginName>Jack</LoginName><StackTrace><Calls>1</Calls><Call>http://requirejs.org/docs/errors.html#scripterror</Call></StackTrace></EDI_Exception_Report>";
			AssertEquals("UserLoginName", "Jack", Occurrence.UserLoginName);
		}

		public void TestFinalKey()
		{
			Occurrence.HO_XMLData = "<EDI_Exception_Report><Key>TestKeyString</Key><StackTrace><Calls>1</Calls><Call>http://requirejs.org/docs/errors.html#scripterror</Call></StackTrace></EDI_Exception_Report>";

			Occurrence.HO_HE = Factory.New<EdiHelpErrorLog>().PK;
			Occurrence.Issue.LoadFinalKeys = false;
			AssertEquals("TestKeyString", Occurrence.FinalKey);

			Occurrence.ResetFinalKeyForTest();
			for (int i = 0; i < 51; i++)
			{
				Occurrence.Issue.Occurrences.AddNew();
			}

			AssertEquals("", Occurrence.FinalKey);

			Occurrence.Issue.LoadFinalKeys = true;
			AssertEquals("TestKeyString", Occurrence.FinalKey);

			Occurrence.HO_XMLData = "<EDI_Exception_Report><Key>SomeOtherKeyString</Key></EDI_Exception_Report>";
			AssertEquals("Should be cached, so won't refresh", "TestKeyString", Occurrence.FinalKey);
		}

		public void TestFinalKeyInfo()
		{
			AssertEquals("FinalKey", Occurrence.FinalKeyInfo.Name);
		}

		HelpErrorLogOccurrence Occurrence
		{
			get { return occurrence ?? (occurrence = Factory.New<HelpErrorLogOccurrence>()); }
		}
	}
}
