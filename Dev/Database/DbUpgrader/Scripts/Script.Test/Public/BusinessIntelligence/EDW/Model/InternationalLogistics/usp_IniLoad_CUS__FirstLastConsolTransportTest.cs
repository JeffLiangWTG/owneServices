using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__FirstLastConsolTransport))]
	internal class usp_IniLoad_CUS__FirstLastConsolTransportTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, "CON", new Guid("34A089D1-FFC2-4BD4-8DD0-0012438186AA"), 1, "SGSIN", new DateTime(2023, 01, 09, 0, 0, 0), new DateTime(2023, 01, 09, 15, 59, 0), new DateTime(2023, 01, 09, 15, 59, 0), "117N", 2, "VNSGN", new DateTime(2023, 01, 11, 0, 0, 0), new DateTime(2023, 01, 11, 12, 59, 0), new DateTime(2023, 01, 11, 12, 59, 0), "117N", 2);
			});
		}

		void AssertRowValues(DataTable resultTable, string parentType, Guid? jK, int? consolidationKey, string firstLoad_Port, DateTime? firstLoad_ETD, DateTime? firstLoad_ATD, DateTime? firstLoad_TD, string firstTransportVoyageFlight, int? firstSailingPK, string lastDischarge_Port, DateTime? lastDischarge_ETA, DateTime? lastDischarge_ATA, DateTime? lastDischarge_TA, string lastTransportVoyageFlight, int? lastSailingPK)
		{
			var selectqry = string.Format("ParentType {0} AND JK {1} AND ConsolidationKey {2} AND FirstLoad_Port {3} AND FirstLoad_ETD {4} AND FirstLoad_ATD {5} AND FirstLoad_TD {6} AND FirstTransportVoyageFlight {7} AND FirstSailingPK {8} AND LastDischarge_Port {9} AND LastDischarge_ETA {10} AND LastDischarge_ATA {11} AND LastDischarge_TA {12} AND LastTransportVoyageFlight {13} AND LastSailingPK {14}",
				parentType == null ? "IS NULL" : "= '" + parentType + "'",
				jK == null ? "IS NULL" : "= '" + jK + "'",
				consolidationKey == null ? "IS NULL" : "=" + consolidationKey,
				firstLoad_Port == null ? "IS NULL" : "= '" + firstLoad_Port + "'",
				firstLoad_ETD == null ? "IS NULL" : "= '" + firstLoad_ETD + "'",
				firstLoad_ATD == null ? "IS NULL" : "= '" + firstLoad_ATD + "'",
				firstLoad_TD == null ? "IS NULL" : "= '" + firstLoad_TD + "'",
				firstTransportVoyageFlight == null ? "IS NULL" : "= '" + firstTransportVoyageFlight + "'",
				firstSailingPK == null ? "IS NULL" : "=" + firstSailingPK,
				lastDischarge_Port == null ? "IS NULL" : "= '" + lastDischarge_Port + "'",
				lastDischarge_ETA == null ? "IS NULL" : "= '" + lastDischarge_ETA + "'",
				lastDischarge_ATA == null ? "IS NULL" : "= '" + lastDischarge_ATA + "'",
				lastDischarge_TA == null ? "IS NULL" : "= '" + lastDischarge_TA + "'",
				lastTransportVoyageFlight == null ? "IS NULL" : "= '" + lastTransportVoyageFlight + "'",
				lastSailingPK == null ? "IS NULL" : "=" + lastSailingPK

				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport](ConsolAndShipmentTransportKey,ParentType,ParentGUID,ConsolidationKey,ConsolAndShipmentTransportID,LoadPort,ETD,ATD,DischargePort,ETA,ATA,VoyageFlight,JobSailingKey)
															VALUES(1,'CON', '34A089D1-FFC2-4BD4-8DD0-0012438186AA', 1, '0B3C16F3-70F4-49C7-BB4A-D4005DADC9B2', 'SGSIN', '2023-01-09 00:00:00', '2023-01-09 15:59:00', 'VNSGN', '2023-01-11 00:00:00', '2023-01-11 12:59:00', '117N', 2),
																  (2,'CON', '34A089D1-FFC2-4BD4-8DD0-0012438186AA', 1, 'EDD62C80-77FD-4662-92E3-B5106156A11C', 'PLGDN', '2022-11-08 00:00:00', '2022-11-07 11:58:00', 'SGSIN', '2022-12-23 01:01:00', '2022-12-23 05:55:00', '013E', 3),
																  (3,'SHP', 'B07F554D-C255-41A5-8F32-0019F7090A04', 3, '2F961EDC-6333-4AC1-A5FA-6E940CF0A311', 'AUSYD', '2023-05-04 12:54:00', '2023-05-04 12:41:00', 'NZAKL', '2023-05-05 12:41:00', NULL, 'NZ1', 4)",
			ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[CUS__FirstLastConsolTransport]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetIniLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__FirstLastConsolTransport'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void Execute()
		{
			var iniLoadSQLText = GetIniLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
