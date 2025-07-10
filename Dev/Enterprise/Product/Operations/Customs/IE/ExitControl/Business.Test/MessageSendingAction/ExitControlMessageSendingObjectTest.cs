using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObject))]
	class ExitControlMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageType()
		{
			CombineAssertions(() =>
			{
				AssertMessageType(ExitReportTypeList.Codes.Presentation, AESOutgoingMessageTypeList.Codes.ArrivalAtExit);
				AssertMessageType(ExitReportTypeList.Codes.InformationOnNonExitedExport, AESOutgoingMessageTypeList.Codes.InformationOnNonExitedExport);
				AssertMessageType(ExitReportTypeList.Codes.ExitNotification, AESOutgoingMessageTypeList.Codes.ExitNotification);
			});
		}

		void AssertMessageType(string reportType, string expectedMessageType)
		{
			var exitReport = Factory.New<CusExitReport>();
			exitReport.CER_Type = reportType;
			var sendingObject = new ExitControlMessageSendingObject(exitReport);

			AssertEquals("Should return correct MessageType", expectedMessageType, sendingObject.MessageType);
		}

		public void TestCreateSender()
		{
			AssertType<ExitControlMessageSender>(((ExitControlMessageSendingObject)GetNewBusinessObject()).CreateSender());
		}

		public void TestExitReport()
		{
			var sendingObject = (ExitControlMessageSendingObject)GetNewBusinessObject();
			AssertSame(exitReport, sendingObject.MessagingObject);
		}

		public void TestDateTimeCaption()
		{
			var sendingObject = (ExitControlMessageSendingObject)GetNewBusinessObject();
			var captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.DateTimeInfo);
			AssertEquals("DateTimeInfo should have correct Caption", "Exit/Arrival Date", captionAttr.Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExitControlMessageSendingObject(exitReport);
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitReport = Factory.New<CusExitReport>();
		}
		CusExitReport exitReport;
	}
}
