using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core;

public class FillOutSystemCreateTimeUtcAndSystemCreateUserForGlbGroupLink : DataTransformation
{
	public override string UserDescription => "Fill out SystemCreateTimeUtc And SystemCreateUser if null in GlbGroupLink";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		var invalidGroupLinks = FetchAllInvalidGroupLinks();
		while (invalidGroupLinks.Count > 0)
		{
			var groupLink = invalidGroupLinks.Dequeue();
			var userAndCreateTime = GetNewSystemCreateTimeUtcAndSystemCreateUser(groupLink);
			UpdateNewUserAndCreateTimeForGroupLink(groupLink, userAndCreateTime);
		}
	}

	void UpdateNewUserAndCreateTimeForGroupLink(InvalidGroupLink groupLink, (string user, DateTime createTime) userAndCreateTime)
	{
		if (!string.IsNullOrEmpty(userAndCreateTime.user) && userAndCreateTime.createTime != DateTime.MinValue)
		{
			var updateSql = @"UPDATE dbo.GlbGroupLink 
SET
    GK_SystemCreateUser = @User,
    GK_SystemCreateTimeUtc = @CreatedTimeUtc
WHERE
    GK_GS = @GS_PK AND GK_GG = @GG_PK;";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_GlbGroupLink_SystemLastEditAuditInfoMustBeUpdated_Update", "GlbGroupLink"))
			using (var cmd = Db.Connection.Command(updateSql))
			{
				cmd.AddParameter("@GS_PK", SqlDbType.UniqueIdentifier, groupLink.GS_PK);
				cmd.AddParameter("@GG_PK", SqlDbType.UniqueIdentifier, groupLink.GG_PK);
				cmd.AddParameter("@User", SqlDbType.VarChar, userAndCreateTime.user);
				cmd.AddParameter("@CreatedTimeUtc", SqlDbType.DateTime, userAndCreateTime.createTime);
				cmd.ExecuteNonQuery();
			}
		}
	}

	(string user,DateTime createTime) GetNewSystemCreateTimeUtcAndSystemCreateUser(InvalidGroupLink groupLink)
	{
		string user = null;
		var createTime = DateTime.MinValue;
		var getNewCreateTimeAndUserSql = @"SELECT TOP 1
		SL_GS_NKUser,
		SL_PostedTimeUtc
		FROM
		dbo.StmALog
		WHERE
		SL_Parent = @GG_PK AND SL_Reference like @GS_Reference
		ORDER BY SL_PostedTimeUtc ASC";
		using var cmd = Db.Connection.Command(getNewCreateTimeAndUserSql);
		{
			cmd.AddParameter("@GS_PK", SqlDbType.UniqueIdentifier, groupLink.GS_PK);
			cmd.AddParameter("@GG_PK", SqlDbType.UniqueIdentifier, groupLink.GG_PK);
			cmd.AddParameter("@GS_Reference", SqlDbType.VarChar, groupLink.GS_Reference);

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				user = reader.GetString(0);
				createTime = reader.GetDateTime(1);
			}
		}
		return (user, createTime);
	}

	Queue<InvalidGroupLink> FetchAllInvalidGroupLinks()
	{
		var invalidGroupLinks = new Queue<InvalidGroupLink>();
		using var cmd = Db.Connection.Command(FetchAllInvalidRecordSql);
		using (var reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				invalidGroupLinks.Enqueue(new InvalidGroupLink((Guid)reader["GS_PK"], (Guid)reader["GG_PK"], (string)reader["GS_Reference"]));
			}
		}

		return invalidGroupLinks;
	}

	class InvalidGroupLink
	{
		public InvalidGroupLink(Guid gsPk, Guid ggPk, string gsReference)
		{
			GS_PK = gsPk;
			GG_PK = ggPk;
			GS_Reference = gsReference;
		}

		public Guid GS_PK;
		public Guid GG_PK;
		public string GS_Reference;
	}

	const string FetchAllInvalidRecordSql = @"SELECT
        GK_GS AS GS_PK,
        GK_GG AS GG_PK,
        CONCAT('Attached - (', staff.GS_Code, ')%') AS GS_Reference
    FROM
        dbo.GlbGroupLink link
    INNER JOIN
     	dbo.GlbStaff staff ON staff.GS_PK = link.GK_GS
		AND staff.GS_IsSystemAccount = 0
    INNER JOIN
     	dbo.GlbGroup glbgroup ON glbgroup.GG_PK = link.GK_GG
		AND glbgroup.GG_Code != 'ALL'
    WHERE
        link.GK_SystemCreateTimeUtc IS NULL
        OR COALESCE(link.GK_SystemCreateUser, '') = ''";
}
