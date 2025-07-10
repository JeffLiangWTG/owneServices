using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BR.ModelViews.BRCusEntryInstruction))]
	class BRCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"BRCusEntryInstruction",
				"CusEntryInstruction",
				new[]
				{
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_AdditionalInformationOption", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_AFRMMMethodOfCalculation", VarChar, 4),
					new TestDbViewHelper.DbColumn("CEI_AFRMMRateOverride", Decimal, -1, 19, 8),
					new TestDbViewHelper.DbColumn("CEI_DetailWithoutLegalDoc", VarChar, 4),
					new TestDbViewHelper.DbColumn("CEI_IsConsortedExport", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_LegalDocument", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_SpecialCustomsClearance", VarChar, 4),
					new TestDbViewHelper.DbColumn("CEI_UtilizationFeeOverride", Decimal, -1, 19, 4)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("BRCusEntryInstruction doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "BRCusEntryInstruction_Idx"));
		}
	}
}
