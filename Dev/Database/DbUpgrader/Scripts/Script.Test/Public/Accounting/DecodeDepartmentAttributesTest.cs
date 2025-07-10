using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(DecodeDepartmentAttributes))]
	class DecodeDepartmentAttributesTest : DbCreateScriptTest
	{
		public void TestDecodeDepartmentAttributes()
		{
			AssertDepartmentAttributes("BRN", "Miscellaneous", "Other", "Other");
			AssertDepartmentAttributes("CIR", "Customs", "Import", "Road");
			AssertDepartmentAttributes("CPP", "Customs", "Other", "Post");
			AssertDepartmentAttributes("DIA", "Depot CFS", "Import", "Air");
			AssertDepartmentAttributes("FIS", "Forwarding", "Import", "Sea");
			AssertDepartmentAttributes("GES", "Gateway", "Export", "Sea");
			AssertDepartmentAttributes("GIL", "Gateway", "Import", "Rail");
			AssertDepartmentAttributes("LIA", "Linehaul", "Import", "Air");
			AssertDepartmentAttributes("SDB", "Ships Agency", "Domestic", "B-Bulk");
			AssertDepartmentAttributes("SDC", "Ships Agency", "Domestic", "Containerized");
			AssertDepartmentAttributes("SED", "Ships Agency", "Export", "Detention");
			AssertDepartmentAttributes("SEL", "Ships Agency", "Export", "Liquid Bulk");
			AssertDepartmentAttributes("SIU", "Ships Agency", "Import", "Bulk");
			AssertDepartmentAttributes("SIV", "Ships Agency", "Import", "RORO/Vehicle");
			AssertDepartmentAttributes("SOA", "Ships Agency", "Other", "Voyage Accounting");
			AssertDepartmentAttributes("SOR", "Ships Agency", "Other", "Other");
			AssertDepartmentAttributes("TEA", "Port Transport", "Export", "Air");
			AssertDepartmentAttributes("WFS", "Warehouse", "Other", "Sea");
			AssertDepartmentAttributes("YDJ", "Container Yard", "Domestic", "Other");

			void AssertDepartmentAttributes(string code, string activity, string direction, string mode)
			{
				var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM DecodeDepartmentAttributes('{code}')");
				AssertEquals("Result should have one row", 1, result.Rows.Count);
				var row = result.Rows[0];
				AssertEquals(activity, row["Activity"]);
				AssertEquals(direction, row["Direction"]);
				AssertEquals(mode, row["Mode"]);
			}
		}
	}
}
