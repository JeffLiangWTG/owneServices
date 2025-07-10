using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ES.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ES.ModelViews.ESJobDeclaration))]
	class ESJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ESJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_AuthPerDeclaration", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_DestinationState", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_DontSendImporterId", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_IsRMTApplicable", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OtherEmailAddr", VarChar, 254),
					new TestDbViewHelper.DbColumn("JE_PartialWriteoff", Bit, -1),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ESJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ESJobDeclaration_Idx"));
		}
	}
}
