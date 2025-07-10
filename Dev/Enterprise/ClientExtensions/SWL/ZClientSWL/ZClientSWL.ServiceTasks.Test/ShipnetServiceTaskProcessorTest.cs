using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using Enterprise.Client.SWL.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.SWL.ServiceTasks.Testing
{
	internal sealed class ShipnetServiceTaskProcessorTest : ShipnetTestCase
	{
		public void TestNotifyUpdatesLogger()
		{
			InfoNotification infoNotify = new InfoNotification("Dummy Message");
			testLogger.ClearLog();
			Processor.Notify(infoNotify);
			AssertStartsWith("Logger should contain notify message", infoNotify.Message, logEntriesList[0]);
		}

		public void TestStoppable()
		{
			testLogger.ClearLog();
			SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.NewGuid());
			Processor.Process();
			AssertStartsWith("Notification Group not set", "Shipnet Notification Email Group has not been properly setup; data cannot be exported.", logEntriesList[0]);
			AssertEquals("Only one log written", 1, logEntriesList.Count);
			testLogger.ClearLog();
			Processor.CanContinue = false;
			Processor.Process();
			AssertEquals("No Logs", 0, logEntriesList.Count);
		}

		public void TestLogRecords()
		{
			testLogger.ClearLog();
			LogProcess("hello world");
			AssertStartsWith("LogRecords", "hello world", logEntriesList[0]);
		}

		[TestDate(2006, 3, 23, 21, 32, 34)]
		[TestUtcOffset(12, 0, 0)]
		public void TestProcess()
		{
			SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.NewGuid());
			DeleteAnyExistingShipnetCarriers();
			OrgHeader carrier = ShipnetCarrier;
			Factory.Save();
			testLogger.ClearLog();
			DateTime highWaterMark = new DateTime(2006, 3, 21, 21, 32, 34);
			string tempBackupDirectory = Temp.GetNewTempSubdirectory();
			try
			{
				SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, tempBackupDirectory);
				SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, highWaterMark);
				Processor.Process();
				AssertStartsWith("Notification Group not set", "Shipnet Notification Email Group has not been properly setup; data cannot be exported.", logEntriesList[0]);
				AssertEquals("Only one log written", 1, logEntriesList.Count);
				AssertEquals("High Water Mark not Changed", highWaterMark, SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.Value);
				testLogger.ClearLog();
				SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.DefaultValue);
				Processor.Process();
				AssertStartsWith("Shipnet Carriers not set", "Error: No Shipnet Carriers found", logEntriesList[0]);
				AssertEquals("Only one log written", 1, logEntriesList.Count);
				AssertEquals("High Water Mark not Changed", highWaterMark, SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.Value);
				SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(carrier.CompanyData.PK, ShipnetCarrierSettings);
				Processor = new ShipnetServiceTaskProcessor(Logger);
				testLogger.ClearLog();
				Processor.Process();
				AssertEquals("Some Logs", 2, logEntriesList.Count);
				AssertContains("Has EDI company", "Processing EDI company...", logEntriesList[0]);
				AssertContains("Has current batch interval", "Current Batch Interval UTC: '21-Mar-06 21:32:34' - '23-Mar-06 21:32:34'", logEntriesList[1]);
				AssertEquals("High Water Mark Changed", new DateTime(2006, 3, 23, 21, 32, 34), SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.Value);
			}
			finally
			{
				TempDirectory.DeleteDirectory(tempBackupDirectory);
			}
		}

		#region Implementation
		ShipnetServiceTaskProcessor Processor;
		protected override void SetUp()
		{
			base.SetUp();
			Processor = new ShipnetServiceTaskProcessor(Logger);
		}

		ILogger Logger
		{
			get
			{
				return logger ?? (logger = new LoggerForTest());
			}
		}

		ILogger logger;
		LoggerForTest testLogger
		{
			get
			{
				return ((LoggerForTest)logger);
			}
		}

		List<string> logEntriesList
		{
			get
			{
				return ((List<string>)testLogger.LogEntries);
			}
		}
		#endregion

		void LogProcess(string message)
		{
			try
			{
				logger.Information(message + " process " + System.Diagnostics.Process.GetCurrentProcess().Id);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Information(ex.Message + ": " + message);
			}
		}
	}
}
