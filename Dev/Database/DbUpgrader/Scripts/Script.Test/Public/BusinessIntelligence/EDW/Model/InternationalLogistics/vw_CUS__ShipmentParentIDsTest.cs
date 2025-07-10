using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__ShipmentParentIDs))]
	internal class vw_CUS__ShipmentParentIDsTest : BiCreateScriptTest
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
			AssertEquals("Rowcount", 7, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"), new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"));
				AssertRowValues(resultTable, 2, new Guid("AA1895C4-612F-42A5-B75B-0024986DE21B"), new Guid("AA1895C4-612F-42A5-B75B-0024986DE21B"));
				AssertRowValues(resultTable, 3, new Guid("415AF163-DD9F-4C05-9E28-002EFC35E760"), new Guid("415AF163-DD9F-4C05-9E28-002EFC35E760"));
				AssertRowValues(resultTable, 1, new Guid("C3F842EF-3BE5-448C-BED3-0017B232C624"), new Guid("6F1755E3-D182-4844-9399-E8585E8D3BE8"));
				AssertRowValues(resultTable, 2, new Guid("AA1895C4-612F-42A5-B75B-0024986DE21B"), new Guid("0DFAE11D-2EBE-4C8C-8DAA-B0EA68C2E126"));
				AssertRowValues(resultTable, 2, new Guid("AA1895C4-612F-42A5-B75B-0024986DE21B"), new Guid("2DA3C0BF-9236-4FB8-8727-1E3D9A6F696F"));
				AssertRowValues(resultTable, 11, new Guid("415AF163-DD9F-4C05-9E28-002EFC35E760"), new Guid("BB6F80B9-3F0B-4A56-AB5B-FE83D3607688"));
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"ShipmentParentKey",
				"JS",
				"ParentID"
			};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, int? shipmentParentKey, Guid? js, Guid? parentID)
		{
			var selectqry = string.Format("ShipmentParentKey {0} AND JS {1} AND ParentID {2}",
				shipmentParentKey == null ? "IS NULL" : "= " + shipmentParentKey,
				js == null ? "IS NULL" : "= '" + js + "'",
				parentID == null ? "IS NULL" : "= '" + parentID + "'"
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
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_CUS__ShipmentParentIDs]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[InternationalLogistics].[BAS__Shipment](ShipmentID, ShipmentKey)
				VALUES	('C3F842EF-3BE5-448C-BED3-0017B232C624', 1),
						('AA1895C4-612F-42A5-B75B-0024986DE21B', 2),
						('415AF163-DD9F-4C05-9E28-002EFC35E760', 3)

				INSERT [{0}].[Customs].[BAS__Declaration](DeclarationID, DeclarationKey, JobShipmentKey)
				VALUES	('6F1755E3-D182-4844-9399-E8585E8D3BE8', 1, 1),
						('0DFAE11D-2EBE-4C8C-8DAA-B0EA68C2E126', 2, 2)

				INSERT [{0}].[Customs].[BAS__EntryHeader](EntryHeaderID, EntryHeaderKey, DeclarationKey)
				VALUES	('2DA3C0BF-9236-4FB8-8727-1E3D9A6F696F', 1, 2)

				INSERT [{0}].[Customs].[BAS__HouseAirWayBill](HouseAirWayBillID, HouseAirWayBillKey, JobShipmentKey, MasterAirWayBillKey)
				VALUES	('377EA46E-F63C-48EB-BEB4-82F5549A4E17', 11, 3, 1)

				INSERT [{0}].[Customs].[BAS__MasterAirWayBill](MasterAirWayBillID, MasterAirWayBillKey)
				VALUES	('BB6F80B9-3F0B-4A56-AB5B-FE83D3607688', 1)",

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
