using System;
using System.Text;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Common
{
	sealed class DbTaskManagerTest : TestCase
	{
		sealed class ManagerErrorReporterTest : TestCase
		{
			public void TestClear()
			{
				// Arrange
				var taskManager = new DbTaskManager();

				var sb = new StringBuilder();
				taskManager.OnShowInfoMessage += message => sb.Append(message);

				IErrorReporter errorReporter = taskManager;

				// Act
				AssertNoExceptionThrown(() => errorReporter.Clear());

				// Assert
				AssertNullOrEmpty(sb.ToString());
			}

			public void TestReport()
			{
				// Arrange
				var taskManager = new DbTaskManager();

				var sb = new StringBuilder();
				taskManager.OnShowInfoMessage += message => sb.Append(message);

				IErrorReporter errorReporter = taskManager;

				// Act
				errorReporter.Report("whatever key", "whatever message", new InvalidOperationException("whatever exception"));

				// Assert
				var info = sb.ToString();
				AssertContains("whatever key", info);
				AssertContains("whatever message", info);
				AssertContains("whatever exception", info);
			}

			public void TestReportDeveloperExceptionOrHandleSilently()
			{
				// Arrange
				var taskManager = new DbTaskManager();

				var sb = new StringBuilder();
				taskManager.OnShowInfoMessage += message => sb.Append(message);

				IErrorReporter errorReporter = taskManager;

				// Act
				errorReporter.ReportDeveloperExceptionOrHandleSilently("whatever key", "whatever message", new InvalidOperationException("whatever exception"));

				// Assert
				var info = sb.ToString();
				AssertContains("whatever key", info);
				AssertContains("whatever message", info);
				AssertContains("whatever exception", info);
			}
		}
	}
}
