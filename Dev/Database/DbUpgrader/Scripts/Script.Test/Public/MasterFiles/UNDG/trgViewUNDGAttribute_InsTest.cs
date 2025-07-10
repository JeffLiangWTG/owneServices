using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.UNDG;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.UNDG
{
	[TestedType(typeof(trgViewUNDGAttribute_Ins))]
	class trgViewUNDGAttribute_InsTest : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var zzUNDGAttributeCountBefore = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGAttribute");

			var undgPK = TestConnection.ExecuteScalar("SELECT TOP 1 DG_PK FROM dbo.UNDGSubstance");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.ViewUNDGAttribute (DA_PK, DA_Descriptor, DA_Index, DA_IsSystem, DA_Language, DA_Type, DA_DG)
VALUES(newid(), 'DESC2', '10', 1, 'EN', 'TP', '{undgPK}')");

			var zzUNDGAttributeCountAfter = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGAttribute");
			AssertEquals(zzUNDGAttributeCountAfter, zzUNDGAttributeCountBefore + 1);
		}
	}
}

