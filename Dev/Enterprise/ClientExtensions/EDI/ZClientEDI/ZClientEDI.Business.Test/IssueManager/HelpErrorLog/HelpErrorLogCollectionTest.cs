using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	[TestedType(typeof(HelpErrorLogCollection))]
	class HelpErrorLogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestToString()
		{
			HelpErrorLogCollection logs = new HelpErrorLogCollection(Factory);
			AssertEquals("0 Logs", logs.ToString());

			logs.AddNew();
			AssertEquals("1 Log", logs.ToString());

			logs.AddNew();
			AssertEquals("2 Logs", logs.ToString());
		}

		public void TestFind()
		{
			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogKey key1 = log.Keys.AddNew();
			key1.HK_Key = "at System.Data.SqlClient.SqlInternalConnection.OnError(  at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(  at System.Data.SqlClient.TdsParser.Connect(  at System.Data.SqlClient.SqlInternalConnectionTds.";
			key1.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key1.HK_Key);
			log.HE_IsClientVisible = true;
			HelpErrorLogKey key2 = log.Keys.AddNew();
			key2.HK_Key = "1b1b1b";
			key2.HK_HashCode = HelpErrorLogKey.GetKeyHashCode(key2.HK_Key);
			Factory.Save();

			HelpErrorLogCollection logs = new HelpErrorLogCollection(Factory);
			logs.Load();

			AssertEquals(log, logs.Find(key1.HK_Key));
			AssertEquals(log, logs.Find(key2.HK_Key));

			AssertNull(logs.Find("blah"));
			AssertNull("Keys that are seen as a match when ignoring case will not be found", logs.Find(key1.HK_HashCode.ToString().ToUpper()));
		}

		public void TestSave()
		{
			HelpErrorLogCollection logs = new HelpErrorLogCollection(Factory);
			EdiHelpErrorLog log = logs.AddNew("blah");

			Assert(!log.IsInDatabase);
			logs.Save();
			Assert(log.IsInDatabase);
		}

		public void TestAddNew2()
		{
			HelpErrorLogCollection logs = new HelpErrorLogCollection(Factory);
			var log1 = logs.AddNew("blah");
			AssertEquals(1, logs.Count);
			AssertEquals(log1, logs[0]);
			var log2 = logs.AddNew("foo");
			AssertEquals(2, logs.Count);
			AssertEquals(log1, logs[0]);
			AssertEquals(log2, logs[1]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HelpErrorLogCollection(Factory);
		}
	}
}
