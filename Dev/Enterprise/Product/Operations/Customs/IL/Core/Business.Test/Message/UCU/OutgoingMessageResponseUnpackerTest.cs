using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IL;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class OutgoingMessageResponseUnpackerTest : TestCaseWithFactory
	{
		public void TestUnpack_WhenOneMessageMappedFromThreeAndPeekWayIs2()
		{
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DCAParameters() { PeekWay = PeekWayList.Codes._2 }))
			{
				var loggerMock = new Mock<ILoggingInformation>();
				var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenOneMessageMappedFromThree_Interchange");
				interchange.EI_From = "TEST1";
				var unpacker = new OutgoingMessageResponseUnpacker();
				var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, null, null, null, loggerMock.Object);

				CombineAssertions("Xml include DF_NG_8251_Web02_DeclarationStatus_Response, MN_NG_1200_MSG2_DeliveryOrder_Message and MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message", () =>
				{
					Assert("Succeed to unpack", unpackResult.IsSuccess);
					AssertEquals("While MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message is mapped", 1, unpackResult.EdiMessages.Count);
					var message = unpackResult.EdiMessages.Single();
					AssertEquals("Message Type", "DLO", message.EM_MessageType);
					AssertEquals("Message SubType", "122", message.EM_MessageSubType);
					AssertEquals("Message Application Code", "ILC", message.EM_ApplicationCode);
					AssertEquals("Message Receive/Transmit", "RCV", message.EM_ReceiveTransmit);
					AssertEquals("Message Status", "QUE", message.EM_Status);
					AssertEquals("Message Application Reference", "74f90415-5a31-480d-adae-77a01c837fc7", message.EM_ApplicationReference);
					AssertLog(loggerMock, 0, 2, 1);
				});

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, "GEN");
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "920");
				var message920 = Factory.LoadTop1<EDIMessage>(query);
				AssertContains("<CorrelationIDs>74f90415-5a31-480d-adae-77a01c837fc7</CorrelationIDs>", message920.EM_MessageText);

				loggerMock = new Mock<ILoggingInformation>();
				interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenOneMessageMappedFromThree_Interchange");
				interchange.EI_From = "TEST2";
				unpacker = new OutgoingMessageResponseUnpacker();
				messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, null, null, null, loggerMock.Object);

				CombineAssertions("When Outgoing Message CorrelationId Exist should Skip Unpack", () =>
				{
					Assert("Succeed to unpack", unpackResult.IsSuccess);
					AssertEquals("should Skip Unpack", 0, unpackResult.EdiMessages.Count);
					AssertLog(loggerMock, 2, 1, 0);
				});

				var messages920 = Factory.Load<EDIMessage>(query);
				AssertEquals(2, messages920.Length);
			}
		}

		public void TestUnpack_WhenAllMessagesNotMappedAndPeekWayIs2()
		{
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DCAParameters() { PeekWay = PeekWayList.Codes._2 }))
			{
				var loggerMock = new Mock<ILoggingInformation>();
				var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenAllMessagesNotMapped_Interchange");
				var unpacker = new OutgoingMessageResponseUnpacker();
				var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, null, null, null, loggerMock.Object);

				CombineAssertions("Xml include DF_NG_8251_Web02_DeclarationStatus_Response, and MN_NG_1200_MSG2_DeliveryOrder_Message that are not mapped", () =>
				{
					Assert("Succeed to unpack", unpackResult.IsSuccess);
					AssertEquals("No Message created", 0, unpackResult.EdiMessages.Count);
					AssertLog(loggerMock, 0, 2, 0);
				});

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, "GEN");
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "920");
				var message920 = Factory.LoadTop1<EDIMessage>(query);
				AssertNull(message920);
			}
		}

		public void TestUnpack_WhenRowNumbersIsZeroAndPeekWayIs2()
		{
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DCAParameters() { PeekWay = PeekWayList.Codes._2 }))
			{
				var loggerMock = new Mock<ILoggingInformation>();
				var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenRowNumbersIsZero_Interchange");
				var unpacker = new OutgoingMessageResponseUnpacker();
				var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, null, null, null, loggerMock.Object);

				CombineAssertions("When Row Numbers Is Zero", () =>
				{
					Assert("Succeed to unpack", unpackResult.IsSuccess);
					AssertEquals("No Message created", 0, unpackResult.EdiMessages.Count);
					AssertLog(loggerMock, 0, 0, 0);
				});

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, "GEN");
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "920");
				var message920 = Factory.LoadTop1<EDIMessage>(query);
				AssertNull(message920);
			}
		}

		public void TestUnpack_WhenTextIsNotValidXMLAndPeekWayIs2()
		{
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DCAParameters() { PeekWay = PeekWayList.Codes._2 }))
			{
				var loggerMock = new Mock<ILoggingInformation>();
				const string expectErerrorReason = "The interchange body text is not valid XML.";
				var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenTextIsNotValidXML_Interchange");
				var unpacker = new OutgoingMessageResponseUnpacker();
				var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, null, null, null, loggerMock.Object);

				CombineAssertions("When body text is not valid XML", () =>
				{
					AssertEquals("Fail to unpack", false, unpackResult.IsSuccess);
					AssertEquals("ErrorReason", expectErerrorReason, unpackResult.ErrorReason);
				});

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, "GEN");
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "920");
				var message920 = Factory.LoadTop1<EDIMessage>(query);
				AssertNull(message920);
			}
		}

		public void TestUnpack_WhenOneMessageMappedFromThreeAndPeekWayIs3()
		{
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DCAParameters() { PeekWay = PeekWayList.Codes._3 }))
			{
				var loggerMock = new Mock<ILoggingInformation>();
				var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenOneMessageMappedFromThree_Interchange");
				interchange.EI_From = "TEST1";
				var unpacker = new OutgoingMessageResponseUnpacker();
				var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, null, null, null, loggerMock.Object);

				CombineAssertions("Xml include DF_NG_8251_Web02_DeclarationStatus_Response, MN_NG_1200_MSG2_DeliveryOrder_Message and MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message", () =>
				{
					Assert("Succeed to unpack", unpackResult.IsSuccess);
					AssertEquals("While MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message is mapped", 1, unpackResult.EdiMessages.Count);
					var message = unpackResult.EdiMessages.Single();
					AssertEquals("Message Type", "DLO", message.EM_MessageType);
					AssertEquals("Message SubType", "122", message.EM_MessageSubType);
					AssertEquals("Message Application Code", "ILC", message.EM_ApplicationCode);
					AssertEquals("Message Receive/Transmit", "RCV", message.EM_ReceiveTransmit);
					AssertEquals("Message Status", "QUE", message.EM_Status);
					AssertEquals("Message Application Reference", "74f90415-5a31-480d-adae-77a01c837fc7", message.EM_ApplicationReference);
					AssertLog(loggerMock, 0, 2, 1);
				});

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, "GEN");
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "920");
				var message920 = Factory.LoadTop1<EDIMessage>(query);
				AssertNull("When Peek Way is set to 3, there is no need to send a sync acknowledgment message.", message920);
			}
		}

		ILEDIInterchange CreateInterchange(string embeddedResourceResponseName)
		{
			var interchange = Factory.New<ILEDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath($"{embeddedResourceResponseName}.xml"));
			return interchange;
		}

		protected override void SetUp()
		{
			base.SetUp();

			currentCompany = GlbCompany.CurrentCompany;
			currentCustomsRegistryNo = currentCompany.GC_CustomsRegistrationNo;
			currentCompany.GC_CustomsRegistrationNo = "ILCOM_REGISTERNO";
			var companyWrapper = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(currentCompany);
			var externalPassword = companyWrapper.GetGlbExternalPasswordOrCreateNew();
			externalPassword.GP_MailBoxID = "560038416";
			currentCompany.Factory.Save();
		}

		protected override void TearDown()
		{
			currentCompany.GC_CustomsRegistrationNo = currentCustomsRegistryNo;
			base.TearDown();
		}

		static void AssertLog(Mock<ILoggingInformation> loggerMock,
			int correlationAlreadyExistMessageCount,
			int unsupportedServiceMessageCount,
			int totalMessagesCreated)
		{
			const string expectedError = "ILC a-sync: New EDI Messages created - totalMessagesCreated \r\nNumber of discarded messages (Correlation already exist) – correlationAlreadyExistMessageCount \r\nNumber of discarded messages (Service not supported) – unsupportedServiceMessageCount";
			loggerMock.Verify(l => l.Log(
				expectedError
				.Replace("correlationAlreadyExistMessageCount", correlationAlreadyExistMessageCount.ToString())
				.Replace("unsupportedServiceMessageCount", unsupportedServiceMessageCount.ToString())
				.Replace("totalMessagesCreated", totalMessagesCreated.ToString())));
		}

		GlbCompany currentCompany;
		ZString currentCustomsRegistryNo;
	}
}
