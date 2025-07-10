using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	[TestedType(typeof(UpdateCOEEventIsCustomizableProperty))]
	class UpdateCOEEventIsCustomizablePropertyTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCOEEventIsCustomizableProperty();

		protected override void PrepareTestData()
		{
			var sql = @"
			DELETE FROM dbo.StmEvent
			INSERT INTO dbo.StmEvent(SE_PK, SE_Code, SE_IsCustomizable, SE_SystemCreateTimeUtc, SE_SystemCreateUser, SE_SystemLastEditTimeUtc, SE_SystemLastEditUser) VALUES (NewId(), 'COE', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			INSERT INTO dbo.StmEvent(SE_PK, SE_Code, SE_IsCustomizable, SE_SystemCreateTimeUtc, SE_SystemCreateUser, SE_SystemLastEditTimeUtc, SE_SystemLastEditUser) VALUES (NewId(), 'AAA', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			INSERT INTO dbo.StmEvent(SE_PK, SE_Code, SE_IsCustomizable, SE_SystemCreateTimeUtc, SE_SystemCreateUser, SE_SystemLastEditTimeUtc, SE_SystemLastEditUser) VALUES (NewId(), 'BBB', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			var cmd = Db.Connection.Command(sql);
			cmd.ExecuteNonQuery();
		}

		protected override void AssertTransformationResults()
		{
			var resultList = new List<(string, bool)>();
			TestConnection.ExecuteReader("SELECT SE_Code, SE_IsCustomizable FROM dbo.StmEvent",
				reader => resultList.Add(((string)reader["SE_Code"], (bool)reader["SE_IsCustomizable"])));
			AssertContainsExactElementsInAnyOrder("Customizable COE event should turn to not customizable.", new List<(string, bool)>()
			{
				("COE", false),
				("AAA", true),
				("BBB", false),
			}, resultList);
		}
	}
}
