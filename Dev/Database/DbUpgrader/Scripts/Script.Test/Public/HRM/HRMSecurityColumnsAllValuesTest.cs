using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRMSecurityColumnsAllValues))]
	class HRMSecurityColumnsAllValuesTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var securityColumns = HRSecurityColumns.GetColumnNames();

			DataTable result;
			using (var command = TestConnection.Command("SELECT * FROM [hrm].[HRMSecurityColumnsAllValues]"))
			{
				result = DataUtils.GetDataTableFromCommand(command);
			}
			var valueArrays = result.Rows.Cast<DataRow>().Select(r => r.ItemArray).ToArray();

			AssertArrayEqualsByElements(securityColumns.ToArray(), result.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());

			var combinationsLength = (int)Math.Pow(2, securityColumns.Length);
			AssertEquals(combinationsLength, result.Rows.Count);
			Assert(expectedResult.All(er => valueArrays.Any(va => va.SequenceEqual(er))));
		}

		public void TestSecurityColumnTypesNotNull()
		{
			var securityColumns = HRSecurityColumns.GetColumnNames();

			DataTable result;
			using (var command = TestConnection.Command(@"
SELECT 
    c.name AS column_name,
    type_name(user_type_id) AS data_type,
    c.is_nullable
FROM 
    sys.columns c
    JOIN sys.views v ON v.object_id = c.object_id
WHERE 
    schema_name(v.schema_id) = 'hrm'
    AND object_name(c.object_id) = 'HRMSecurityColumnsAllValues'
"))
			{
				result = DataUtils.GetDataTableFromCommand(command);
			}
			var valueArrays = result.Rows.Cast<DataRow>().Select(r => r.ItemArray).ToArray();

			AssertContainsExactElementsInExactOrder(securityColumns, valueArrays.Select(item => item[0]).ToArray());
			Assert("All column types are BIT", valueArrays.All(item => ((string)item[1]) == "bit"));
			Assert("All columns are NOT NULL", valueArrays.All(item => !((bool)item[2])));
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;

		readonly object[][] expectedResult =
		[
			[false, false, false, false],
			[false, true, false, false],
			[false, false, true, false],
			[false, true, true, false],
			[false, false, false, true],
			[false, true, false, true],
			[false, false, true, true],
			[false, true, true, true],
			[true, false, false, false],
			[true, true, false, false],
			[true, false, true, false],
			[true, true, true, false],
			[true, false, false, true],
			[true, true, false, true],
			[true, false, true, true],
			[true, true, true, true]
		];
	}
}
