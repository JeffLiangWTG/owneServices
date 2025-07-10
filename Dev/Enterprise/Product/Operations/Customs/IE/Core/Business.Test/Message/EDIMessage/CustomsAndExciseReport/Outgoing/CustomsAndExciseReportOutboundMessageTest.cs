using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CustomsAndExciseReportOutboundMessage))]
	public class CustomsAndExciseReportOutboundMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestLoadMessageDate()
		{
			var date = new ZDateTime(2024, 09, 06);
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.DSR;
			message.MessageDate = date;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedMessage = anotherFactory.Load<CustomsAndExciseReportOutboundMessage>(message.PK);
			AssertEquals(date, loadedMessage.MessageDate);
		}

		public void TestSetMessageDate()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			var messageDate = new ZDateTime(2024, 09, 06);
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.PSR, new ZDateTime(2024, 09, 01), "20240901");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.PCT, new ZDateTime(2024, 09, 01), "20240901");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.PTT, new ZDateTime(2024, 09, 01), "20240901");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.PCI, new ZDateTime(2024, 09, 01), "20240901");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.DSR, new ZDateTime(2024, 09, 06), "20240906");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.DCT, new ZDateTime(2024, 09, 06), "20240906");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.DTT, new ZDateTime(2024, 09, 06), "20240906");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.UDR, new ZDateTime(2024, 09, 06), "20240906");
			AssertSetMessageDate(CustomsAndExciseReportTypeList.Codes.BAL, new ZDateTime(2024, 09, 06), "20240906");
			AssertSetMessageDate("XXX", new ZDateTime(2024, 09, 06), "20240906");

			void AssertSetMessageDate(string messageType, ZDateTime expectedMessageDate, string expectedApplicationReference)
			{
				message.EM_MessageType = messageType;
				message.MessageDate = messageDate;
				AssertEquals(messageType, expectedMessageDate, message.MessageDate);
			}
		}

		public void TestMessageDateReadOnly()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.UDR;
			AssertEquals("Message Date should be read only for UDR", true, message.MessageDateInfo.ReadOnly);

			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.BAL;
			AssertEquals("Message Date should be read only for BAL", true, message.MessageDateInfo.ReadOnly);

			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
			AssertEquals("Message Date should not be read only for PSR before we have got an interpretation", false, message.MessageDateInfo.ReadOnly);

			AttachResponseToMessagePSR(message);
			AssertEquals("Message Date should be readonly once we have got an interpretation", true, message.MessageDateInfo.ReadOnly);
		}

		public void TestMessages()
		{
			var request = Factory.New<CustomsAndExciseReportOutboundMessage>();
			request.EM_SystemCreateTimeUtc = new ZDateTime(2024, 09, 01, 12, 00, 00);
			var response1 = Factory.New<CustomsAndExciseReportInboundMessage>();
			response1.EM_LinkedObject = request;
			response1.EM_SystemCreateTimeUtc = new ZDateTime(2024, 09, 01, 12, 15, 00);
			var response2 = Factory.New<CustomsAndExciseReportInboundMessage>();
			response2.EM_LinkedObject = request;
			response2.EM_SystemCreateTimeUtc = new ZDateTime(2024, 09, 01, 12, 02, 00);
			AssertContainsExactElementsInExactOrder(new CustomsAndExciseReportMessage[] { request, response2, response1 }, request.Messages.Cast<CustomsAndExciseReportMessage>());
		}

		public void TestEM_MessageInterpretation_PSR()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			AssertEquals("EM_MessageInterpretation should be empty before Message Type is set", ZString.Empty, message.EM_MessageInterpretation);
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
			message.MessageDate = new ZDateTime(2024, 11, 01);
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 12, 19, 12, 30, 15);

			var expectedInterpretation = @"<html><body style='font-family: arial;'>
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Message Type</td><td>PSR</td></tr>
					<tr><td>Report Period</td><td>Nov-24</td></tr>
					<tr><td>Created Date</td><td>19-Dec-24 12:30:15</td></tr>
				</table></body></html>";

			AssertXMLEquals("EM_MessageInterpretation should be set when Message Type is set", expectedInterpretation.RemoveLineBreakingsAndIndents(), message.EM_MessageInterpretation.RemoveLineBreakingsAndIndents());

			message.MessageDate = new ZDateTime(2024, 12, 01);
			expectedInterpretation = @"<html><body style='font-family: arial;'>
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Message Type</td><td>PSR</td></tr>
					<tr><td>Report Period</td><td>Dec-24</td></tr>
					<tr><td>Created Date</td><td>19-Dec-24 12:30:15</td></tr>
				</table></body></html>";

			AssertXMLEquals("EM_MessageInterpretation should be updated when Message Date changes", expectedInterpretation.RemoveLineBreakingsAndIndents(), message.EM_MessageInterpretation.RemoveLineBreakingsAndIndents());
		}

		public void TestEM_MessageInterpretation_DSR()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			AssertEquals("EM_MessageInterpretation should be empty before Message Type is set", ZString.Empty, message.EM_MessageInterpretation);
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.DSR;
			message.MessageDate = new ZDateTime(2024, 11, 06);
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 12, 19, 12, 30, 15);

			var expectedInterpretation = @"<html><body style='font-family: arial;'>
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Message Type</td><td>DSR</td></tr>
					<tr><td>Report Period</td><td>06-Nov-24</td></tr>
					<tr><td>Created Date</td><td>19-Dec-24 12:30:15</td></tr>
				</table></body></html>";

			AssertXMLEquals("EM_MessageInterpretation should be set when Message Type is set", expectedInterpretation.RemoveLineBreakingsAndIndents(), message.EM_MessageInterpretation.RemoveLineBreakingsAndIndents());
		}

		public void TestEM_MessageInterpretation_BAL()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			AssertEquals("EM_MessageInterpretation should be empty before Message Type is set", ZString.Empty, message.EM_MessageInterpretation);
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.BAL;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 12, 19, 12, 30, 15);

			var expectedInterpretation = @"<html><body style='font-family: arial;'>
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Message Type</td><td>BAL</td></tr>
					<tr><td>Created Date</td><td>19-Dec-24 12:30:15</td></tr>
				</table></body></html>";

			AssertXMLEquals("EM_MessageInterpretation should be set when Message Type is set", expectedInterpretation.RemoveLineBreakingsAndIndents(), message.EM_MessageInterpretation.RemoveLineBreakingsAndIndents());
		}

		public void TestSetEM_MessageType()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			var messageDate = new ZDateTime(2024, 09, 06);
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.PSR, new ZDateTime(2024, 09, 01));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.PCT, new ZDateTime(2024, 09, 01));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.PTT, new ZDateTime(2024, 09, 01));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.PCI, new ZDateTime(2024, 09, 01));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.DSR, new ZDateTime(2024, 09, 06));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.DCT, new ZDateTime(2024, 09, 06));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.DTT, new ZDateTime(2024, 09, 06));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.UDR, new ZDateTime(2024, 09, 06));
			AssertSetEM_MessageType(CustomsAndExciseReportTypeList.Codes.BAL, new ZDateTime(2024, 09, 06));
			AssertSetEM_MessageType("XXX", new ZDateTime(2024, 09, 06));

			void AssertSetEM_MessageType(string messageType, ZDateTime expectedMessageDate)
			{
				message.EM_MessageType = ZString.Empty;
				message.MessageDate = messageDate;
				message.EM_MessageType = messageType;
				AssertEquals(messageType, expectedMessageDate, message.MessageDate);
			}
		}

		public void TestMessageTypeReadOnly()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
			AssertEquals("Message Type should not be read only before we get an interpretation", false, message.EM_MessageTypeInfo.ReadOnly);

			AttachResponseToMessagePSR(message);
			AssertEquals("Message Type should be read only once we have got an interpretation", true, message.EM_MessageTypeInfo.ReadOnly);
		}

		public void TestEM_MessageType_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<CustomsAndExciseReportOutboundMessage>().EM_MessageTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Report Type", resourceStringDataAttribute.Caption);
			});
		}

		public void TestEM_MessageNum_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<CustomsAndExciseReportOutboundMessage>().EM_MessageNumInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Message Number", resourceStringDataAttribute.Caption);
			});
		}

		public void TestEM_Status_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<CustomsAndExciseReportOutboundMessage>().EM_StatusInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Status", resourceStringDataAttribute.Caption);
			});
		}

		public void TestDate_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<CustomsAndExciseReportOutboundMessage>().DateInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Date", resourceStringDataAttribute.Caption);
			});
		}

		public void TestInterchangeNum_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CustomsAndExciseReportOutboundMessage), nameof(CustomsAndExciseReportOutboundMessage.InterchangeNum), false, x => x.Caption == "Interchange Number");
		}

		public void TestInterchangeType_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CustomsAndExciseReportOutboundMessage), nameof(CustomsAndExciseReportOutboundMessage.InterchangeType), false, x => x.Caption == "Interchange Type");
		}

		public void TestBodyText_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CustomsAndExciseReportOutboundMessage), nameof(CustomsAndExciseReportOutboundMessage.BodyText), false, x => x.Caption == "Body Text");
		}

		public void TestSetDefaultValues()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsAndExcise, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", ZString.Empty, message.EM_Status);
			});
		}

		public void TestStatusAfterSave()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			Factory.Save();
			AssertEquals("EM_Status should be empty when it is not ready to be sent", ZString.Empty, message.EM_Status);

			message.EM_MessageType = "PSR";
			message.MessageDate = new ZDateTime(2024, 09, 12);
			Factory.Save();
			AssertEquals("EM_Status changes to QUE when it is ready to be sent", "QUE", message.EM_Status);

			message.EM_MessageType = "XXX";
			Factory.Save();
			AssertEquals("EM_Status changes back to empty from QUE when it is not ready to be sent", ZString.Empty, message.EM_Status);

			message.EM_Status = "RCV";
			Factory.Save();
			AssertEquals("If not ready to be sent, EM_Status does not change unless old status was QUE", "RCV", message.EM_Status);

			message.EM_MessageType = "PSR";
			message.EM_Status = "SNT";
			Factory.Save();
			AssertEquals("If ready to be sent, EM_Status does not change unless old status was blank", "SNT", message.EM_Status);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "IER00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "IER00000000000002", message2.EM_MessageNum);
		}

		public void TestCollationKey()
		{
			var message = Factory.New<CustomsAndExciseReportOutboundMessage>();
			message.EM_MessageType = "PSR";
			message.MessageDate = new ZDateTime(2024, 10, 1);
			AssertEquals("CollationKey should return the expected value", "PSR20241001",  message.CollationKey);
		}

		public void TestCanContinueWithSaveCore()
		{
			var wrapper = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
			var password = wrapper.GetGlbExternalPassword() ?? Factory.New<GlbCompanyCredential>();
			password.GP_PasswordStatus = "VAL";
			password.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			password.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			Factory.Save();

			var message1 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			AssertEquals(true, message1.CanContinueWithSave);

			password.GP_PasswordStatus = "ZZZ";
			Factory.Save();

			var message2 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			AssertEquals(false, message2.CanContinueWithSave);
		}

		void AttachResponseToMessagePSR(CustomsAndExciseReportOutboundMessage message)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Received;
			var responseMessage = Factory.New<CustomsAndExciseReportInboundMessage>();
			responseMessage.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
			responseMessage.EM_MessageInterpretation = "<html>";
			responseMessage.EM_LinkedObject = message;
			message.Messages.Add(responseMessage);
			Factory.Save();
		}
	}
}
