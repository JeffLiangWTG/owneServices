using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.MemoryManagement;
using CargoWise.Data.Diagnostics;
using CargoWise.Data.Providers.Common;

namespace CargoWise.Data
{
	public interface IEnableStateControl
	{
		void SetState(bool newValue);
	}

	public sealed class SqlEventTracker : IEnableStateControl
	{
		static SqlEventTracker()
		{
			MemoryManager.Register("SQL Event Tracker", FlushCallback.OnAnyThread, delegate (FlushAction action) // diagnostic tool name
			{
				Instance.Clear();
				return FlushResult.Exhausted;
			});
		}

		internal SqlEventTracker()
		{
			this.sqlEventList = new RingBuffer<string>(MaxSqlEvents);
			this.FailedSqlEventList = new RingBuffer<string>(MaxFailedSqlEvents);
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly SqlEventTracker instance = new();

		public static SqlEventTracker Instance
		{
			get
			{
				return instance;
			}
		}

		#region Sql Event Tracking

		public void Clear()
		{
			lock (sqlEventList)
			{
				sqlEventList.Clear();
				FailedSqlEventList.Clear();
			}
		}

		#region Enabled

		public bool IsEnabled
		{
			get { return isEnabled; }
		}

		void IEnableStateControl.SetState(bool newValue)
		{
			isEnabled = newValue;
		}

		bool isEnabled = true;

		#endregion

		public bool HasQueries
		{
			get
			{
				lock (sqlEventList)
				{
					return sqlEventList.Count > 0;
				}
			}
		}

		public string LastSqlQuery => GetSqlQuery(0, "LAST SQL STATEMENT FROM THIS CONNECTION"); // diagnostic message

		public string ThirdToLastSqlQuery => GetSqlQuery(2, "PENULTIMATE SQL STATEMENT FROM THIS CONNECTION"); // diagnostic message

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		string GetSqlQuery(int tailNumber, string headerLabelText)
		{
			var sb = new StringBuilder();
			if (string.IsNullOrEmpty(headerLabelText))
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "\r\n\r\n-- {0} TAIL SQL STATEMENT FROM THIS CONNECTION  --\r\n\r\n", tailNumber + 1); // diagnostic message
			}
			else
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "\r\n\r\n-- {0} --\r\n\r\n", headerLabelText); // diagnostic message
			}
			try
			{
				lock (sqlEventList)
				{
					var index = sqlEventList.Count - tailNumber - 1;
					sb.AppendLine(index >= 0 && index < sqlEventList.Count
						? sqlEventList[index]
						: "No Sql statements logged"); // diagnostic message
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				sb.AppendLine("Error getting sql query: " + ex); // exception message
			}
			return sb.ToString();
		}

		public void AddSqlEvent(IDbCommand sqlCommand, Exception e, TimeSpan elapsed)
		{
			if (!(!(this.IsEnabled) || sqlCommand != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(sqlCommand));
			}

			if (!(!(this.IsEnabled) || sqlCommand.CommandText != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(sqlCommand));
			}

			if (IsEnabled)
			{
				var statement = sqlCommand.CommandText;
				var startOfStatement = statement.Length > 100 ? statement.Substring(0, 100) : statement;

				var isSelectStatement = startOfStatement.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
				var timeNow = DateTime.Now; // Cannot use Env.Time as it results in an infinite
				var sql = new StringBuilder();
				var commandTrimmer = new CommandTextTrimmer();

				if (e != null)
				{
					statement = commandTrimmer.EnsureCommandThatCausedExceptionIsNotTruncated(e, statement);
					sql.Append("[COMMAND FAILED: \r\n" + e.Message + "]\r\n\r\n"); // diagnostic message
					sql.AppendLine(QueryStackTraceRecorderCore.StackTraceHeader);

					if (e.InnerException != null)
					{
						sql.AppendLine("Inner Exception Details");
						sql.AppendLine(e.InnerException.StackTrace);
						sql.AppendLine("--- End of inner exception stack trace ---");
					}

					var stackTraceWithoutCallToThisFunction =
						string.Join(Environment.NewLine, new System.Diagnostics.StackTrace().ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList());
					sql.AppendLine(e.StackTrace);
					sql.AppendLine(stackTraceWithoutCallToThisFunction);
					sql.AppendLine(QueryStackTraceRecorderCore.StackTraceFooter);
				}

				if (isSelectStatement)
				{
					sql.Append(commandTrimmer.GetTruncatedText(statement) + "\r\n");
				}
				else
				{
					sql.Append(commandTrimmer.GetTruncatedTextWithoutBlobs(statement) + "\r\n");
				}

				foreach (var parameterString in GetSqlParameterStrings(sqlCommand))
				{
					sql.Append(FormattableString.Invariant($"   {parameterString}\r\n")); // diagnostic message
				}

				sql.Append("\r\n");

				sql.Append("   [Time=" + timeNow.ToLongTimeString() + "]\r\n"); // diagnostic message
				sql.Append("   [Duration=" + elapsed.TotalMilliseconds + "ms]\r\n"); // diagnostic message
				sql.Append("   [Transaction=" + ((sqlCommand.Transaction == null) ? "false" : "true") + "]\r\n"); // diagnostic message
				sql.Append("   [Current thread ID=" + Thread.CurrentThread.ManagedThreadId + "]\r\n"); // diagnostic message
				sql.Append("\r\n");

				var sqlString = sql.ToString();

				lock (sqlEventList)
				{
					sqlEventList.Add(sqlString);
				}

				if (e != null)
				{
					lock (FailedSqlEventList)
					{
						FailedSqlEventList.Add(sqlString);
					}
				}

				if (SqlCommandExecutedEvent != null)
				{
					var commandText = string.Concat(
						AddCommandParameters(sqlCommand),
						GetStackTraceAsComment());

					var args = new SqlCommandExecutedEventArgs(commandText, e, timeNow);

					if (SqlCommandExecutedEvent != null)
					{
						SqlCommandExecutedEvent(args);
					}
				}
			}
		}

		public static IEnumerable<string> GetSqlParameterStrings(IDbCommand sqlCommand)
		{
			foreach (System.Data.Common.DbParameter paramBase in sqlCommand.Parameters)
			{
				var param = new SqlParameterWrapper(paramBase);
				var paramString = param.Value == DBNull.Value || param.Value == null ? "NULL" : Convert.ToString(param.Value);

				if (paramString.Length > 2048 + 24) //24 being length of the extra text added
				{
					var bytesSkipped = paramString.Length - 2048;
					paramString = paramString.Substring(0, 2048) + string.Format(CultureInfo.InvariantCulture, "... ({0} bytes skipped)", bytesSkipped); // diagnostic message
				}

				yield return FormattableString.Invariant($"[{param.ParameterName} = \"{paramString}\", {param.SqlDbType}({param.Size})]"); // diagnostic message
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Cannot use Env.Time as it results in an infinite")]
		public void AddSqlEvent(ISqlBulkCopy bulkCopy, Exception e, TimeSpan elapsed)
		{
			if (!(!IsEnabled || bulkCopy != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(bulkCopy));
			}

			if (IsEnabled)
			{
				var statement = $"insert bulk {bulkCopy.DestinationTableName} with ({bulkCopy.BulkCopyOptions})"; // diagnostic message

				var timeNow = DateTime.Now; // Cannot use Env.Time as it results in an infinite
				var sql = new StringBuilder();

				if (e != null)
				{
					sql.Append("[BULKCOPY FAILED: \r\n" + e.Message + "]\r\n\r\n"); // diagnostic message
					sql.AppendLine(QueryStackTraceRecorderCore.StackTraceHeader);

					if (e.InnerException != null)
					{
						sql.AppendLine("Inner Exception Details");
						sql.AppendLine(e.InnerException.StackTrace);
						sql.AppendLine("--- End of inner exception stack trace ---");
					}

					var stackTraceWithoutCallToThisFunction =
						string.Join(Environment.NewLine, new System.Diagnostics.StackTrace().ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList());
					sql.AppendLine(e.StackTrace);
					sql.AppendLine(stackTraceWithoutCallToThisFunction);
					sql.AppendLine(QueryStackTraceRecorderCore.StackTraceFooter);
				}

				sql.Append(statement + "\r\n");

				sql.Append("   [Time=" + timeNow.ToLongTimeString() + "]\r\n"); // diagnostic message
				sql.Append("   [Duration=" + elapsed.TotalMilliseconds + "ms]\r\n"); // diagnostic message
				sql.Append("   [Transaction=" + ((bulkCopy.Transaction == null) ? "false" : "true") + "]\r\n"); // diagnostic message
				sql.Append("   [Current thread ID=" + Thread.CurrentThread.ManagedThreadId + "]\r\n"); // diagnostic message
				sql.Append("\r\n");

				var sqlString = sql.ToString();

				lock (sqlEventList)
				{
					sqlEventList.Add(sqlString);
				}

				if (e != null)
				{
					lock (FailedSqlEventList)
					{
						FailedSqlEventList.Add(sqlString);
					}
				}

				if (SqlCommandExecutedEvent != null)
				{
					var commandText = string.Concat(
						statement,
						GetStackTraceAsComment());

					var args = new SqlCommandExecutedEventArgs(commandText, e, timeNow);

					if (SqlCommandExecutedEvent != null)
					{
						SqlCommandExecutedEvent(args);
					}
				}
			}
		}

		string AddCommandParameters(IDbCommand command)
		{
			Argument.NotNull(command, nameof(command)); // Suggested By ReviewBot 
			var parameters = command.Parameters;

			if (parameters == null || parameters.Count == 0)
			{
				return command.CommandText;
			}

			var builder = new StringBuilder();

			foreach (var parameter in parameters.Cast<IDataParameter>())
			{
				var variableDeclaration = string.Format(CultureInfo.InvariantCulture, "DECLARE {0} AS {1} = {2}", // sql command format string
					parameter.ParameterName,
					parameter.GetFormattedDbType(),
					parameter.GetFormattedValue());

				builder.AppendLine(variableDeclaration);
			}

			if (command.CommandType == CommandType.StoredProcedure)
			{
				var commandText = string.Format(CultureInfo.InvariantCulture, "exec {0} {1}", // sql command format string
					command.CommandText,
					string.Join(", ", parameters.Cast<IDataParameter>().Select(p => p.ParameterName)));

				builder.AppendLine(commandText);
			}
			else
			{
				builder.AppendLine(command.CommandText);
			}

			return builder.ToString();
		}

		string GetStackTraceAsComment()
		{
			var builder = new StringBuilder();

			if (QueryStackTraceRecorderCore.InstanceCore.Enabled)
			{
				builder.AppendLine();
				builder.AppendLine("/*");
				builder.AppendLine(QueryStackTraceRecorderCore.StackTraceHeader);
				builder.AppendLine(new System.Diagnostics.StackTrace(4).ToString());
				builder.AppendLine(QueryStackTraceRecorderCore.StackTraceFooter);
				builder.AppendLine("*/");
			}

			return builder.ToString();
		}

		public string SqlEventDescription
		{
			get
			{
				return GetSqlEventDescription("SqlEvents", sqlEventList);
			}
		}

		public string SqlFailedEventDescription
		{
			get
			{
				return GetSqlEventDescription("SqlFailedEvents", FailedSqlEventList);
			}
		}

		string GetSqlEventDescription(string startElement, RingBuffer<string> buffer)
		{
			Argument.NotNull(buffer, nameof(buffer)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(startElement, nameof(startElement)); // Suggested By ReviewBot 

			StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlTextWriter xtw = new CustomXmlWriter(strWriter);

			xtw.WriteStartElement(startElement);
			lock (buffer)
			{
				xtw.WriteElementString("Count", buffer.Count.ToString(CultureInfo.InvariantCulture));

				foreach (string sql in buffer)
				{
					xtw.WriteElementString("Command", sql);
				}
			}
			xtw.WriteEndElement();

			return strWriter.GetStringBuilder().ToString();
		}

		public event Action<SqlCommandExecutedEventArgs> SqlCommandExecutedEvent;

		#region Implementation

		const int MaxSqlEventsWinforms = 20;
		const int MaxSqlEventsWeb = 50;
		const int MaxFailedSqlEvents = 3;

		int MaxSqlEvents
		{
			get { return DbEnv.Instance.IsServingWebBasedApp ? MaxSqlEventsWeb : MaxSqlEventsWinforms; }
		}

		public string LastSqlEvent
		{
			get
			{
				return sqlEventList.Current();
			}
		}

		readonly RingBuffer<string> sqlEventList;
		readonly RingBuffer<string> FailedSqlEventList;

		#endregion

		#endregion

#if DEBUG

		public IEnumerable<string> SqlEventList
		{
			get
			{
				lock (sqlEventList)
				{
					return sqlEventList.ToArray();
				}
			}
		}

#endif
	}
}
