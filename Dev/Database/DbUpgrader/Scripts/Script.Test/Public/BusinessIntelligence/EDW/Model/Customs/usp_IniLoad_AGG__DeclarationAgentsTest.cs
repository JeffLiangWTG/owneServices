using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Customs.Testing
{
	[TestedType(typeof(usp_IniLoad_AGG__DeclarationAgents))]
	internal class usp_IniLoad_AGG__DeclarationAgentsTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, "CAREDILHR", new Guid("4774D8BD-0A05-408D-8E47-F050B80FF23E"), new Guid("37A1CA19-71B0-40E3-80CE-75353511DCF2"), 1736, "CAREDILHR", new Guid("A37E26B1-EF01-4B81-99B4-E5D1F949D753"), "CAREDILHR", new Guid("A37E26B1-EF01-4B81-99B4-E5D1F949D753"), new Guid("A37E26B1-EF01-4B81-99B4-E5D1F949D753"), 34247, new Guid("A37E26B1-EF01-4B81-99B4-E5D1F949D753"), 34247);
			});
		}

		void AssertRowValues(DataTable resultTable, string carriercode, Guid? carrierid, Guid? consolidationid, int? consolidationkey, string receivingagentcode, Guid? receivingagentid, string sendingagentcode, Guid? sendingagentid, Guid? sendingorganizationid, int? sendingorganizationkey, Guid? shippingorganizationid, int? shippingorganizationkey)
		{
			var selectqry = string.Format("CarrierCode {0} AND CarrierID {1} AND ConsolidationID {2} AND ConsolidationKey {3} AND ReceivingAgentCode {4} AND ReceivingAgentID {5} AND SendingAgentCode {6} AND SendingAgentID {7} AND SendingOrganizationID {8} AND SendingOrganizationKey {9} AND ShippingOrganizationID {10} AND ShippingOrganizationKey {11}",
				carriercode == null ? "IS NULL" : "= '" + carriercode + "'",
				carrierid == null ? "IS NULL" : "= '" + carrierid + "'",
				consolidationid == null ? "IS NULL" : "= '" + consolidationid + "'",
				consolidationkey == null ? "IS NULL" : "= '" + consolidationkey + "'",
				receivingagentcode == null ? "IS NULL" : "= '" + receivingagentcode + "'",
				receivingagentid == null ? "IS NULL" : "= '" + receivingagentid + "'",
				sendingagentcode == null ? "IS NULL" : "= '" + sendingagentcode + "'",
				sendingagentid == null ? "IS NULL" : "= '" + sendingagentid + "'",
				sendingorganizationid == null ? "IS NULL" : "= '" + sendingorganizationid + "'",
				sendingorganizationkey == null ? "IS NULL" : "= '" + sendingorganizationkey + "'",
				shippingorganizationid == null ? "IS NULL" : "= '" + shippingorganizationid + "'",
				shippingorganizationkey == null ? "IS NULL" : "= '" + shippingorganizationkey + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Customs].[BAS__Declaration]
					([ShippingLineID], [DeclarationID], [DeclarationKey], [ForwarderID])
				VALUES
					('4774D8BD-0A05-408D-8E47-F050B80FF23E', '37A1CA19-71B0-40E3-80CE-75353511DCF2', 1736, 'A37E26B1-EF01-4B81-99B4-E5D1F949D753');

				INSERT [{0}].[Organization].[BAS__Organization]
					([Code], [FullName], [OrganizationKey], [OrganizationID])
				VALUES
					('CAREDILHR', 'CAREDILHR', 34247, 'A37E26B1-EF01-4B81-99B4-E5D1F949D753');
				",
					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Customs].[AGG__DeclarationAgents]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
