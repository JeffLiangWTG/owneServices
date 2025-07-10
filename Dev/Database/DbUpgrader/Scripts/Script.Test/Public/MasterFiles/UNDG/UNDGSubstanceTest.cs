using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.UNDG;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.UNDG
{
	[TestedType(typeof(UNDGSubstance))]
	class UNDGSubstanceTest : DbCreateScriptTest
	{
		public void TestColumnTypes()
		{
			var numberOfOriginalColumns = TestConnection.ExecuteScalar(@"SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ZZUNDGSubstance') AND [name] NOT IN ('DG_SystemCreateTimeUtc','DG_SystemCreateUser','DG_SystemLastEditTimeUtc','DG_SystemLastEditUser')");
			AssertEquals(numberOfOriginalColumns, TestConnection.ExecuteScalar(@"
SELECT COUNT(*) FROM sys.columns c1 
JOIN sys.columns c2 ON c1.name = c2.name AND (c1.system_type_id = c2.system_type_id OR (c1.system_type_id IN (167, 175) AND c2.system_type_id IN (167, 175))) -- char/varchar
WHERE c1.object_id = OBJECT_ID('dbo.ZZUNDGSubstance') AND c2.object_id = OBJECT_ID('dbo.UNDGSubstance')
"));
		}

		protected override bool RequiresSchemaBinding => false;
	}
}

