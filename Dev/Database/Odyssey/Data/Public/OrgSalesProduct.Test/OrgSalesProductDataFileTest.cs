using System.Data;
using System.IO;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class OrgSalesProductDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new OrgSalesProductDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestLoadDataFromDatabase()
		{
			var insertSql = @"
INSERT
	dbo.OrgSalesProduct (MP_PK, MP_Code, MP_Name, MP_IsSystemDefined, MP_FormLayoutData, MP_SystemCreateTimeUtc, MP_SystemCreateUser, MP_SystemLastEditTimeUtc, MP_SystemLastEditUser)
VALUES
	('2B939D50-B94F-4FB3-9230-9F79A803DB31', 'SYS1', 'System Defined 1', 1, '', '1-1-2015', 'ZZZ', '1-1-2015', 'ZZZ'),
	('2B939D50-B94F-4FB3-9230-9F79A803DB32', 'SYS2', 'System Defined 2', 1, '', '1-1-2015', 'ZZZ', '1-1-2015', 'ZZZ'),
	('2B939D50-B94F-4FB3-9230-9F79A803DB33', 'NON1', 'Non-System Defined 1', 0, '', '1-1-2015', 'ZZZ', '1-1-2015', 'ZZZ'),
	('2B939D50-B94F-4FB3-9230-9F79A803DB34', 'NON2', 'Non-System Defined 2', 0, '', '1-1-2015', 'ZZZ', '1-1-2015', 'ZZZ')
";

			TestConnection.ExecuteNonQuery(insertSql);

			var file = new OrgSalesProductDataFile();
			var data = file.LoadDataFromDatabase();

			var actualTables = data.Tables.Cast<DataTable>().Select(x => x.TableName);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					OrgSalesProductSchema.Constants.TableName
				},
				actualTables);

			var actualNames = data.Tables[OrgSalesProductSchema.Constants.TableName].Select("MP_SystemCreateUser = 'ZZZ'").Select(x => x[OrgSalesProductSchema.Constants.MP_Name]);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"System Defined 1",
					"System Defined 2",
				},
				actualNames);
		}
	}
}
