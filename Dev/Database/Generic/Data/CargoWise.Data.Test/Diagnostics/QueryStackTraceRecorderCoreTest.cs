using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Data.Utils;
using NUnit.Framework;

namespace CargoWise.Data.Diagnostics.Testing
{
	sealed class QueryStackTraceRecorderCoreTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLongStackTraceIsNotTruncated()
		{
			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;

			using var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			try
			{
				Method1();
			}
			catch (OperationCanceledException)
			{
			}

			void Method1()
			{
				CheckStackTraceAndExecuteCommand();
				Method2();
			}

			void Method2()
			{
				CheckStackTraceAndExecuteCommand();
				Method1();
			}

			[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
			void CheckStackTraceAndExecuteCommand()
			{
				cancellationToken.ThrowIfCancellationRequested();

				var stackTrace = new StackTrace(1).ToString();
				if (stackTrace.Length > 9000)
				{
					var serverVersion = Db.Connection.ExecuteScalar<string>("SELECT @@version");
					var commandText = $@"SELECT @@version WHERE 'A'!=@value";

					var connection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
					var originalCmd = connection.CreateCommand();
					originalCmd.CommandText = commandText;
					_ = originalCmd.AddParameterWithValue("@value", stackTrace);

					var command = stackRecorder.GetCommandWithCallStackTraceAddedIfEnabled(originalCmd);
					command.Connection = ((IDbConnectionInternals)Db.Connection).ADOConnection;

					AssertEquals("Parameters", 2, command.Parameters.Count);
					var callStackParameter = (string)command.Parameters[1].Value;

					AssertGreaterThan(callStackParameter.IndexOf(QueryStackTraceRecorderCore.StackTraceHeader), -1);
					AssertGreaterThan(callStackParameter.IndexOf(QueryStackTraceRecorderCore.StackTraceFooter), -1);

					var result = (string)command.ExecuteScalar();
					AssertEquals(serverVersion, result);

					cancellationTokenSource.Cancel();
				}
			}
		}

