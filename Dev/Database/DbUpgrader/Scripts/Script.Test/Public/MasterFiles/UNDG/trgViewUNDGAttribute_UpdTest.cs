using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.UNDG;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.UNDG
{
	[TestedType(typeof(trgViewUNDGAttribute_Upd))]
	class trgViewUNDGAttribute_UpdTest : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			var undgPK = TestConnection.ExecuteScalar("SELECT TOP 1 DG_PK FROM dbo.UNDGSubstance");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.ViewUNDGAttribute (DA_PK, DA_Descriptor, DA_Index, DA_IsSystem, DA_Language, DA_Type, DA_DG)
VALUES('ED90256F-8503-46FA-9B96-5B98B839AAF8', 'DESC2', '10', 1, 'EN', 'TP', '{undgPK}')");

			TestConnection.ExecuteNonQuery(@"UPDATE dbo.ViewUNDGAttribute SET DA_Descriptor = 'DESC2', DA_Index='20', DA_Language='DE', DA_Type='T2' WHERE DA_PK = 'ED90256F-8503-46FA-9B96-5B98B839AAF8'");

			using (var cmd = TestConnection.Command("SELECT DA_Descriptor, DA_Index, DA_Language, DA_Type FROM dbo.ViewUNDGAttribute WHERE DA_PK = 'ED90256F-8503-46FA-9B96-5B98B839AAF8'"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var descriptor = reader.GetString(0);
					AssertEquals("DESC2", descriptor);
					var index = reader.GetString(1);
					AssertEquals("20", index);
					var language = reader.GetString(2);
					AssertEquals("DE", language);
					var type = reader.GetString(3);
					AssertEquals("T2", type);
				}
			}
		}
	}
}

