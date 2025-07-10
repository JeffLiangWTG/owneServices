using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.UNDG;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.UNDG
{
	[TestedType(typeof(trgViewUNDGAttribute_Del))]
	class trgViewUNDGAttribute_DelTest : DbCreateScriptTest
	{
		public void DeleteTest()
		{
			var undgPK = TestConnection.ExecuteScalar("SELECT TOP 1 DG_PK FROM dbo.UNDGSubstance");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.ViewUNDGAttribute (DA_PK, DA_Descriptor, DA_Index, DA_IsSystem, DA_Language, DA_Type, DA_DG)
VALUES('EAFAFD29-A1D9-41F4-868A-60C5C64FEE83', 'DESC2', '10', 1, 'EN', 'TP', '{undgPK}')");

			var zzUNDGAttributeCountBefore = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGAttribute");

			TestConnection.Command("DELETE FROM dbo.ViewUNDGAttribute WHERE DA_PK = 'EAFAFD29-A1D9-41F4-868A-60C5C64FEE83'");

			var zzUNDGAttributeCountAfter = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGAttribute");

			AssertEquals(zzUNDGAttributeCountAfter, zzUNDGAttributeCountBefore - 1);
		}
	}
}

