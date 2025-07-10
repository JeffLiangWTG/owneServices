using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZDataException : Exception
	{
		public ZDataException(Exception innerEx, DataRow row, DbConnection connection, bool isTriggerException = false)
			: this(innerEx, row != null ? new List<DataRow> { row } : null, true, connection, isTriggerException)
		{
		}

		public ZDataException(Exception innerEx, IList<DataRow> rows, bool needPrintAll, DbConnection connection, bool isTriggerException = false)
			: base(CreateMessage(rows, innerEx, needPrintAll), innerEx)
		{
			Connection = connection;
			extraDebugInfo = null;
			this.row = rows?.FirstOrDefault();

			friendlyMessage = HandleUserDefinedError(isTriggerException);
		}

		protected ZDataException(Exception innerEx, string friendlyMessage, string debugMessage, DataRow row, DbConnection connection)
			: this(innerEx, row, connection)
		{
			this.friendlyMessage = friendlyMessage;
			this.extraDebugInfo = debugMessage;
		}

#if NETFRAMEWORK
		protected ZDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public DataRow Row
		{
			get { return row; }
		}

		public Dictionary<string, object> ColumnsDBChanged { get; set; } = new Dictionary<string, object>();

		public string FriendlyMessage
		{
			get
			{
				if (friendlyMessage == null && CoreErrorHandler != null)
				{
					friendlyMessage = CoreErrorHandler.GetDBErrorUserFriendlyMessage();
				}
				return NullToEmptyString(friendlyMessage);
			}
		}

		public bool CanRecover
		{
			get { return CoreErrorHandler == null || CoreErrorHandler.CanRecover; }
		}

		public virtual bool ShouldBeReportedToEDI
		{
			get
			{
				return CoreErrorHandler != null &&
					(
						CoreErrorHandler.ExceptionType == DbErrorType.DeadlockError
							|| CoreErrorHandler.ExceptionType == DbErrorType.LockTimeoutExpired
							|| CoreErrorHandler.ExceptionType == DbErrorType.TimeoutExpired
							|| CoreErrorHandler.ExceptionType == DbErrorType.InsertConflictedWithCheckConstraint
					);
			}
		}

		public string ExtraDebugInfo
		{
			get
			{
				if (extraDebugInfo == null && CoreErrorHandler != null)
				{
					extraDebugInfo = CoreErrorHandler.GetExtraDebugInformation();
				}
				return NullToEmptyString(extraDebugInfo);
			}
		}

		public DbErrorType DbErrorType
		{
			get
			{
				return CoreErrorHandler?.ExceptionType ?? DbErrorType.NotHandled;
			}
		}

		protected virtual string HandleUserDefinedError(bool isTriggerException)
		{
			return (InnerException is SqlException sqlException && sqlException.Number == 50000) ? sqlException.Message : null;
		}

		string NullToEmptyString(string s)
		{
			return s ?? "";
		}

		public DbErrorHandler CoreErrorHandler
		{
			get
			{
				if (coreErrorHandler == null && InnerException is SqlException sqlEx)
				{
					coreErrorHandler = new DbErrorHandler(sqlEx, Connection);
				}
				return coreErrorHandler;
			}
		}

		DbErrorHandler coreErrorHandler;
		string friendlyMessage;
		string extraDebugInfo;
		readonly DataRow row;
		readonly DbConnection Connection;

		#region SuppressResourceStringsCheckRegion

		public static string CreateMessage(DataRow row, Exception innerEx)
		{
			var result = string.Empty;
			if (row == null)
			{
				result = "<ROW IS NULL>";
			}
			else
			{
				result += CreateMessageForRow(row);
			}

			if (innerEx != null)
			{
				result += System.Environment.NewLine + "InnerException Message = " + innerEx.Message;
			}

			return result;
		}

		public static string CreateMessage(IList<DataRow> rows, Exception innerEx, bool needPrintAll)
		{
			var result = string.Empty;
			if (rows == null || rows.Count == 0)
			{
				result = "<ROW IS NULL>";
			}
			else
			{
				var rowsToReport = needPrintAll ? rows : rows.Take(1);
				result = string.Join(Environment.NewLine, rowsToReport.Select(CreateMessageForRow));
			}

			if (innerEx != null)
			{
				result += System.Environment.NewLine + "InnerException Message = " + innerEx.Message;
			}

			return result;
		}

		static string CreateMessageForRow(DataRow row)
		{
			var result = "Error from Data layer: ";

			result += "TableName=" + row.Table.TableName + ", PK=" + DataUtils.GetPk(row) + ", RowState=" + row.RowState.ToString();
			try
			{
				if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
				{
					foreach (DataColumn column in row.Table.Columns)
					{
						result += System.Environment.NewLine + column.ColumnName + " = " + row[column].ToString();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result += System.Environment.NewLine + "Error generating row dump : " + ex.ToString();
			}

			return result;
		}

		#endregion

		#region TestCase
#if DEBUG
		public void SetFriendlyMessageForTest(string message)
		{
			friendlyMessage = message;
		}

#endif
		#endregion

	}

	[Serializable]
	public class ZDataConcurrencyException : ZDataException, IConcurrencyException
	{
		public ZDataConcurrencyException(Exception innerException, DataRow row, DbConnection connection)
			: this(innerException, row, connection, false)
		{
		}

		public ZDataConcurrencyException(Exception innerException, DataRow row, DbConnection connection, bool isTriggerException)
			: base(innerException, row, connection, isTriggerException)
		{
		}

		public ZDataConcurrencyException(Exception innerException, IList<DataRow> rows, bool needPrintAll, DbConnection connection, bool isTriggerException)
			: base(innerException, rows, needPrintAll, connection, isTriggerException)
		{
		}

		public ZDataConcurrencyException(Exception innerException, DataRow row, DbConnection connection, Action<List<Guid>> recoverAction)
			: base(innerException, row, connection)
		{
			RecoverAction = recoverAction;
		}

		public ZDataConcurrencyException(Exception innerException, IList<DataRow> rows, bool needPrintAll, DbConnection connection, Action<List<Guid>> recoverAction)
			: base(innerException, rows, needPrintAll, connection)
		{
			RecoverAction = recoverAction;
		}

#if NETFRAMEWORK
		protected ZDataConcurrencyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		protected override string HandleUserDefinedError(bool isTriggerException)
		{
			return isTriggerException ? base.HandleUserDefinedError(isTriggerException) : null;
		}

		public Action<List<Guid>> RecoverAction { get; }
	}

	[Serializable]
	public class ZUniqueIndexViolationException : ZDataException
	{
		public ZUniqueIndexViolationException(SqlException innerEx, DbConnection connection, DataTable table, string tablePrefix)
			: base(innerEx, GetDuplicatedRowOrNull(innerEx, table, tablePrefix), connection)
		{
		}

#if NETFRAMEWORK
		protected ZUniqueIndexViolationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static DataRow GetDuplicatedRowOrNull(SqlException innerEx, DataTable table, string tablePrefix)
		{
			var result = default(DataRow);

			if (table != null)
			{
				var formatter = new SqlExceptionReformatter().Reformat(innerEx);
				var duplicatedValues = formatter.DuplicatedValue.Split(',');
				var columnNames = MetaData.GetColumnNamesByIndexNameAndTablePrefix(innerEx.Message, tablePrefix);
				if (!columnNames.IsNullOrEmpty() && columnNames.Length == duplicatedValues.Length)
				{
					var filterStringBuilder = new StringBuilder();
					for (var i = 0; i < columnNames.Length; i++)
					{
						filterStringBuilder.Append(columnNames[i] + " = ");
						var valueType = table.Columns[columnNames[i]].DataType;
						filterStringBuilder.Append(NumericUtil.IsNumeric(valueType)
							? $"{duplicatedValues[i]}"
							: $"'{duplicatedValues[i]}'");

						if (i < columnNames.Length - 1)
						{
							filterStringBuilder.Append(" and ");
						}
					}

					result = table.Select(filterStringBuilder.ToString()).FirstOrDefault();
				}
			}

			return result;
		}
	}
}
