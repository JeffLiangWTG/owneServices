using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPJobDeclaration))]
	sealed class JPJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"JPJobDeclaration",
				"JobDeclaration",
				[
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_CustomsOfficeDepartment", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_NACCSCredential", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_OwnerSectionCode", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_FinalDestinationName", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_PortOfLoadingName", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_PaymentDeadlineExtension", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_ACP_POA", VarChar, 15),
					new TestDbViewHelper.DbColumn("JE_DeliveryMode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_RadioCallSign", VarChar, 10),
					new TestDbViewHelper.DbColumn("JE_ReceiptMode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_ArrivalAtLoadingDate", Date, -1)
				]
			);
		}
	}
}
