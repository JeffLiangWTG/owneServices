using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.PreUpgrade;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	class ColumnCreatorPopulateAuditTimeAndUser : ColumnCreatorWithDefaultValue
	{
		public ColumnCreatorPopulateAuditTimeAndUser(IUpgradeManager manager, ColumnChangeMetadata columnMetadata, string dbBeingUpgraded, IPopulateAuditTimeAndUserInfo populateAuditTimeAndUserInfo)
			: base(manager, columnMetadata, dbBeingUpgraded)
		{
			this.populateInfo = populateAuditTimeAndUserInfo;
			this.sourceAlias = "source_StmALog";
		}

		readonly IPopulateAuditTimeAndUserInfo populateInfo;
		readonly string sourceAlias;

		protected override string GetPopulateExpression()
		{
			if (populateInfo.CreateTime != null && columnMetadata.ColumnName.Equals(populateInfo.CreateTime.Name, StringComparison.OrdinalIgnoreCase))
			{
				return String.Format("{0}.CreateTimeUTCExpression", this.sourceAlias);
			}
			else if (populateInfo.CreateUser != null && columnMetadata.ColumnName.Equals(populateInfo.CreateUser.Name, StringComparison.OrdinalIgnoreCase))
			{
				return String.Format("{0}.CreateUserExpression", this.sourceAlias);
			}
			else if (populateInfo.LastEditTime != null && columnMetadata.ColumnName.Equals(populateInfo.LastEditTime.Name, StringComparison.OrdinalIgnoreCase))
			{
				return String.Format("{0}.EditTimeUTCExpression", this.sourceAlias);
			}
			else if (populateInfo.LastEditUser != null && columnMetadata.ColumnName.Equals(populateInfo.LastEditUser.Name, StringComparison.OrdinalIgnoreCase))
			{
				return String.Format("{0}.EditUserExpression", this.sourceAlias);
			}
			else
			{
				throw new Exception(String.Format("[{0}] cannot be handled by this column creator.", columnMetadata.ColumnName));
			}
		}

		protected override string GetPopulateSource()
		{
			var source = GetSource(String.Format("[{0}].[{1}].[{2}] ", dbBeingUpgraded, Db.SqlDbOwnerSchema, StmALogSchema.Constants.TableName));
			return String.Format(@"JOIN{0} ON {1}.Parent = target.[{2}]"
				, source                           // 0
				, this.sourceAlias                 // 1
				, populateInfo.TableSchema.PK.Name // 2
				);
		}

		string GetSource(string fromClause)
		{
			return String.Format(@"
	(
		SELECT
			Parent = t.{0}

			, CreateTimeUTCExpression = COALESCE(t.add_time_utc, t.edt_first_time_utc)
			, CreateUserExpression    = COALESCE(t.add_user, t.edt_first_user)

			, EditTimeUTCExpression   = COALESCE(t.edt_last_time_utc, t.add_time_utc)
			, EditUserExpression      = COALESCE(t.edt_last_user, t.add_user)
		FROM
			(
				SELECT
					{0}

					, add_user     = MAX(CASE {1} WHEN 'ADD' THEN {2} END)
					, add_time_utc = MAX(CASE {1} WHEN 'ADD' THEN {3} END)

					, edt_first_user     = SUBSTRING(MIN(CASE {1} WHEN 'EDT' THEN CONVERT(char(23), {3}, 121) + {2} END), 24, 3)
					, edt_first_time_utc = MIN(CASE {1} WHEN 'EDT' THEN {3} END)

					, edt_last_user      = SUBSTRING(MAX(CASE {1} WHEN 'EDT' THEN CONVERT(char(23), {3}, 121) + {2} END), 24, 3)
					, edt_last_time_utc  = MAX(CASE {1} WHEN 'EDT' THEN {3} END)
				FROM
					{7}
				WHERE 1=1
					AND {4} = '{5}'
					AND {1} in ('ADD', 'EDT')
				GROUP BY
					{0}
			) AS t
	) AS {6}"
				, StmALogSchema.Constants.SL_Parent        // 0
				, StmALogSchema.Constants.SL_SE_NKEvent    // 1
				, StmALogSchema.Constants.SL_GS_NKUser     // 2
				, StmALogSchema.Constants.SL_PostedTimeUtc // 3
				, StmALogSchema.Constants.SL_Table         // 4
				, columnMetadata.TableName                 // 5
				, this.sourceAlias                         // 6
				, fromClause                               // 7
			);
		}
	}
}
