using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core;

public class BackFillMissingDetachedAttachedLogForGlbStaffAndGlbGroup : DataTransformation
{
	public override string UserDescription => "Back fill missing attached or detached log in StmALog";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		try
		{
			var offset = 0;
			var groupLinks = FetchGroupLinks(offset);
			while (groupLinks.Count > 0)
			{
				foreach (var groupLink in groupLinks)
				{
					var allLogs = FetchAllLogs(groupLink);
					if (allLogs.Count == 0)
					{
						ProcessLogsForNoAttachLogs(groupLink, allLogs);
					}
					else
					{
						var logsGroupByTable = allLogs.GroupBy(x => x.SL_Table);
						foreach (var logs in logsGroupByTable)
						{
							var list = logs.ToList();
							ProcessLogsForNoAttachLogs(groupLink, list);
							ProcessLogsForLatestLogIsDetached(groupLink, list);
						}
					}
				}

				offset += MaxRowsCount;
				groupLinks = FetchGroupLinks(offset);
			}

			ProcessOnlyHaveDetachedLogs();
		}
		catch (Exception e)
		{
			ErrorReporter.ReportOnce("BackFillMissingDetachedAttachedLogForGlbStaffAndGlbGroup", e);
		}
	}

	List<AttachedDetachedLogBusinessObject> FetchAllLogs(GroupLink groupLink)
	{
		var list = new List<AttachedDetachedLogBusinessObject>();
		var sql = @"
SELECT SL_PK, SL_Parent, SL_Table, SL_Reference, SL_PostedTimeUtc
FROM dbo.StmALog
WHERE (SL_Parent = @StaffPK AND (SL_Reference LIKE 'Attached - (' + @GroupCode + ')%' OR SL_Reference LIKE 'Detached - (' + @GroupCode + ')%'))
   OR (SL_Parent = @GroupPK AND (SL_Reference LIKE 'Attached - (' + @StaffCode + ')%' OR SL_Reference LIKE 'Detached - (' + @StaffCode + ')%'))";
		using (var cmd = Db.Connection.Command(sql))
		{
			cmd.AddParameter("@StaffPK", SqlDbType.UniqueIdentifier, groupLink.GK_GS);
			cmd.AddParameter("@GroupPK", SqlDbType.UniqueIdentifier, groupLink.GK_GG);
			cmd.AddParameter("@StaffCode", SqlDbType.VarChar, groupLink.GS_Code);
			cmd.AddParameter("@GroupCode", SqlDbType.VarChar, groupLink.GG_Code);

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = (Guid)reader["SL_PK"];
					var parent = (Guid)reader["SL_Parent"];
					var slTable = (string)reader["SL_Table"];
					var reference = (string)reader["SL_Reference"];
					var postedTimeUtc = (DateTime)reader["SL_PostedTimeUtc"];

					list.Add(new AttachedDetachedLogBusinessObject(pk, parent, slTable, reference, postedTimeUtc));
				}
			}
		}

		return list;
	}

	List<GroupLink> FetchGroupLinks(int offset)
	{
		var groupLinks = new List<GroupLink>();
		using (var cmd = Db.Connection.Command(GroupLinksSql))
		{
			cmd.AddParameter("@Offset", SqlDbType.Int, offset);
			cmd.AddParameter("@MaxRowsCount", SqlDbType.Int, MaxRowsCount);
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var groupLink = new GroupLink
					{
						GK_PK = (Guid)reader["GK_PK"],
						GK_GS = (Guid)reader["GK_GS"],
						GK_GG = (Guid)reader["GK_GG"],
						GS_FullName = (string)reader["GS_FullName"],
						GS_Code = (string)reader["GS_Code"],
						GG_Desc = (string)reader["GG_Desc"],
						GG_Code = (string)reader["GG_Code"],
						GK_SystemCreateTimeUtc = reader.IsDBNull(reader.GetOrdinal("GK_SystemCreateTimeUtc")) ? DateTime.MinValue : (DateTime)reader["GK_SystemCreateTimeUtc"],
						GS_SystemCreateTimeUtc = reader.IsDBNull(reader.GetOrdinal("GS_SystemCreateTimeUtc")) ? DateTime.MinValue : (DateTime)reader["GS_SystemCreateTimeUtc"],
						GG_SystemCreateTimeUtc = reader.IsDBNull(reader.GetOrdinal("GG_SystemCreateTimeUtc")) ? DateTime.MinValue : (DateTime)reader["GG_SystemCreateTimeUtc"]
					};

					groupLinks.Add(groupLink);
				}
			}
		}

		return groupLinks;
	}

	void ProcessLogsForNoAttachLogs(GroupLink groupLink, List<AttachedDetachedLogBusinessObject> logs)
	{
		if (logs.Any(x => x.SL_Reference.Contains("Attached")))
		{
			return;
		}

		DateTime postTimeUtc;

		var code = groupLink.GG_Code;
		var desc = groupLink.GG_Desc;
		if (logs.Count > 0)
		{
			var log = logs.FirstOrDefault();
			code = log.SL_Table == GlbGroupSchema.Constants.TableName ? groupLink.GS_Code : groupLink.GG_Code;
			desc = log.SL_Table == GlbGroupSchema.Constants.TableName ? groupLink.GS_FullName : groupLink.GG_Desc;
		}

		var detachedReference = $"Detached - ({code})";
		var lastedDetachLog = logs.OrderByDescending(x => x.SL_PostedTimeUtc)
			.FirstOrDefault(x => x.SL_Reference.StartsWith(detachedReference));
		if (lastedDetachLog == null)
		{
			postTimeUtc = logs.Count == 0 ? GetMaxValueInGroupCreateTimeAndStaffCreateTime(groupLink) : groupLink.GK_SystemCreateTimeUtc;
		}
		else
		{
			postTimeUtc = lastedDetachLog.SL_PostedTimeUtc.AddMinutes(1);
		}

		if (logs.Count == 0)
		{
			if (groupLink.GK_SystemCreateTimeUtc != DateTime.MinValue)
			{
				postTimeUtc = groupLink.GK_SystemCreateTimeUtc;
			}

			var referenceGG = $"Attached - ({groupLink.GG_Code}) {groupLink.GG_Desc}";
			var pkGG = CreateStmALog("GlbStaff", groupLink.GK_GS, referenceGG, postTimeUtc);
			logs.Add(new AttachedDetachedLogBusinessObject(pkGG, groupLink.GK_GS, "GlbStaff", referenceGG, postTimeUtc));

			var referenceGS = $"Attached - ({groupLink.GS_Code}) {groupLink.GS_FullName}";
			var pkGS = CreateStmALog("GlbGroup", groupLink.GK_GG, referenceGS, postTimeUtc);
			logs.Add(new AttachedDetachedLogBusinessObject(pkGS, groupLink.GK_GG, "GlbGroup", referenceGS, postTimeUtc));
		}
		else
		{
			var reference = $"Attached - ({code}) {desc}";
			var pk = CreateStmALog(logs[0].SL_Table, logs[0].SL_Parent, reference, postTimeUtc);
			logs.Add(new AttachedDetachedLogBusinessObject(pk, logs[0].SL_Parent, logs[0].SL_Table, reference, postTimeUtc));
		}
	}

	void ProcessLogsForLatestLogIsDetached(GroupLink groupLink, List<AttachedDetachedLogBusinessObject> logs)
	{
		if (!logs.Any())
		{
			return;
		}

		var latestLog = logs.OrderByDescending(x => x.SL_PostedTimeUtc).First();
		if (latestLog.SL_Reference.StartsWith("Detached"))
		{
			var code = latestLog.SL_Table == GlbGroupSchema.Constants.TableName ? groupLink.GS_Code : groupLink.GG_Code;
			var desc = latestLog.SL_Table == GlbGroupSchema.Constants.TableName ? groupLink.GS_FullName : groupLink.GG_Desc;

			var reference = $"Attached - ({code}) {desc}";
			var postTimeUtc = groupLink.GK_SystemCreateTimeUtc == DateTime.MinValue ? latestLog.SL_PostedTimeUtc.AddMinutes(1) : groupLink.GK_SystemCreateTimeUtc;
			var pk = CreateStmALog(latestLog.SL_Table, latestLog.SL_Parent, reference, postTimeUtc);
			logs.Add(new AttachedDetachedLogBusinessObject(pk, latestLog.SL_Parent, latestLog.SL_Table, reference, postTimeUtc));
		}
	}

	void ProcessOnlyHaveDetachedLogs()
	{
		var offset = 0;
		var pks = FetchVaildPKS(offset);
		while (pks.Count > 0)
		{
			foreach (var guid in pks)
			{
				var allLogs = FetchLogsBaseOnSLParent(guid);
				var detachedLogs = allLogs.Where(x => x.SL_Reference.StartsWith("D"))
					.GroupBy(x => x.SL_Reference).Select(g => g.First()).ToList();
				var onlyHaveDetachedLogRecords = detachedLogs.Where(x => !allLogs.Any(y => y.SL_Reference.StartsWith($"Attached - ({x.Code})"))).ToList();
				if (onlyHaveDetachedLogRecords.Count > 0)
				{
					foreach (var logRecord in onlyHaveDetachedLogRecords)
					{
						var maxCreateTime = GetMaxCreateTime(logRecord);
						if (maxCreateTime == DateTime.MinValue)
						{
							maxCreateTime = logRecord.SL_PostedTimeUtc.AddMinutes(-1);
						}

						var index = logRecord.SL_Reference.IndexOf('-');

						if (index != -1 && index < logRecord.SL_Reference.Length - 1)
						{
							var reference = "Attached -" + logRecord.SL_Reference.Substring(index + 1);
							CreateStmALog(logRecord.SL_Table, logRecord.SL_Parent, reference, maxCreateTime);
						}
					}
				}
			}

			offset += MaxRowsCount;
			pks = FetchVaildPKS(offset);
		}
	}

	List<OnlyHaveDetachedLogRecord> FetchLogsBaseOnSLParent(Guid slParent)
	{
		var list = new List<OnlyHaveDetachedLogRecord>();
		const string sql = @"
SELECT SL_PK, SL_Parent, SL_Table, SL_Reference, SL_PostedTimeUtc 
FROM dbo.StmALog 
WHERE SL_Parent = @ParentPK AND (SL_Reference LIKE 'Attached - %' OR SL_Reference LIKE 'Detached - %')";
		using (var cmd = Db.Connection.Command(sql))
		{
			cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, slParent);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var invalidLog = new OnlyHaveDetachedLogRecord();
				invalidLog.SL_Parent = (Guid)reader["SL_Parent"];
				invalidLog.SL_Table = (string)reader["SL_Table"];
				invalidLog.SL_Reference = (string)reader["SL_Reference"];
				invalidLog.SL_PostedTimeUtc = (DateTime)reader["SL_PostedTimeUtc"];
				var pattern = @"\((.*?)\)";
				Match match = Regex.Match(invalidLog.SL_Reference, pattern);
				if (match.Success)
				{
					invalidLog.Code = match.Groups[1].Value;
					list.Add(invalidLog);
				}
			}
		}

		return list;
	}

	DateTime GetMaxCreateTime(OnlyHaveDetachedLogRecord logRecord)
	{
		var sql = "";
		if (logRecord.SL_Table == GlbGroupSchema.Constants.TableName)
		{
			sql = @"SELECT MAX(CreateTime)
FROM (
    SELECT GG_SystemCreateTimeUtc AS CreateTime
    FROM dbo.GlbGroup
    WHERE GG_PK = @PK
    UNION ALL
    SELECT GS_SystemCreateTimeUtc AS CreateTime
    FROM dbo.GlbStaff
    WHERE GS_Code = @Code
) AS TEMP;
";
		}
		else
		{
			sql = @"SELECT MAX(CreateTime)
FROM (
    SELECT GG_SystemCreateTimeUtc AS CreateTime
    FROM dbo.GlbGroup
    WHERE GG_Code = @Code
    UNION ALL
    SELECT GS_SystemCreateTimeUtc AS CreateTime
    FROM dbo.GlbStaff
    WHERE GS_PK = @PK
) AS TEMP;
";
		}

		using (var command = Db.Connection.Command(sql))
		{
			command.AddParameter("@PK", SqlDbType.UniqueIdentifier, logRecord.SL_Parent);
			command.AddParameter("@Code", SqlDbType.VarChar, logRecord.Code);
			var createTime = command.ExecuteScalar();
			return createTime == DBNull.Value ? DateTime.MinValue : (DateTime)createTime;
		}
	}

	DateTime GetMaxValueInGroupCreateTimeAndStaffCreateTime(GroupLink groupLink)
	{
		return groupLink.GG_SystemCreateTimeUtc > groupLink.GS_SystemCreateTimeUtc ? groupLink.GG_SystemCreateTimeUtc : groupLink.GS_SystemCreateTimeUtc;
	}

	Guid CreateStmALog(string tableName, Guid parentPK, string reference, DateTime utcDate)
	{
		var time = utcDate == DateTime.MinValue ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) : utcDate;
		var pk = Guid.NewGuid();
		using (var command = Db.Connection.Command(CreateStmALogSql))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@tableName", SqlDbType.VarChar, StmALogSchema.SL_Table.MaxLength, tableName);
			command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
			command.AddParameter("@userCode", SqlDbType.VarChar, StmALogSchema.SL_GS_NKUser.MaxLength, "~BP");
			command.AddParameter("@cancelled", SqlDbType.VarChar, StmALogSchema.SL_IsCancelled.MaxLength, "N");
			command.AddParameter("@eventCode", SqlDbType.VarChar, StmALogSchema.SL_SE_NKEvent.MaxLength, "EDT");
			command.AddParameter("@reference", SqlDbType.VarChar, StmALogSchema.SL_Reference.MaxLength, reference.Trim() + " (Calculated)");
			command.AddParameter("@date", SqlDbType.SmallDateTime, time);
			command.AddParameter("@utcDate", SqlDbType.SmallDateTime, time);
			command.ExecuteNonQuery();
		}

		return pk;
	}

	List<Guid> FetchVaildPKS(int offset)
	{
		var list = new List<Guid>();
		using (var cmd = Db.Connection.Command(ValidParentPKsSql))
		{
			cmd.AddParameter("@Offset", SqlDbType.Int, offset);
			cmd.AddParameter("@MaxRowsCount", SqlDbType.Int, MaxRowsCount);
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var parent = (Guid)reader["PK"];
					list.Add(parent);
				}
			}
		}

		return list;
	}

	class GroupLink
	{
		public Guid GK_PK { get; set; }
		public Guid GK_GS { get; set; }
		public Guid GK_GG { get; set; }
		public string GS_FullName { get; set; }
		public string GS_Code { get; set; }
		public string GG_Desc { get; set; }
		public string GG_Code { get; set; }

		public DateTime GK_SystemCreateTimeUtc { get; set; }

		public DateTime GS_SystemCreateTimeUtc { get; set; }

		public DateTime GG_SystemCreateTimeUtc { get; set; }
	}

	class AttachedDetachedLogBusinessObject
	{
		public AttachedDetachedLogBusinessObject(Guid slPk, Guid slParent, string slTable, string slReference, DateTime slPostedTimeUtc)
		{
			SL_PK = slPk;
			SL_Parent = slParent;
			SL_Table = slTable;
			SL_Reference = slReference;
			SL_PostedTimeUtc = slPostedTimeUtc;
		}

		public Guid SL_PK;

		public Guid SL_Parent;
		public string SL_Table;
		public string SL_Reference;
		public DateTime SL_PostedTimeUtc;
	}

	class OnlyHaveDetachedLogRecord
	{
		public string SL_Table { get; set; }
		public Guid SL_Parent { get; set; }
		public string Code { get; set; }
		public string SL_Reference { get; set; }
		public DateTime SL_PostedTimeUtc { get; set; }
	}

	const string ValidParentPKsSql = @"
