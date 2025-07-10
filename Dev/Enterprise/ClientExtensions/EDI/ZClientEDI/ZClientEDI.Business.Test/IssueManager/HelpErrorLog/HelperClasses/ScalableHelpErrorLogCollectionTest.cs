using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	class ScalableHelpErrorLogCollectionTest : TestCaseWithFactory
	{
		public void TestFind()
		{
			ScalableHelpErrorLogCollection logs = new ScalableHelpErrorLogCollection(Factory);

			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogKey key1 = log.Keys.AddNew();
			key1.HK_Key = "at System.Data.SqlClient.SqlInternalConnection.OnError(  at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(  at System.Data.SqlClient.TdsParser.Connect(  at System.Data.SqlClient.SqlInternalConnectionTds.";
			key1.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key1.HK_Key);
			log.HE_IsClientVisible = true;
			HelpErrorLogKey key2 = log.Keys.AddNew();
			key2.HK_Key = "1b1b1b";
			key2.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key2.HK_Key);
			Factory.Save();

			AssertEquals(log, logs.Find(key1.HK_Key));
			AssertEquals(log, logs.Find(key2.HK_Key));

			AssertNull(logs.Find("blah"));
			AssertNull("Keys that are seen as a match when ignoring case will not be found", logs.Find(key1.HK_HashCode.ToString().ToUpper()));
		}

		public void TestAddNew()
		{
			ScalableHelpErrorLogCollection logs = new ScalableHelpErrorLogCollection(Factory);

			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogKey key1 = log1.Keys.AddNew();
			key1.HK_Key = "at System.Data.SqlClient.SqlInternalConnection.OnError(  at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(  at System.Data.SqlClient.TdsParser.Connect(  at System.Data.SqlClient.SqlInternalConnectionTds.";
			key1.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key1.HK_Key);
			log1.HE_IsClientVisible = true;
			Factory.Save();

			EdiHelpErrorLog log2 = logs.AddNew("blah");

			AssertEquals(log2, logs.Find("blah"));
			AssertEquals(log1, logs.Find(key1.HK_Key));

			AssertNull(logs.Find("blah".ToUpper()));
		}

		public void TestSave()
		{
			ScalableHelpErrorLogCollection logs = new ScalableHelpErrorLogCollection(Factory);
			EdiHelpErrorLog log = logs.AddNew("blah");

			Assert(!log.IsInDatabase);
			logs.Save();
			Assert(log.IsInDatabase);
		}
	}
}