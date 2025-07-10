using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.DevTools.Testing
{
	class ZQueryAnalyzerTest : TestCaseWithFactory
	{
		class ZQueryAnalyzerFormTest : ZQueryAnalyzerForm
		{
			public ZQueryAnalyzerFormTest(string sqlText) : base(sqlText)
			{
			}

			protected override void RunCommandInANewTask(string commandText)
			{
				if (commandText == null || commandText.Trim().Length == 0)
				{
					return;
				}

				callBackManualEvent = new ManualResetEvent(false);

				base.RunCommandInANewTask(commandText);

				callBackManualEvent.WaitOne(TimeSpan.FromSeconds(10));
			}

			protected override void ApplyQueryResult(CommandResult result)
			{
				base.ApplyQueryResult(result);

				callBackManualEvent.Set();
			}

			ManualResetEvent callBackManualEvent { get; set; }
		}

		public void TestErrorMessageShown()
		{
			ErrorReporter.SuppressReportingOfErrors = true;

			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				queryform.RunCommand("cast varchar varbinary select");
				Assert(UnitTestUserNotification.Instance.LastMessage.Text, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Incorrect syntax"));

				UnitTestUserNotification.Instance.ClearMessages();
				queryform.RunCommand("");
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSingleSQLThreadOwnershipProtect()
		{
			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				ErrorReporter.Clear();
				AssertNoExceptionThrown(() => queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbStaff"));
				AssertEquals("No errors should be reported", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestTransactionCommitThreadOwnershipProtect()
		{
			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				ErrorReporter.Clear();
				AssertNoExceptionThrown(() =>
				{
					queryform.RunCommand("begin tran");
					queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbStaff");
					queryform.RunCommand("SELECT TOP 1 * FROM sys.time_zone_info");
					queryform.RunCommand("commit");
				});
				AssertEquals("No errors should be reported", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestTransactionRollBackThreadOwnershipProtect()
		{
			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				ErrorReporter.Clear();
				AssertNoExceptionThrown(() =>
				{
					queryform.RunCommand("begin tran");
					queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbStaff");
					queryform.RunCommand("SELECT TOP 1 * FROM sys.time_zone_info");
					queryform.RunCommand("rollback");
				});
				AssertEquals("No errors should be reported", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestCombinedTransactionCommitThreadOwnershipProtect()
		{
			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				ErrorReporter.Clear();
				AssertNoExceptionThrown(() =>
				{
					queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbBranch");
					queryform.RunCommand("begin tran");
					queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbStaff");
					queryform.RunCommand("SELECT TOP 1 * FROM sys.time_zone_info");
					queryform.RunCommand("commit");
				});
				AssertEquals("No errors should be reported", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestCombinedTransactionRollBackThreadOwnershipProtect()
		{
			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				ErrorReporter.Clear();
				AssertNoExceptionThrown(() =>
				{
					queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbBranch");
					queryform.RunCommand("begin tran");
					queryform.RunCommand("SELECT TOP 1 * FROM dbo.GlbStaff");
					queryform.RunCommand("SELECT TOP 1 * FROM sys.time_zone_info");
					queryform.RunCommand("rollback");
				});
				AssertEquals("No errors should be reported", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestIgnoreSqlCommentsWhenDecidingIsCommandNonQuery()
		{
			using (var queryform = new ZQueryAnalyzerFormTest(""))
			{
				queryform.RunCommand("--update\r\nSELECT TOP 1 * FROM dbo.GlbBranch");
				var resultTextBox = queryform.Controls.Find("ResultsTextBox", true)[0] as KTextBox;
				AssertEquals("1 record(s) returned", resultTextBox.Text);
			}
		}
	}
}