		public void TestSingletonInstance()
		{
			IQueryStackTraceRecorder recorder1 = QueryStackTraceRecorderCore.InstanceCore;
			IQueryStackTraceRecorder recorder2 = QueryStackTraceRecorderCore.InstanceCore;
			AssertEquals("Singleton object reference is the same", true, Object.ReferenceEquals(recorder1, recorder2));
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestEnabled_WhenFalse_StackTraceIsNotAdded()
		{
			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			const string originalCommandText = "SELECT top 1 * from dbo.OrgHeader";
			var originalCmd = new SqlCommand(originalCommandText);
			AssertEquals(false, stackRecorder.Enabled);
			AssertCommandNotChanged(stackRecorder, originalCmd);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestCommandWithNoParameters_StackTraceIsAddedAsComment()
		{
			const string originalCommandText = "SELECT top 1 * from dbo.OrgHeader";
			var originalCmd = new SqlCommand(originalCommandText);

			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;

			var testCmd = stackRecorder.GetCommandWithCallStackTraceAddedIfEnabled(originalCmd);

			CombineAssertions(() =>
			{
				AssertEquals("ReferenceEquals cmd", false, Object.ReferenceEquals(originalCmd, testCmd));
				AssertContains("Command Text has Stack Trace", QueryStackTraceRecorderCore.StackTraceHeader, testCmd.CommandText);
				AssertEquals(
					"Stack Trace is wrapped in a comment", true,
					testCmd.CommandText.Substring(originalCommandText.Length, 4) == "\r\n/*" && testCmd.CommandText.EndsWith("*/\r\n")
					);
				int firstStackTraceIndex = testCmd.CommandText.IndexOf(QueryStackTraceRecorderCore.StackTraceHeader);
				AssertEquals("Only 1 Stack Trace", firstStackTraceIndex, testCmd.CommandText.LastIndexOf(QueryStackTraceRecorderCore.StackTraceHeader));
				AssertEquals("Parameters", 0, testCmd.Parameters.Count);
				AssertEquals("Original Command is not modified", originalCommandText, originalCmd.CommandText);
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestCommandWithInputParameter_StackTraceIsAddedAsParameter()
		{
			const string originalCommandText = "SELECT top 1 * from dbo.OrgHeader WHERE OH_Code = @Code";
			var originalCmd = new SqlCommand(originalCommandText);
			originalCmd.Parameters.AddWithValue("@Code", "XYZZY");

			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;

			var testCmd = stackRecorder.GetCommandWithCallStackTraceAddedIfEnabled(originalCmd);

			var expectedStackTrace = new StackTrace().ToString();
			var expectedParameterValue = string.Join(Environment.NewLine,
				QueryStackTraceRecorderCore.StackTraceHeader,
				expectedStackTrace,
				QueryStackTraceRecorderCore.StackTraceFooter);

			CombineAssertions(() =>
			{
				AssertEquals("ReferenceEquals cmd", false, Object.ReferenceEquals(originalCmd, testCmd));
				AssertEquals("Command Text", originalCommandText, testCmd.CommandText);
				AssertEquals("Parameters", 2, testCmd.Parameters.Count);

				AssertEquals("Old parameter name not changed", "@Code", testCmd.Parameters[0].ParameterName);
				AssertEquals("Old parameter value not changed", "XYZZY", (string)testCmd.Parameters[0].Value);

				AssertEquals("New Parameter value", expectedParameterValue, (string)testCmd.Parameters[1].Value);
				AssertEquals("New Parameter name", QueryStackTraceRecorderCore.CallStackParameterName, testCmd.Parameters[1].ParameterName);

				AssertEquals("Original Command text is not modified", originalCommandText, originalCmd.CommandText);
				AssertEquals("Original Command parameter count is not modified", 1, originalCmd.Parameters.Count);
			});
		}

		public void TestCommandWithOutputParameter_StackTraceNotAdded()
		{
			var cmd = GetCommand("SELECT @OutParam = @Code");
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.Add("@Code", SqlDbType.VarChar);
			var returnParam = cmd.Parameters.Add("@OutParam", SqlDbType.VarChar);
			returnParam.Direction = ParameterDirection.Output;

			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;
			AssertCommandNotChanged(stackRecorder, cmd);
		}

		public void TestStoredProcedureCommandWithNoParameters_IsConvertedToText_StackTraceIsAddedAsParameter()
		{
			var originalCmd = CreateStoredProcedureCommand("testproc", Array.Empty<SqlParameter>());

			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;

			var testCmd = stackRecorder.GetCommandWithCallStackTraceAddedIfEnabled(originalCmd);

			var expectedStackTrace = new StackTrace().ToString();
			var expectedParameterValue = string.Join(Environment.NewLine,
				QueryStackTraceRecorderCore.StackTraceHeader,
				expectedStackTrace,
				QueryStackTraceRecorderCore.StackTraceFooter);
			AssertEquals("CommandText", "exec testproc", testCmd.CommandText);
			AssertEquals("Parameters.Count", 1, testCmd.Parameters.Count);
			AssertEquals("Parameter value", expectedParameterValue, testCmd.Parameters[0].Value);
			AssertEquals("Parameter name", QueryStackTraceRecorderCore.CallStackParameterName, testCmd.Parameters[0].ParameterName);
		}

		public void TestStoredProcedureCommandWithOnlyInputParameters_IsConvertedToText_StackTraceIsAddedAsParameter()
		{
			var parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@Code", SqlDbType.VarChar);
			parameters[0].Value = "blabla";
			parameters[1] = new SqlParameter("@Valid", SqlDbType.Char);
			parameters[1].Value = "Y";
			var originalCmd = CreateStoredProcedureCommand("testproc", parameters);

			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;

			var testCmd = stackRecorder.GetCommandWithCallStackTraceAddedIfEnabled(originalCmd);
			var expectedStackTrace = new StackTrace().ToString();
			var expectedValue = string.Join(Environment.NewLine,
				QueryStackTraceRecorderCore.StackTraceHeader,
				expectedStackTrace,
				QueryStackTraceRecorderCore.StackTraceFooter);
			AssertEquals("CommandText", "exec testproc @Code=@Code, @Valid=@Valid", testCmd.CommandText);
			AssertEquals("Parameters.Count", 3, testCmd.Parameters.Count);
			AssertEquals("Parameter value", expectedValue, testCmd.Parameters[2].Value);
			AssertEquals("Parameter name", QueryStackTraceRecorderCore.CallStackParameterName, testCmd.Parameters[2].ParameterName);
		}

		public void TestStoredProcedureCommandWithReturnValueParameter_StackTraceNotAdded()
		{
			var cmd = GetCommand("testproc");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("@Code", SqlDbType.VarChar);
			var returnParam = cmd.Parameters.Add("@Result", SqlDbType.VarChar);
			returnParam.Direction = ParameterDirection.ReturnValue;

			IQueryStackTraceRecorder stackRecorder = new QueryStackTraceRecorderCore();
			stackRecorder.Enabled = true;
			AssertCommandNotChanged(stackRecorder, cmd);
		}

		void AssertCommandNotChanged(IQueryStackTraceRecorder stackRecorder, SqlCommand originalCmd)
		{
			var originalText = originalCmd.CommandText;
			var originalType = originalCmd.CommandType;
			var originalParameterCount = originalCmd.Parameters.Count;
			var testCmd = stackRecorder.GetCommandWithCallStackTraceAddedIfEnabled(originalCmd);

			AssertEquals("Text", originalText, testCmd.CommandText);
			AssertEquals("SqlCommand object reference is the same", true, Object.ReferenceEquals(originalCmd, testCmd));
			AssertEquals("Command Type", originalType, testCmd.CommandType);
			AssertEquals("Parameter count", originalParameterCount, testCmd.Parameters.Count);
		}

		SqlCommand CreateStoredProcedureCommand(string sql, SqlParameter[] parameters)
		{
			var result = GetCommand(sql);
			result.CommandType = CommandType.StoredProcedure;
			result.Parameters.AddRange(parameters);
			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		SqlCommand GetCommand(string sqlText)
		{
			return new SqlCommand(sqlText); // This is done for Testing
		}
	}
}
