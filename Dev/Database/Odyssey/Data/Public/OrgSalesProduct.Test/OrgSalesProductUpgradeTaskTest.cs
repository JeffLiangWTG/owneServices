using System;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class OrgSalesProductUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDoNotOverwriteUserEditableColumns()
		{
			var updateSql = @"
UPDATE dbo.OrgSalesProduct
SET
	MP_Name = 'Custom Name',
	MP_FormLayoutData = '<SalesProductFormLayoutData xmlns=""http://cargowise.com/SalesProductFormLayoutData.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<TradeLaneCustomColumnDefinitionFks>335a8422-95ae-4096-9bb0-740430995d3f</TradeLaneCustomColumnDefinitionFks>
</SalesProductFormLayoutData>',
	MP_SystemLastEditTimeUtc = '1-1-2015',
	MP_SystemLastEditUser = 'ADL'
WHERE
	MP_PK = '24fab43b-7a5b-4b3e-a2fe-a6b5cfd2503f'";

			TestConnection.ExecuteNonQuery(updateSql);

			var task = new OrgSalesProductUpgradeTask();
			task.Run();

			var selectSql = @"SELECT * FROM dbo.OrgSalesProduct WHERE MP_PK = '24fab43b-7a5b-4b3e-a2fe-a6b5cfd2503f'";

			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());

				CombineAssertions("user editable columns should retain overriden values", () =>
				{
					AssertEquals("MP_Name", "Custom Name", reader[OrgSalesProductSchema.Constants.MP_Name]);
					AssertEquals("MP_FormLayoutDat", @"<SalesProductFormLayoutData xmlns=""http://cargowise.com/SalesProductFormLayoutData.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><TradeLaneCustomColumnDefinitionFks>335a8422-95ae-4096-9bb0-740430995d3f</TradeLaneCustomColumnDefinitionFks></SalesProductFormLayoutData>", reader[OrgSalesProductSchema.Constants.MP_FormLayoutData]);
					AssertEquals("MP_SystemLastEditTimeUtc", new DateTime(2015, 1, 1), reader[OrgSalesProductSchema.Constants.MP_SystemLastEditTimeUtc]);
					AssertEquals("MP_SystemLastEditUser", "ADL", reader[OrgSalesProductSchema.Constants.MP_SystemLastEditUser]);
				});
			}
		}

		public void TestOverwriteUserEditableColumnsIfProductWasCreatedFromTransformation()
		{
			var updateSql = @"
UPDATE dbo.OrgSalesProduct
SET
	MP_Name = 'Custom Name',
	MP_FormLayoutData = '<?transformation placeholder?>',
	MP_SystemLastEditTimeUtc = GETUTCDATE(),
	MP_SystemLastEditUser = 'ADL'
WHERE
	MP_PK = '24fab43b-7a5b-4b3e-a2fe-a6b5cfd2503f'";

			TestConnection.ExecuteNonQuery(updateSql);

			var task = new OrgSalesProductUpgradeTask();
			task.Run();

			var selectSql = @"SELECT * FROM dbo.OrgSalesProduct WHERE MP_PK = '24fab43b-7a5b-4b3e-a2fe-a6b5cfd2503f'";

			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());

				CombineAssertions("user editable columns should retain overriden values", () =>
				{
					AssertEquals("MP_Name", "Forwarding", reader[OrgSalesProductSchema.Constants.MP_Name]);
					AssertEquals("MP_FormLayoutDat", @"<SalesProductFormLayoutData xmlns=""http://cargowise.com/SalesProductFormLayoutData.xsd"" xmlns:xs=""http://www.w3.com/2001/XMLSchema"" />", reader[OrgSalesProductSchema.Constants.MP_FormLayoutData]);
				});
			}
		}
	}
}
