using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__ShipmentMainConsol))]
	internal class vw_GRP__ShipmentMainConsolTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = GetColumns();
			TestColumnsAreAsExpected(ScriptToTest.Name, columns);

			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"), new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"), 2, 1, "AUSYD", "HKHKG", "NZAKL, HKHKG, NZAKL, HKHKG", "AU", "HK");
				AssertRowValues(resultTable, new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"), new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"), 1, 1, "AUSYD", "HKHKG", "NZAKL, HKHKG, NZAKL, HKHKG", "AU", "HK");
				AssertRowValues(resultTable, new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"), new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"), 2, 1, "AUSYD", "NZAKL", "NZAKL, HKHKG, NZAKL, HKHKG", "AU", "NZ");
				AssertRowValues(resultTable, new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"), new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"), 1, 1, "AUSYD", "NZAKL", "NZAKL, HKHKG, NZAKL, HKHKG", "AU", "NZ");
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"ConsolidationID",
				"ConsolidationKey",
				"ConsolidationShipmentPivotKey",
				"DischargePort",
				"LoadPort",
				"ShipmentID",
				"Discharges",
				"LoadPortCountryCode",
				"DischargePortCountryCode"
			};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, Guid? shipmentID, Guid? consolidationID, int? consolidationShipmentPivotKey, int? consolidationKey, string loadPort, string dischargePort, string discharges, string loadportcountrycode, string dischargeportcountrycode)
		{
			var selectqry = string.Format("ShipmentID {0} AND ConsolidationID {1} AND ConsolidationShipmentPivotKey {2} AND ConsolidationKey {3} AND LoadPort {4} AND DischargePort {5} AND Discharges {6} AND LoadPortCountryCode {7} AND DischargePortCountryCode {8}",
				shipmentID == null ? "IS NULL" : "= '" + shipmentID + "'",
				consolidationID == null ? "IS NULL" : "= '" + consolidationID + "'",
				consolidationShipmentPivotKey == null ? "IS NULL" : "= " + consolidationShipmentPivotKey,
				consolidationKey == null ? "IS NULL" : "= " + consolidationKey,
				loadPort == null ? "IS NULL" : "= '" + loadPort + "'",
				dischargePort == null ? "IS NULL" : "= '" + dischargePort + "'",
				discharges == null ? "IS NULL" : "= '" + discharges + "'",
				loadportcountrycode == null ? "IS NULL" : "= '" + loadportcountrycode + "'",
				dischargeportcountrycode == null ? "IS NULL" : "= '" + dischargeportcountrycode + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_GRP__ShipmentMainConsol]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot](ShipmentID, ConsolidationShipmentPivotID, ConsolidationShipmentPivotKey, ConsolidationID, ConsolidationKey)
															VALUES('C3F842EF-3BE5-448C-BED3-0017B232C624', 'AA1895C4-612F-42A5-B75B-0024986DE21B', 1, '4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 1),
															('C3F842EF-3BE5-448C-BED3-0017B232C624', 'A98862FC-97D9-4561-B88A-57C4A94FBE98', 2, '4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 1)

				INSERT [{0}].[InternationalLogistics].[BAS__Consolidation](ConsolidationID, ConsolidationKey, DischargePort, LoadPort)
															VALUES('4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 1, 'NZAKL', 'AUSYD'),
															('4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 1, 'HKHKG', 'AUSYD')",
			ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
