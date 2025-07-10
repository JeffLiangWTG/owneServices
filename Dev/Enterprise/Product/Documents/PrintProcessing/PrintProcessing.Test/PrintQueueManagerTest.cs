using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.RemotePrinting.Engine;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class PrintQueueManagerTest : TestCaseWithFactory
	{
		class PrintQueueManagerForTesting : PrintQueueManager
		{
			protected internal override ReadOnlyCollection<PrinterInfo> InstalledPrinters
			{
				get
				{
					List<PrinterInfo> result = new List<PrinterInfo>(base.InstalledPrinters);
					if (RemoveTestPrinterFromList)
					{
						int nullPrintQueueIndex = result.FindIndex(printerInfo => printerInfo.Name.Equals(TestAssistant.PrintQueueNameForTesting, StringComparison.Ordinal));
						if (nullPrintQueueIndex >= 0)
						{
							result.RemoveAt(nullPrintQueueIndex);
						}
					}

					return result.AsReadOnly();
				}
			}

			public bool RemoveTestPrinterFromList;
		}

		class PrintQueueManagerWithRPCException : PrintQueueManager
		{
			protected internal override ReadOnlyCollection<PrinterInfo> InstalledPrinters
			{
				get
				{
					throw new Win32Exception(ErrorHandler.RPC.RPC_S_SERVER_UNAVAILABLE);
				}
			}
		}

		class PrintQueueManagerWithGeneralException : PrintQueueManager
		{
			protected internal override ReadOnlyCollection<PrinterInfo> InstalledPrinters
			{
				get { throw new Exception("hello"); }
			}
		}

		string AlternateCase(string text)
		{
			StringBuilder result = new StringBuilder(text.Length);
			while (result.Length < text.Length)
			{
				char character = (result.Length % 2) == 1 ? Char.ToUpper(text[result.Length]) : Char.ToLower(text[result.Length]);
				result.Append(character);
			}
			return result.ToString();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			InstalledPrintersListForTesting = SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting);
			Manager = new PrintQueueManagerForTesting();
		}

		protected override void TearDown()
		{
			InstalledPrintersListForTesting.Dispose();
			base.TearDown();
		}

		static bool PrinterForTestExists()
		{
			return SafeInstalledPrinters.InstalledLocalPrinterNames.Contains(TestAssistant.PrintQueueNameForTesting);
		}

		public void TestLogWhenNoRecepientForNotificationEmail()
		{
			const string updateToNoController = "UPDATE dbo.GlbStaff SET GS_IsController = 1, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_IsController = 0";
			TestConnection.ExecuteNonQuery(updateToNoController);

			var expectedMseeage = string.Format("Print Queues not updated for {0}, and failed to send a notification e-mail with error message: Email (Subject: '{1} Batch Server Warning: Print Queues not updated for {0}') must have at least one recipient, CC or BCC", System.Environment.MachineName, Core.Constants.ProductName);
			PrintQueueManagerWithRPCException manager = new PrintQueueManagerWithRPCException();

			AssertExceptionThrown<HostedServiceException>("HostedServiceException should be throw when cannot find Recepient for notification email", expectedMseeage, () => manager.MaintainStmPrintQueue());
		}

		public void TestMaintainStmPrintQueueReAddingPrinterQueue()
		{
			Assert("Precondition: the TestPrinter should be listed on this machine", PrinterForTestExists());
			Manager.MaintainStmPrintQueue();
			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName);
			var query = new ZDBOnlyQuery(typeof(StmPrintQueue));
			query.AddSubQuery(serverSubQuery, JoinCondition.And);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueName, TestAssistant.PrintQueueNameForTesting);
			var queue = Factory.LoadTop1<StmPrintQueue>(query);
			AssertNotNull("Precondition: the TestPrinter queue for this machine should exist in the DB", queue);

			queue.SQ_QueueDeleted = ZDateTime.Now;

			Factory.Save();

			bool updated = Manager.MaintainStmPrintQueue();
			Assert("The print queue list should have been updated", updated);

			var queueAfter = Factory.Load<StmPrintQueue>(queue.PK);
			AssertEquals("The print queue for the Test printer should be updated with SQ_QueueDeleted blank", ZDateTime.Empty, queueAfter.SQ_QueueDeleted);
		}

		public void TestMaintainStmPrintQueueRemovingPrinterQueue()
		{
			Assert("Precondition: the TestPrinter should be listed on this machine", PrinterForTestExists());
			Manager.MaintainStmPrintQueue();
			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName);
			var query = new ZDBOnlyQuery(typeof(StmPrintQueue));
			query.AddSubQuery(serverSubQuery, JoinCondition.And);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueName, TestAssistant.PrintQueueNameForTesting);
			var queue = Factory.LoadTop1<StmPrintQueue>(query);
			AssertNotNull("Precondition: the Test Printer queue for this machine should exist in the DB", queue);
			AssertEquals("Queue deleted field empty?", true, queue.SQ_QueueDeleted.IsEmpty);

			Manager.RemoveTestPrinterFromList = true;

			var updated = Manager.MaintainStmPrintQueue();
			Assert("The print queue list should have been updated", updated);

			queue = Factory.LoadTop1<StmPrintQueue>(query);
			Assert("Queue deleted field should have been updated", !queue.SQ_QueueDeleted.IsEmpty);
		}

		public void TestMaintainStmPrintQueueNoChanges()
		{
			AssertEquals("Precondition: The test printer should be listed on this machine.", true, PrinterForTestExists());

			Manager.MaintainStmPrintQueue();
			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName);
			var query = new ZDBOnlyQuery(typeof(StmPrintQueue));
			query.AddSubQuery(serverSubQuery, JoinCondition.And);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueName, TestAssistant.PrintQueueNameForTesting);
			var queue = Factory.LoadTop1<StmPrintQueue>(query);
			queue.SQ_QueueName = AlternateCase(queue.SQ_QueueName);
			Factory.Save();

			var updated = Manager.MaintainStmPrintQueue();
			AssertEquals("No changes, the Print Queues should not be updated.", false, updated);
		}

		public void TestMaintainStmPrintQueueDeleteWithDefValues()
		{
			StmPrintQueue defQueue = Factory.New<StmPrintQueue>();
			defQueue.SQ_ServerName = System.Environment.MachineName;
			defQueue.SQ_QueueName = "Test";
			defQueue.SQ_DisplayName = defQueue.GetDefaultDisplayName();
			defQueue.SQ_QueueDeleted = new ZDateTime(2008, 2, 15, 14, 33, 0);

			StmScheduleTaskRecipient task = Factory.NewWithValidTestData<StmScheduleTaskRecipient>();
			task.S6_SQ = defQueue.PK;

			Factory.Save();

			bool updated = Manager.MaintainStmPrintQueue();
			Assert("The print queue list should have been updated", updated);

			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, defQueue.SQ_ServerName);
			var query = new ZDBOnlyQuery(typeof(StmPrintQueue));
			query.AddSubQuery(serverSubQuery, JoinCondition.And);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueName, defQueue.SQ_QueueName);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueDeleted, defQueue.SQ_QueueDeleted);
			StmPrintQueue loadedQueue = Factory.LoadTop1<StmPrintQueue>(query);
			AssertEquals("Queue deleted field should NOT have been updated", defQueue.SQ_QueueDeleted, loadedQueue.SQ_QueueDeleted);

			StmScheduleTaskRecipient deletedRecipient = Factory.Load<StmScheduleTaskRecipient>(task.PK);
			AssertEquals("StmScheduleTaskRecipient reference not affected", loadedQueue.PK, deletedRecipient.S6_SQ);
		}

		public void TestMaintainStmPrintQueueNotUpdatedWhenItMarkedAsDeleted()
		{
			Manager.MaintainStmPrintQueue();
			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "Test";
			queue.SQ_DisplayName = queue.GetDefaultDisplayName();
			queue.SQ_AllowPrinting = false;
			queue.SQ_QueueDeleted = ZDateTime.Now;

			Factory.Save();

			bool updated = Manager.MaintainStmPrintQueue();
			AssertEquals("The print queue list should not be updated", false, updated);

			AssertNotNull("Queue exists in the DB", Factory.Load(typeof(StmPrintQueue), queue.PK));
		}

		public void TestMaintainStmPrintQueue_WithRPCErrorSendsNotification()
		{
			AssertEquals("Precondition: no emails to be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			TestAssistant.SetupEmailAddressForAllStaff(Factory); // so we don't get warnings about not having postmaster email address
			Factory.Save();
			UnattendedUserNotification.Instance.ClearShownErrorKeys();
			PrintQueueManagerWithRPCException manager = new PrintQueueManagerWithRPCException();

			manager.MaintainStmPrintQueue();

			AssertEquals("Should have shown an unattended user notification via email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("that the 'Print Spooler' windows service is running", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertNull("Shouldn't have sent any silent exceptions", ErrorReporter.LastExceptionReported);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestMaintainStmPrintQueue_WithRegularErrorSendsSilentException()
		{
			PrintQueueManagerWithGeneralException manager = new PrintQueueManagerWithGeneralException();
			manager.MaintainStmPrintQueue();

			Assert("Should have thrown a silent exception", ErrorReporter.LastExceptionReported.Message.Contains("hello"));
			ErrorReporter.Clear();
		}

		PrintQueueManagerForTesting Manager;
		IDisposable InstalledPrintersListForTesting;
	}
}
