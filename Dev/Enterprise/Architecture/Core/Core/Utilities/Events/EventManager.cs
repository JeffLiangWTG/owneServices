using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public class EventManager : ILastEditQuery
	{
		#region AddAuditLogEvent Overloads

		public void AddAuditLogEvent(string eventCode, string tableName, Guid parentPK,
			DateTime eventTime, string reference, string isEstimate, string isCancelled)
		{
			DoAddAuditLogEvent(eventCode, tableName, parentPK, eventTime, reference, isEstimate, isCancelled);
		}

		public void AddAuditLogEvent(string eventCode, string tableName, Guid parentPK,
			DateTime eventTime, string reference)
		{
			AddAuditLogEvent(eventCode, tableName, parentPK, eventTime, reference, DefaultIsEstimate, DefaultIsCancelled);
		}

		public void AddAuditLogEvent(string eventCode, string tableName, Guid parentPK, DateTime eventTime)
		{
			AddAuditLogEvent(eventCode, tableName, parentPK, eventTime, DefaultReference);
		}

		#endregion

		#region Logged Events for given record(s)

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Table Name for Events")]
		public const string EventsTableName = "Events";

		#endregion

		#region Last User Who Fired Event

		public string GetUserNameAndTimeOfLastEditOrDeleteOfARecord(DataRow row, bool isAutoLogged = true)
		{
			string result = "";

			string tablePrefix = GetTablePrefixFromColumnName(row.Table.Columns[0].ColumnName);

			if (!string.IsNullOrWhiteSpace(tablePrefix) && TableContainsSystemLastEditTimeAndUser(tablePrefix, row.Table))
			{
				if (row.RowState == DataRowState.Deleted)
				{
					result = FormatUserNameAndTimeMarker(EnvProxy.Instance.CurrentUser.FullName, ZDateTime.Now.ToDateTime());
				}
				else
				{
					string systemLastEditTimeColumnName = tablePrefix + "_SystemLastEditTimeUtc";
					string systemLastEditUserColumnName = tablePrefix + "_SystemLastEditUser";

					object lastEditUserObj = row[systemLastEditUserColumnName];
					object lastEditTimeObj = row[systemLastEditTimeColumnName];
					string lastEditUser = (lastEditUserObj == null || lastEditUserObj == DBNull.Value) ? "" : lastEditUserObj.ToString();
					DateTime lastEditTime = (lastEditTimeObj == null || lastEditTimeObj == DBNull.Value || !(lastEditTimeObj is DateTime)) ? ZDateTime.MinSmallDateTimeValue.ToDateTime() : (DateTime)lastEditTimeObj;
					result = FormatUserNameAndTimeMarker(GetUserFullNameFromCode(lastEditUser), lastEditTime);
				}
			}
			else if (isAutoLogged && row.Table.Columns[0].DataType == typeof(Guid))
			{
				result = GetUserNameAndTimeOfLastEditOrDeleteOfARecord(row.Table.TableName, DataUtils.GetPk(row));
			}

			return result;
		}

		string FormatUserNameAndTimeMarker(string userName, DateTime dateTime)
		{
			return userName + " @ " + EnvProxy.Instance.Time.GetLocalTimeFromUtc(dateTime).ToString("dd MMM yyyy HH:mm:ss");
		}

		string GetTablePrefixFromColumnName(string columnName)
		{
			Match tablePrefixMatch = tablePrefixRegex.Match(columnName);
			return (tablePrefixMatch != null) ? tablePrefixMatch.Value : string.Empty;
		}

		static readonly Regex tablePrefixRegex = new Regex(@"^\w{2,3}(?=_\w+$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		bool TableContainsSystemLastEditTimeAndUser(string tablePrefix, DataTable table)
		{
			string systemLastEditTimeColumnName = tablePrefix + "_SystemLastEditTimeUtc";
			string systemLastEditUserColumnName = tablePrefix + "_SystemLastEditUser";

			return table.Columns.Contains(systemLastEditUserColumnName) && table.Columns.Contains(systemLastEditTimeColumnName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public string GetUserNameAndTimeOfLastEditOrDeleteOfARecord(string tableName, Guid parentPK)
		{
			string sqlText = string.Format(@"
				SELECT TOP 1 {0}, {8}
				FROM  {1} INNER JOIN {2} ON {3} = {4}
				WHERE {5} = @SL_Parent
				AND   {6} = @SL_Table
				AND   {7} IN (@event1, @event2)
				ORDER BY {8} DESC",
				/*0*/GlbStaffSchema.Constants.GS_FullName,
				/*1*/StmALogSchema.Constants.TableName,  /*2*/GlbStaffSchema.Constants.TableName,  /*3*/StmALogSchema.Constants.SL_GS_NKUser,  /*4*/GlbStaffSchema.Constants.GS_Code,
				/*5*/StmALogSchema.Constants.SL_Parent,  /*6*/StmALogSchema.Constants.SL_Table,  /*7*/StmALogSchema.Constants.SL_SE_NKEvent,  /*8*/StmALogSchema.Constants.SL_PostedTimeUtc);

			string result = "";

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@SL_Parent", parentPK, StmALogSchema.SL_Parent);
				command.AddParameterBasedOnDbColumn("@SL_Table", tableName, StmALogSchema.SL_Table);
				command.AddParameterBasedOnDbColumn("@event1", "EDT", StmALogSchema.SL_SE_NKEvent);
				command.AddParameterBasedOnDbColumn("@event2", "DEL", StmALogSchema.SL_SE_NKEvent);

				using (IDataReader reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						result = FormatUserNameAndTimeMarker(reader[0].ToString(), (DateTime)reader[1]);
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public string GetUserCodeWhoAddedOrLastEditedRecord(string tableName, Guid parentPK)
		{
			string sqlText = string.Format(@"
				SELECT TOP 1 {0}
				FROM  {1}
				WHERE {2} = @SL_Parent
				AND   {3} = @SL_Table
				AND   {4} IN (@event1, @event2)
				ORDER BY {5} DESC",
				StmALogSchema.Constants.SL_GS_NKUser,
				StmALogSchema.Constants.TableName, StmALogSchema.Constants.SL_Parent, StmALogSchema.Constants.SL_Table,
				StmALogSchema.Constants.SL_SE_NKEvent, StmALogSchema.Constants.SL_PostedTimeUtc);

			DbCommand command = Db.Connection.Command(sqlText);
			command.AddParameterBasedOnDbColumn("@SL_Parent", parentPK, StmALogSchema.SL_Parent);
			command.AddParameterBasedOnDbColumn("@SL_Table", tableName, StmALogSchema.SL_Table);
			command.AddParameterBasedOnDbColumn("@event1", "ADD", StmALogSchema.SL_SE_NKEvent);
			command.AddParameterBasedOnDbColumn("@event2", "EDT", StmALogSchema.SL_SE_NKEvent);

			string result = Utilities.GetStringFromObject(command.ExecuteScalar());
			return result;
		}

		#endregion

		#region Implementation

		protected const string DefaultReference = "";
		protected const string DefaultIsEstimate = "N";
		protected const string DefaultIsCancelled = "N";

		protected
#if DEBUG
		virtual
#endif
		void DoAddAuditLogEvent(string eventCode, string tableName, Guid parentPK,
			DateTime eventTime, string reference, string isEstimate, string isCancelled)
		{
			// Validate Arguments
			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException("Invalid table name", nameof(tableName));
			}
			else if (eventTime <= SqlDateTime.MinValue.Value)
			{
				throw new ArgumentException("Invalid event time", nameof(eventTime));
			}

			var sql =
				"INSERT " + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName + // the only place to be
				"(" + StmALogSchema.Constants.PK +
				"," + StmALogSchema.Constants.SL_Table +
				"," + StmALogSchema.Constants.SL_Parent +
				"," + StmALogSchema.Constants.SL_IsEstimate +
				"," + StmALogSchema.Constants.SL_IsCancelled +
				"," + StmALogSchema.Constants.SL_Reference +
				"," + StmALogSchema.Constants.SL_EventTime +
				"," + StmALogSchema.Constants.SL_GS_NKUser +
				"," + StmALogSchema.Constants.SL_SE_NKEvent +
				"," + StmALogSchema.Constants.SL_PostedTimeUtc +
				"," + StmALogSchema.Constants.SL_GB_NKBranch +
				"," + StmALogSchema.Constants.SL_GE_NKDepartment +
				") VALUES (" +
				"NEWID(), @SL_Table, @SL_Parent, @SL_IsEstimate, @SL_IsCancelled," +
				"@SL_Reference, @SL_EventTime, @SL_GS_NKUser, @SL_SE_NKEvent, @SL_PostedTimeUtc, @SL_GB_NKBranch, @SL_GE_NKDepartment)";

			Db.Connection.RunInTransaction(() =>
			{
				Db.Connection.ExecuteNonQuery(sql
					, (cmd) =>
					{
						cmd.AddParameterBasedOnDbColumn("@SL_Table", tableName, StmALogSchema.SL_Table);
						cmd.AddParameterBasedOnDbColumn("@SL_Parent", parentPK, StmALogSchema.SL_Parent);
						cmd.AddParameterBasedOnDbColumn("@SL_IsEstimate", GetValidYesNoValue(isEstimate), StmALogSchema.SL_IsEstimate);
						cmd.AddParameterBasedOnDbColumn("@SL_IsCancelled", GetValidYesNoValue(isCancelled), StmALogSchema.SL_IsCancelled);
						cmd.AddParameterBasedOnDbColumn("@SL_Reference", reference, StmALogSchema.SL_Reference);
						cmd.AddParameterBasedOnDbColumn("@SL_EventTime", eventTime, StmALogSchema.SL_EventTime);
						cmd.AddParameterBasedOnDbColumn("@SL_GS_NKUser", EnvProxy.Instance.CurrentUser.Initials, StmALogSchema.SL_GS_NKUser);
						cmd.AddParameterBasedOnDbColumn("@SL_SE_NKEvent", eventCode, StmALogSchema.SL_SE_NKEvent);
						cmd.AddParameterBasedOnDbColumn("@SL_PostedTimeUtc", ZDateTime.UtcNow.ToDateTime(), StmALogSchema.SL_PostedTimeUtc);
						cmd.AddParameterBasedOnDbColumn("@SL_GB_NKBranch", EnvProxy.Instance.CurrentBranch != null ? EnvProxy.Instance.CurrentBranch.Code : "", StmALogSchema.SL_GB_NKBranch);
						cmd.AddParameterBasedOnDbColumn("@SL_GE_NKDepartment", EnvProxy.Instance.CurrentDepartment != null ? EnvProxy.Instance.CurrentDepartment.Code : "", StmALogSchema.SL_GE_NKDepartment);
					});
			});
		}

		protected string GetValidYesNoValue(string value)
		{
			string result = value.Trim().ToUpper();
			if (result != Constants.BooleanTrueString)
			{
				result = Constants.BooleanFalseString;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string GetUserFullNameFromCode(string userCode)
		{
			string sqlText = string.Format(@"
				SELECT {0}
				FROM  {1}
				WHERE {2} = @GS_Code",
			GlbStaffSchema.Constants.GS_FullName,
			GlbStaffSchema.Constants.TableName,
			GlbStaffSchema.Constants.GS_Code);

			DbCommand command = Db.Connection.Command(sqlText);
			command.AddParameterBasedOnDbColumn("@GS_Code", userCode, GlbStaffSchema.GS_Code);

			string result = Utilities.GetStringFromObject(command.ExecuteScalar());
			return result;
		}

		#endregion
	}
}
