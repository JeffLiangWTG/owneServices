using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(GetCustomFieldByName))]
	class GetCustomFieldByNameTest : DbCreateScriptTest
	{
		public void TestGetCustomFieldByName()
		{
			var orgPK = Guid.NewGuid();
			string createDataSQL = string.Format(@"
insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) values ('{0}', 'ZZZ', 'Test Organisation')
insert into dbo.GenCustomAddOnValue(XV_PK, XV_Name, XV_Type, XV_Data, XV_IsRuleEnabled, XV_ParentTableCode, XV_ParentID)
values (NEWID(), 'Foo', 'STR', 'blah', 0, 'OH', '{0}')",
				orgPK);
			TestConnection.ExecuteNonQuery(createDataSQL);

			string testAssertSQL = string.Format(@"select * from dbo.GetCustomFieldByName('{0}', 'Foo')", orgPK);
			AssertEquals("blah", TestConnection.ExecuteScalar(testAssertSQL).ToString());

			testAssertSQL = string.Format(@"select * from dbo.GetCustomFieldByName('{0}', 'bar')", orgPK);
			AssertNull(TestConnection.ExecuteScalar(testAssertSQL));
		}
	}
}
