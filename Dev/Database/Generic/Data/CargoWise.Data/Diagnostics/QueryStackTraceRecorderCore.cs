using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Data.Diagnostics
{
	public interface IQueryStackTraceRecorder
	{
		System.Data.Common.DbCommand GetCommandWithCallStackTraceAddedIfEnabled(System.Data.Common.DbCommand originalCommand);
		bool Enabled { get; set; }
	}

	[ImmutableObject(true)]
	public class QueryStackTraceRecorderCore : IQueryStackTraceRecorder
	{
		#region Factory Method

		protected internal QueryStackTraceRecorderCore()
		{
		}

		// Explicit static constructor to tell C# compiler NOT to mark type as beforefieldinit
		// This guarantees lazy instantiation of the singleton object (i.e. only when referenced)
		static QueryStackTraceRecorderCore() { }
		protected internal static readonly IQueryStackTraceRecorder InstanceCore = new QueryStackTraceRecorderCore();

		#endregion

		#region Enabled

		bool IQueryStackTraceRecorder.Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}
		bool enabled;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name used to identify SqlClient types")]
		System.Data.Common.DbCommand IQueryStackTraceRecorder.GetCommandWithCallStackTraceAddedIfEnabled(System.Data.Common.DbCommand originalCommand)
		{
			Argument.NotNull(originalCommand, nameof(originalCommand));
			if (!enabled || !AreAllParametersInput(originalCommand.Parameters))
			{
				return originalCommand;
			}

			System.Data.Common.DbCommand newCommand = null;
			if (originalCommand is System.Data.SqlClient.SqlCommand sqlCommand)
			{
				newCommand = sqlCommand.Clone();
			}
#if NET
			else if (originalCommand is Microsoft.Data.SqlClient.SqlCommand sqlCommandMS)
			{
				newCommand = sqlCommandMS.Clone();
			}
#endif
			else
			{
				throw new NotSupportedException($"Unsupported command type: {originalCommand.GetType()}");
			}

			var callStack = new StackTrace(1).ToString();
			if (originalCommand.Parameters.Count > 0 ||
				originalCommand.CommandType == CommandType.StoredProcedure)
			{
				InsertStackTraceAsParameter(newCommand, callStack);
			}
			else
			{
				// Can't add a parameter to a text command without parameters since the.NET SQL framework will use "exec sp_executesql" to run it.
				// This breaks any command that can't be run that way, such as "EXECUTE AS USER ... WITH COOKIE" in ExecuteAsReader
				InsertStackTraceAsComment(newCommand, callStack);
			}
			return newCommand;
		}

		void InsertStackTraceAsComment(System.Data.Common.DbCommand command, string callStack)
		{
			command.CommandText = string.Join(Environment.NewLine,
				command.CommandText,
				"/*",
				StackTraceHeader,
				callStack,
				StackTraceFooter,
				"*/" + Environment.NewLine
				);
		}

		static void InsertStackTraceAsParameter(System.Data.Common.DbCommand command, string stackTrace)
		{
			if (command.CommandType == CommandType.StoredProcedure)
			{
				ConvertStoredProcCommandToText(command);
			}

			var parameterValue = string.Join(Environment.NewLine, StackTraceHeader, stackTrace, StackTraceFooter);
			var newParameter = command.CreateParameter();
			var parameterWrapper = new SqlParameterWrapper(newParameter);
			parameterWrapper.ParameterName = CallStackParameterName;
			parameterWrapper.SqlDbType = SqlDbType.NVarChar;
			parameterWrapper.Size = (int)SqlMetaData.Max;
			parameterWrapper.Value = parameterValue;
			command.Parameters.Add(newParameter);
		}

		static void ConvertStoredProcCommandToText(System.Data.Common.DbCommand command)
		{
			var builder = new StringBuilder();
			builder.Append("exec "); // part of sql command
			builder.Append(command.CommandText);

			for (var i = 0; i < command.Parameters.Count; i++)
			{
				var name = command.Parameters[i].ParameterName;
				builder.Append(i == 0 ? " " : ", ");
				builder.Append(name);
				builder.Append('=');
				builder.Append(name);
			}

			command.CommandType = CommandType.Text;
			command.CommandText = builder.ToString();
		}

		bool AreAllParametersInput(System.Data.Common.DbParameterCollection parameters)
		{
			return parameters.Cast<System.Data.Common.DbParameter>().All(param => param.Direction == ParameterDirection.Input);
		}

		internal const string CallStackParameterName = "@__callStack__";
		internal const string StackTraceHeader = "================     STACK TRACE      ================"; // diagnostic message
		internal const string StackTraceFooter = "================  END OF STACK TRACE  ================"; // diagnostic message
	}
}
