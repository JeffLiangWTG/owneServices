using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.Testing
{
	[TestedType(typeof(JobDocAddressFormatter))]
	class JobDocAddressFormatterEDWTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestJobDocAddressFormatterForAll()
		{
			int counter = 1;
			Action<string, string, string, string, string, string> assertJobDocAddress = (address1, address2, city, state, postCode, expectedDocAddress) =>
			{
				counter++;
				var parentPK = Guid.NewGuid();
				CreateJobDocAddress(counter, parentPK, "JJ", "LCI", address1, address2, city, state, postCode, "", 0);

				var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT DocAddress FROM {0}.dbo.JobDocAddressFormatter('{1}', 'JJ', 'LCI', 'ALL')", dbName, parentPK));
				AssertEquals(expectedDocAddress, result.Rows[0]["DocAddress"]);
			};

			assertJobDocAddress("Add1-1", "Add1-2", "City1", "State1", "1111", "Add1-1\nAdd1-2\nCity1, State1 1111\n");

			assertJobDocAddress("Add2-1", "", "", "", "", "Add2-1\n");
			assertJobDocAddress("Add3-1", "Add3-2", "", "", "", "Add3-1\nAdd3-2\n");

			assertJobDocAddress("Add4-1", "", "", "State2", "", "Add4-1\nState2 \n");
			assertJobDocAddress("Add5-1", "", "", "", "2222", "Add5-1\n2222\n");
			assertJobDocAddress("Add6-1", "", "", "State3", "3333", "Add6-1\nState3 3333\n");
			assertJobDocAddress("Add7-1", "", "City2", "State4", "", "Add7-1\nCity2, State4 \n");
			assertJobDocAddress("Add8-1", "", "City3", "", "4444", "Add8-1\nCity3, 4444\n");
			assertJobDocAddress("Add9-1", "", "City4", "State5", "5555", "Add9-1\nCity4, State5 5555\n");
		}

		protected override string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}

		string dbName => Db.EdwDatabaseName;

		void CreateJobDocAddress(int jobDocAddressKey, Guid parentPK, string parentTableCode, string addressType, string address1, string address2, string city, string state, string postCode, string description, int addressSequence)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[CUS__JobDocAddress] (JobDocAddressKey, ParentID, ParentTableCode, AddressType, Address1, Address2, City, State, PostCode, Description, AddressSequence) VALUES ({1}, '{2}', '{3}', '{4}', '{5}', '{6}','{7}', '{8}', '{9}', '{10}', {11})",
				dbName, jobDocAddressKey, parentPK, parentTableCode, addressType, address1, address2, city, state, postCode, description, addressSequence);

			TestConnection.ExecuteNonQuery(sql);
		}

		public void TestJobDocAddressFormatterForA1A2()
		{
			var parentPK = Guid.NewGuid();

			CreateJobDocAddress(1, parentPK, "JJ", "LCI", "Add1-1", "Add1-2", "City1", "State1", "1111", "TestDesc", 0);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT DocAddress FROM {0}.dbo.JobDocAddressFormatter('{1}', 'JJ', 'LCI', 'A1, A2')", dbName, parentPK));
			AssertEquals("Add1-1\nAdd1-2\n", result.Rows[0]["DocAddress"]);
		}
	}
}

