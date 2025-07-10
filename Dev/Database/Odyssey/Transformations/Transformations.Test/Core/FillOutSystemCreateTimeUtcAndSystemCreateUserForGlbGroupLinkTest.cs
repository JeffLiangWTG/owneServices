using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing;

[TestedType(typeof(FillOutSystemCreateTimeUtcAndSystemCreateUserForGlbGroupLink))]
public class FillOutSystemCreateTimeUtcAndSystemCreateUserForGlbGroupLinkTest : DataTransformationTestCase
{
	readonly Guid InvalidGroupLinkPK = Guid.NewGuid();
	protected override void PrepareTestData()
	{
		var helper = new TransformationTestDataCreator();
		var groupId =	helper.CreateGlbGroup("EDITST", "STF", "EDI TEST");
		var staffPK = helper.CreateStaff("TEST", "TST");

		helper.CreateStmALog("GlbGroup", groupId, "AAA", "EDT", "Attached - (TST)", new DateTime(2014, 11, 20, 11, 30, 0), new DateTime(2014, 11, 20, 11, 30, 0));
		helper.CreateStmALog("GlbStaff", staffPK, "E", "EDT", "Attached - (TST)EDI TEST", new DateTime(2014, 11, 21, 11, 30, 0), new DateTime(2014, 11, 21, 11, 30, 0));

		var createGlbGroupLinkSql = @"INSERT INTO dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS, GK_SystemCreateTimeUtc, GK_SystemCreateUser, GK_SystemLastEditTimeUtc, GK_SystemLastEditUser)
VALUES (@pk, @groupPk, @staffPk, @CreateTimeUtc, @CreateUser, GetUtcDate(), '~BP')";

		using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("GlbGroupLink"))
		using (var command = Db.Connection.Command(createGlbGroupLinkSql))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, InvalidGroupLinkPK);
			command.AddParameter("@groupPk", SqlDbType.UniqueIdentifier, groupId);
			command.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
			command.AddParameter("@CreateTimeUtc", SqlDbType.DateTime, DBNull.Value);
			command.AddParameter("@CreateUser", SqlDbType.VarChar, "");
			command.ExecuteNonQuery();
		}
	}

	protected override void AssertTransformationResults()
	{
		string user = null;
		var createTime = DateTime.MinValue;
		var sql = @"SELECT GK_SystemCreateUser,GK_SystemCreateTimeUtc FROM dbo.GlbGroupLink WHERE GK_PK = @PK";
		using var cmd = TestConnection.Command(sql);
		{
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, InvalidGroupLinkPK);

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				user = reader.GetString(0);
				createTime = reader.GetDateTime(1);
			}
		}
		AssertEquals("AAA", user);
		AssertEquals(new DateTime(2014, 11, 20, 11, 30, 0), createTime);
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new FillOutSystemCreateTimeUtcAndSystemCreateUserForGlbGroupLink();
	}
}
