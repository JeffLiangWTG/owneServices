using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.HRMS;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.HRMS
{
	[TestedType(typeof(RenameGlbStaffLanguageToGlbPersonLanguage))]
	public class RenameGlbStaffLanguageToGlbPersonLanguageTest : DataTransformationTestCase
	{
		Guid staff1PK = Guid.NewGuid();
		Guid staff2PK = Guid.NewGuid();

		Guid person1PK = Guid.NewGuid();
		Guid person2PK = Guid.NewGuid();

		bool TransformNeeded { get; set; } = true;

		protected override DataTransformation GetNewTestTransformationInstance() => new RenameGlbStaffLanguageToGlbPersonLanguage();

		protected override void AssertTransformationResults()
		{
			if (!TransformNeeded)
			{
				return;
			}

			Assert(!DbObjectCreator.TableExists(Db.Connection, "GlbStaffLanguage"));
			Assert(DbObjectCreator.TableExists(Db.Connection, "GlbPersonLanguage"));

			CombineAssertions(() =>
			{
				Assert(DbObjectCreator.ColumnExists(Db.Connection, "GlbPersonLanguage", "G7_PK"));
				Assert(DbObjectCreator.ColumnExists(Db.Connection, "GlbPersonLanguage", "G7_IsValid"));
				Assert(DbObjectCreator.ColumnExists(Db.Connection, "GlbPersonLanguage", "G7_Language"));
				Assert(DbObjectCreator.ColumnExists(Db.Connection, "GlbPersonLanguage", "G7_SkillLevel"));
				Assert(DbObjectCreator.ColumnExists(Db.Connection, "GlbPersonLanguage", "G7_PER_Person"));
				Assert(DbObjectCreator.IndexExists(Db.Connection, "GlbPersonLanguage", "FK_RC__G7_PER_Person"));
				Assert(DbObjectCreator.TriggerExists(Db.Connection, "GlbPersonLanguage", "TG_GlbPersonLanguage_AuditDetailsAreNotMissing_Insert"));
				Assert(DbObjectCreator.TriggerExists(Db.Connection, "GlbPersonLanguage", "TG_GlbPersonLanguage_AuditDetailsAreNotMissing_Update"));
				Assert(DbObjectCreator.TriggerExists(Db.Connection, "GlbPersonLanguage", "TG_GlbPersonLanguage_SystemLastEditAuditInfoMustBeUpdated_Update"));
				Assert(DbObjectCreator.TriggerExists(Db.Connection, "GlbPersonLanguage", "TG_GlbPersonLanguage_UpdateAutoVersion"));
			});

			var sql = $@"
SELECT
	G7_Language, G7_ParentID, G7_ParentTableCode
FROM
	dbo.GlbPersonLanguage
";

			var resultList = new List<(string language, Guid personPK)> { };
			Db.Connection.ExecuteReader(sql, reader => resultList.Add((reader[0].ToString(), new Guid(reader[1].ToString()))));

			CombineAssertions(() =>
			{
				resultList.Contains(("EN-AU", person1PK));
				resultList.Contains(("ES-ES", person1PK));
				resultList.Contains(("EN-US", person2PK));
			});
		}

		protected override void PrepareTestData()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "GlbStaffLanguage") && !DbObjectCreator.TableExists(Db.Connection, "GlbPersonLanguage"))
			{
				var sql = $@"
INSERT INTO
	dbo.GlbPerson(PER_PK, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser)
VALUES
	(@person1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(@person2, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

INSERT INTO
	dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staff1, 'PPP', 'p', @person1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(@staff2, 'QQQ', 'q', @person2, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

INSERT INTO
	dbo.GlbStaffLanguage(G7_PK, G7_GS, G7_Language, G7_SystemCreateTimeUtc, G7_SystemCreateUser, G7_SystemLastEditTimeUtc, G7_SystemLastEditUser)
VALUES
	(NEWID(), @staff1, 'EN-AU', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(NEWID(), @staff1, 'ES-ES', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(NEWID(), @staff2, 'EN-US', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
";

				using var command = Db.Connection.Command(sql);
				_ = command.AddParameter("@person1", SqlDbType.UniqueIdentifier, person1PK);
				_ = command.AddParameter("@person2", SqlDbType.UniqueIdentifier, person2PK);
				_ = command.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1PK);
				_ = command.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2PK);
				_ = command.ExecuteNonQuery();
			}
			else
			{
				TransformNeeded = false;
			}
		}
	}
}
