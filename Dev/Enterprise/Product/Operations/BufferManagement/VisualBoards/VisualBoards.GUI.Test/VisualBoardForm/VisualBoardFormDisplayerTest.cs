using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Billing.Business.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.VisualBoards.GUI.Test
{
	class VisualBoardFormDisplayerTest : NonTransactionedTestCase
	{
		public void TestOpenVisualBoard_ShouldReportUsage()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			var newBuffer1 = BMSTestHelper.CreateBuffer(config.System, Name = "otherBuffer1");
			var newBuffer2 = BMSTestHelper.CreateBuffer(config.System, Name = "otherBuffer2");
			BMSTestHelper.CreateBoardSection(config.Buffer, board);
			BMSTestHelper.CreateBoardSection(newBuffer1, board);
			BMSTestHelper.CreateBoardSection(newBuffer2, board);

			var otherBucket = BMSTestHelper.CreateBucket(config.System, Name = "otherBucket");
			BMSTestHelper.CreateBoardSection(config.Bucket, board);
			BMSTestHelper.CreateBoardSection(otherBucket, board);

			BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessTasks, board);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
			}
			Application.DoEvents();

			var messages = new UsageCollectorTestHelper(Factory).LoadUsageMessages();
			AssertEquals("Expected 1 message to be present, but was: " + messages.Length, 1, messages.Length);

			var message = messages.First();
			var messageText = message.EM_MessageText;
			var currentBranch = Env.CurrentBranch.Code;

			AssertContains("Message must contain FeatureCode: VBO", "\"FeatureCode\": \"VBO\"", messageText);
			AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", messageText);
			AssertContains("Message must contain BranchCode: " + currentBranch, currentBranch, message.Branch.Code);
			AssertContains("Message must contain PK:" + board.PK, $"\"PK\": \"{board.PK}\"", messageText);
			AssertContains("Message must contain Name:" + board.MB_Name, $"\"Name\": \"{board.MB_Name}\"", messageText);
			AssertContains("Message must contain CMP: 5", "\"CMP\": \"5\"", messageText);
			AssertContains("Message must contain MOD: 2", "\"MOD\": \"2\"", messageText);
			AssertContains("Message must contain BUF: 3", "\"BUF\": \"3\"", messageText);
			AssertContains("Message must contain BUC: 2", "\"BUC\": \"2\"", messageText);
		}

		public void TestOpenVisualBoard_WhenReportUsageException_ShouldOpenBoardAndReport()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			Factory.Save();

			var mockIPAVEUsageCollector = new Mock<IPAVEUsageCollector>();
			var exception = new Exception("On no!");
			mockIPAVEUsageCollector
				.Setup(m => m.ReportVisualBoardOpen(It.IsAny<IDictionary<string, string>>()))
				.Throws(() => exception);

			ObjectFactory.Substitute(mockIPAVEUsageCollector.Object);

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
			}
			Application.DoEvents();

			var lastException = ErrorReporter.LastExceptionReported;

			AssertEquals(exception, lastException);
			ErrorReporter.Clear();
		}

		public void TestOpenVisualBoardTriggersLicenceUsage()
		{
			VisualBoardLicensedComponent.ResetBMLicencing_ForTest();
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var query = new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.BufferManagement.Name);

				var activityRecord = Factory.LoadTop1<StmActivityLog>(query);
				AssertNotNull(activityRecord);
			}
		}

		public void TestShouldBeAbleToOpenAgain_AfterClosing()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertNotNull(form);
			}
			Application.DoEvents();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertNotNull(form);
			}
		}
	}
}
