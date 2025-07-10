using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.MFI.CaroTrans;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.MFI.ServiceTasks.Testing
{
	[TestedType(typeof(CoroTransExportServiceTask))]
	class CoroTransExportServiceTaskTest : ServiceTaskTestCase<CoroTransExportServiceTask>
	{
		public void TestListeners()
		{
			var listeners = ServiceTask.GetListeners(Env.TempPath);
			AssertNotNull("Listeners should not be null", listeners);
			AssertEquals("Number of listeners", 4, listeners.Length);
			AssertEquals("Type of listeners[0]", typeof(CaroTransConsolArrivalListener), listeners[0].GetType());
			AssertEquals("Type of listeners[1]", typeof(CaroTransConsolAvailableListener), listeners[1].GetType());
			AssertEquals("Type of listeners[2]", typeof(CaroTransShipmentCustomsListener), listeners[2].GetType());
			AssertEquals("Type of listeners[3]", typeof(CaroTransShipmentDeliveryListener), listeners[3].GetType());
		}

		[TestDate(2005, 11, 28, 12, 23, 10)]
		public void TestRunTask()
		{
			MFIDataRegistry.Instance.CaroTransHighWaterMark = ZDateTime.Now.AddMinutes(-1);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			string finalFile = "";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("EmailsSavedToBeSent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var tempFile = Path.Combine(Env.TempPath, Constants.InternalDefaultCaroTransExportFileWithExtensionTest);
			SetupExportedFile(tempFile);
			AssertEquals("ExportedFile should exist", true, File.Exists(tempFile));
			ServiceTask.Notify.Clear();
			RunTaskSchedule(ServiceTask);
			AssertEquals("ExportedFile should be deleted", false, File.Exists(tempFile));
			finalFile = "ST20051129122310." + Constants.CargoTransDefaultFileExtension;
			AssertEmail(finalFile, true);
		}

		[TestDate(2021, 09, 23, 12, 23, 10)]
		public void TestTimeSpanOverOneDay_QueryRangeWithinOneDay()
		{
			// Arrange
			MFIDataRegistry.Instance.CaroTransHighWaterMark = ZDateTime.Now.AddMinutes(-1);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2);
			var expectedHighWaterMark = MFIDataRegistry.Instance.CaroTransHighWaterMark.AddDays(1).ToDateTime();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ServiceTask.Notify.Clear();
			var tempFile = Path.Combine(Env.TempPath, Constants.InternalDefaultCaroTransExportFileWithExtensionTest);
			SetupExportedFile(tempFile);
			// Act
			RunTaskSchedule(ServiceTask);
			// Assert
			AssertEquals(expectedHighWaterMark, MFIDataRegistry.Instance.CaroTransHighWaterMark.ToDateTime());
		}

		[TestDate(2021, 09, 23, 12, 23, 10)]
		public void TestTimeSpanWithinSameDay_QueryRangeCutOffOneHourBeforeUtcNow()
		{
			// Arrange
			MFIDataRegistry.Instance.CaroTransHighWaterMark = ZDateTime.UtcNow.AddMinutes(-1);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(0.5);
			var expectedHighWaterMark = TestDateAttribute.Date.AddHours(-1);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ServiceTask.Notify.Clear();
			var tempFile = Path.Combine(Env.TempPath, Constants.InternalDefaultCaroTransExportFileWithExtensionTest);
			SetupExportedFile(tempFile);
			// Act
			RunTaskSchedule(ServiceTask);
			// Assert
			AssertEquals(expectedHighWaterMark, MFIDataRegistry.Instance.CaroTransHighWaterMark.ToDateTime());
		}

		public void TestGetDistinctExportedLines()
		{
			var tempFile = Path.Combine(Env.TempPath, Constants.InternalDefaultCaroTransExportFileWithExtensionTest);
			Assert("ExportedFile should not exist", !File.Exists(tempFile));
			using (var fs = File.Create(tempFile))
			{
			}

			try
			{
				using (StreamWriter writer = new StreamWriter(tempFile))
				{
					writer.WriteLine("1,1,1,1,1,1,1,1,1");
					writer.WriteLine("1,1,1,1,1,1,2,2,2");
					writer.WriteLine("2,2,2,2,2,2,2,2,2");
					writer.WriteLine("1,1,1,1,1,1,3,3,3");
					writer.WriteLine("1,1,1,1,1,3,3,3,3");
					writer.WriteLine("1,1,1,1,3,1,3,3,3");
					writer.WriteLine("1,1,1,3,1,1,3,3,3");
					writer.WriteLine("1,1,3,1,1,1,3,3,3");
					writer.WriteLine("1,3,1,1,1,1,3,3,3");
					writer.WriteLine("3,1,1,1,1,1,3,3,3");
					writer.Flush();
				}

				var lines = ServiceTask.GetDistinctExportedLines(new FileInfo(tempFile));
				AssertEquals("Lines count", 8, lines.Length);
				AssertEquals("Lines[0]", "2,2,2,2,2,2,2,2,2", lines[0]);
				AssertEquals("Lines[1]", "1,1,1,1,1,1,3,3,3", lines[1]);
				AssertEquals("Lines[2]", "1,1,1,1,1,3,3,3,3", lines[2]);
				AssertEquals("Lines[3]", "1,1,1,1,3,1,3,3,3", lines[3]);
				AssertEquals("Lines[4]", "1,1,1,3,1,1,3,3,3", lines[4]);
				AssertEquals("Lines[5]", "1,1,3,1,1,1,3,3,3", lines[5]);
				AssertEquals("Lines[6]", "1,3,1,1,1,1,3,3,3", lines[6]);
				AssertEquals("Lines[7]", "3,1,1,1,1,1,3,3,3", lines[7]);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		void AssertExportedData(AttachmentDef attachemntDef)
		{
			using (var stream = new MemoryStream(attachemntDef.Data))
			using (var reader = new StreamReader(stream))
			{
				string line = reader.ReadLine();
				AssertEquals("Line", "1,2,3,4,5,6,7,8,9", line);
				line = reader.ReadLine();
				AssertEquals("Line", "99,1", line);
				line = reader.ReadLine();
				AssertNull("Line", line);
			}
		}

		void AssertEmail(string finalFile, bool emailSent)
		{
			if (emailSent)
			{
				AssertEquals("EmailsSavedToBeSent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Recipients count", 1, sentEmail.Recipients.Count);
				AssertEquals("Recipient", "CaroTrans@test.com", sentEmail.Recipients[0]);
				AssertEquals("Subject", "Shipments Data", sentEmail.Subject);
				AssertEquals("Attachments count", 1, sentEmail.Attachments.Count);
				var attachment = sentEmail.Attachments[0];
				AssertEquals("Attachment file name", finalFile, attachment.DisplayName);
				AssertExportedData(attachment);
			}
			else
			{
				AssertEquals("EmailsSavedToBeSent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Error message should be shown", 1, ServiceTask.Notify.Events.Length);
				string errorMessage = "Error: 'CaroTrans Track Email Address' registry item has invalid value ''. Please fix it to allow notification.\n";
				errorMessage += "The file '" + finalFile + "' contains the not-sent exported data.";
				Assert(errorMessage, ServiceTask.Notify.AsString.Contains("Error message"));
			}
		}

		void SetupExportedFile(ZString filePath)
		{
			DeleteIfExists(filePath);
			AssertEquals("ExportedFile should not exist", false, File.Exists(filePath));
			using (var fs = File.Create(filePath))
			{
			}

			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("1,2,3,4,5,6,7,8,9");
				writer.Flush();
			}
		}

		CoroTransExportServiceTask ServiceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new CoroTransExportServiceTask();
			InitialiseTaskSchedule(ServiceTask);
			var agents = new Guid[] { GlbCompany.CurrentCompany.OrgProxy.PK.ToGuid() };
			MFIDataRegistry.Instance.CaroTransAgentsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, agents);
			MFIDataRegistry.Instance.CaroTransTrackEmailAddress = "CaroTrans@test.com";
			var shipment = Factory.NewWithValidTestData<MFIForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			shipment.Consols.Add(consol);
			shipment.Logs.AddNew(Events.CargoAvailable);
			Factory.Save();
		}
		#endregion
	}
}
