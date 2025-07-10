using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.CLE.Testing
{
	public class ContainerDatesExceptionBufferTest : TestCaseWithFactory
	{
		public void TestExceptionReport()
		{
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("09-03-2007,,ContainerNumber,,10-03-2007,,,,CLE");
			ExceptionBuffer.AddToExceptionReport("Test Exception Message", row);
			NotificationBuffer notify = new NotificationBuffer();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ExceptionBuffer.CreateExceptionReport(notify);
			Assert("Report message:", notify.AsString.Contains("An Exception report file has been sent to the email notification group."));
			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email subject", ContainerDatesExceptionBuffer.EmailSubject, email.Subject);
			AttachmentDef attachment = email.Attachments[0];
			string expectedData = "Container,Job,Delivered,De-Hire" + System.Environment.NewLine + "ContainerNumber,CLE,09-03-2007,10-03-2007,Test Exception Message" + System.Environment.NewLine;
			AssertEquals("Attachement data:", expectedData, System.Text.Encoding.UTF8.GetString(attachment.Data));
		}

		public void TestNoEmailSentWhenNoExceptionLines()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ExceptionBuffer.CreateExceptionReport(notify);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(notify.AsString.Contains(ContainerDatesExceptionBuffer.NoExceptionFound));
		}

		ContainerDatesExceptionBuffer ExceptionBuffer
		{
			get
			{
				return exceptionBuffer ?? (exceptionBuffer = new ContainerDatesExceptionBuffer());
			}
		}

		ContainerDatesExceptionBuffer exceptionBuffer;
		protected override void SetUp()
		{
			base.SetUp();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			CLEDataRegistry.Instance.ContainerUploadEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			GlbGroup pmgGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			pmgGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
			Factory.Save();
		}
	}
}
