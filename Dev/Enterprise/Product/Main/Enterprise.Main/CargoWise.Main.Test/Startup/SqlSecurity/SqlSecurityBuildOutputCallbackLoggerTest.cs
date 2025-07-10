using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Main.Startup.SqlSecurity.Testing
{
	sealed class SqlSecurityBuildOutputCallbackLoggerTest : TestCase
	{
		public void TestCallbackIsCalledWithCorrectLogMessage()
		{
			// Arrange
			var messagePassedToCallback = string.Empty;
			var callback = new Action<LogType, string, Exception>((logType, message, ex) =>
			{
				messagePassedToCallback = message;
			});

			var logger = new SqlSecurityBuildOutputCallbackLogger(callback);

			// Act
			logger.Log(LogType.Debug, "Some message");

			// Assert
			AssertEquals("Callback should be called with message equal to 'Some message'", "Some message", messagePassedToCallback);
		}

		public void TestCallbackIsCalledWithCorrectLogType()
		{
			// Arrange
			var logTypePassedToCallBack = LogType.Debug;

			var callback = new Action<LogType, string, Exception>((logType, message, ex) =>
			{
				logTypePassedToCallBack = logType;
			});

			var logger = new SqlSecurityBuildOutputCallbackLogger(callback);

			// Act
			logger.Log(LogType.Error, "Some Error message");
			var logTypePassedToCallBackSaved1 = logTypePassedToCallBack;
			logger.Log(LogType.Warning, "Some Warning message");
			var logTypePassedToCallBackSaved2 = logTypePassedToCallBack;
			logger.Log(LogType.Information, "Some Information message");
			var logTypePassedToCallBackSaved3 = logTypePassedToCallBack;
			logger.Log(LogType.Debug, "Some Debug message");
			var logTypePassedToCallBackSaved4 = logTypePassedToCallBack;

			// Assert
			AssertEquals("Callback should be called with log type 'Error' first time", LogType.Error, logTypePassedToCallBackSaved1);
			AssertEquals("Callback should be called with log type 'Warning' second time", LogType.Warning, logTypePassedToCallBackSaved2);
			AssertEquals("Callback should be called with log type 'Information' third time", LogType.Information, logTypePassedToCallBackSaved3);
			AssertEquals("Callback should be called with log type 'Debug' forth time", LogType.Debug, logTypePassedToCallBackSaved4);
		}

		public void TestCallbackIsCalledWithCorrectException()
		{
			// Arrange
			var exceptionPassedtoCallback = default(Exception);
			var callback = new Action<LogType, string, Exception>((logType, message, ex) =>
			{
				exceptionPassedtoCallback = ex;
			});

			var logger = new SqlSecurityBuildOutputCallbackLogger(callback);

			var exception = new Exception("Some exception");

			// Act
			logger.Log(LogType.Debug, "Some message", exception);

			// Assert
			AssertEquals("Callback should be called with correct exception", exception, exceptionPassedtoCallback);
		}
	}
}