SELECT *
FROM (
SELECT GG_PK AS PK, GG_SystemCreateTimeUtc AS CreateTime
      FROM dbo.GlbGroup
      WHERE GG_Code != 'ALL'
      UNION ALL
      SELECT GS_PK AS PK, GS_SystemCreateTimeUtc AS CreateTime
      FROM dbo.GlbStaff
      WHERE GS_IsSystemAccount = 0
      ) Temp
ORDER BY CreateTime
OFFSET @Offset ROWS FETCH NEXT @MaxRowsCount ROWS ONLY";

	const string GroupLinksSql = @"SELECT GK_GS,
       GK_PK,
       GK_GG,
       GS_FullName,
       GS_Code,
       GG_Desc,
       GG_Code,
       GK_SystemCreateTimeUtc,
       GG_SystemCreateTimeUtc,
       GS_SystemCreateTimeUtc
FROM dbo.GlbGroupLink link
         JOIN dbo.GlbStaff staff ON staff.GS_PK = link.GK_GS
    AND staff.GS_IsSystemAccount = 0
         JOIN dbo.GlbGroup glbGroup ON link.GK_GG = glbGroup.GG_PK
    AND glbGroup.GG_Code != 'ALL'
ORDER BY link.GK_SystemCreateTimeUtc
OFFSET @Offset ROWS FETCH NEXT @MaxRowsCount ROWS ONLY";

	const string CreateStmALogSql = @"INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_GS_NKUser, SL_SE_NKEvent, SL_EventTime, SL_EventTimeUtc, SL_PostedTimeUtc, SL_Reference, SL_IsCancelled)
			VALUES (@pk, @tableName, @parentPK, @userCode, @eventCode, @date, @utcDate, @utcDate, @reference, @cancelled)";

	const int MaxRowsCount = 10;
}
