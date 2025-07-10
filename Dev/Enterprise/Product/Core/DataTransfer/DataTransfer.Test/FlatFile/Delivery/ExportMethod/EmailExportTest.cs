using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class EmailExportTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliver_EmailSendFailedExceptionOccured()
		{
			var instructions = new ExportInstructions();
			instructions.EmailProperties.AddRecipient("test@test.com");
			var export = new EmailExportThrowsException(instructions, notify);

			AttemptToDeliverWithFile(export);
			AssertEquals("No emails sent as exception has occured", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEquals(1, notify.Events.Length);
			AssertEquals("Error: No members of the Orders Notification Group exist (There was an error sending the email : TestFailMessage)", notify.Events[0].Message);
			AssertEquals(typeof(ErrorType), notify.Events[0].Type.GetType());
		}

		public void TestExportType()
		{
			var instructions = new ExportInstructions();
			var export = new EmailExport(instructions, notify);
			AssertEquals("Should ALWAYS be email delivery type", ExportType.Email, export.ExportType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliver()
		{
			var instructions = new ExportInstructions();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var export = new EmailExport(instructions, notify);
			AttemptToDeliverWithFile(export);
			AssertEquals("No email sent because there is information missing from instructions.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			string nonexistentFile = @"BasePath\a\b\c.txt";
			instructions.EmailProperties.AddRecipient("test@test.com");
			export.Deliver(nonexistentFile);
			AssertEquals("No email sent because the temp file doesn't exist", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			AttemptToDeliverWithFile(export);
			AssertEquals("1 email sent now that there is a recipient and an acutal file was used in export", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCanDeliver()
		{
			var instructions = new ExportInstructions();
			var export = new EmailExport(instructions, notify);
			AssertEquals("Prerequisite conditions not met - no recipients", false, export.CanDeliver);

			instructions.EmailProperties.AddRecipient("test@test.com");
			AssertEquals("Prerequisite conditions met - recipients exist", true, export.CanDeliver);
		}

		void AttemptToDeliverWithFile(EmailExport export)
		{
			var testFile = BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\Delivery\TestFiles\File1.txt";
			var tempFileToDoThings = Temp.GetTempFileName();

			//			using (TempFile TempFile = (TempFile)TempFile.New())
			//			{
			File.Copy(testFile, tempFileToDoThings, true);
			File.SetAttributes(tempFileToDoThings, FileAttributes.Normal);
			export.Deliver(tempFileToDoThings);
			//			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			notify = new NotificationBuffer();
		}

		NotificationBuffer notify;

		sealed class EmailExportThrowsException : EmailExport
		{
			public EmailExportThrowsException(ExportInstructions instructions, INotifications notifications) : base(instructions, notifications)
			{
			}

			protected override void SendEmail(EmailDef email)
			{
				throw new EmailSendFailedException("TestFailMessage");
			}
		}
	}
}
