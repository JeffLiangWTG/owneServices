using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using EDIMessage = Enterprise.Customs.IE.Business.EDIMessage;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NctsMessageSenderTest : TestCaseWithFactory
	{
		public void TestMessageTypeDefault_CustomsStatusBlank()
		{
			var sendingObject = CreateSendingObjectForTest("");
			AssertEquals("Default message type should be 015", NCTSOutgoingMessageTypeList.Codes.DeclarationData, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusENQ()
		{
			var sendingObject = CreateSendingObjectForTest(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry);
			AssertEquals("Default message type should be 141", NCTSOutgoingMessageTypeList.Codes.InformationAboutNonArrivedMovement, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusRFA()
		{
			var sendingObject = CreateSendingObjectForTest(NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice);
			AssertEquals("Default message type should be 054", NCTSOutgoingMessageTypeList.Codes.RequestOfRelease, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusC00()
		{
			var sendingObject = CreateSendingObjectForTest(NCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
			AssertEquals("Default message type should be 054", NCTSOutgoingMessageTypeList.Codes.RequestOfRelease, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusC01()
		{
			var sendingObject = CreateSendingObjectForTest(NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest);
			AssertEquals("Default message type should be 054", NCTSOutgoingMessageTypeList.Codes.RequestOfRelease, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusC02()
		{
			var sendingObject = CreateSendingObjectForTest(NCTS5DepartureCustomsStatusList.Codes.IntentionToControl);
			AssertEquals("Default message type should be 054", NCTSOutgoingMessageTypeList.Codes.RequestOfRelease, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusPRE()
		{
			var sendingObject = CreateSendingObjectForTest(NCTS5DepartureCustomsStatusList.Codes.PreLodged);
			AssertEquals("Default message type should be 170", NCTSOutgoingMessageTypeList.Codes.PresentationNotification, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusXXX_NoMRN()
		{
			var sendingObject = CreateSendingObjectForTest("XXX");
			AssertEquals("Default message type should not be set", string.Empty, sendingObject.MessageType);
		}

		public void TestMessageTypeDefault_CustomsStatusXXX_WithMRN()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("XXX");
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "IE23ROS12489725";
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			AssertEquals("Default message type should be 013", NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment, sendingObject.MessageType);
		}

		public void TestSendIE013()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			var sendingAction2 = new NctsMessageSendingAction(sendingObject);
			sendingAction2.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment;
			var sender2 = new NctsMessageSender(sendingAction2);
			sender2.Send();

			AssertEquals("Message count", 2, movementHeader.Messages.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Application code", "IEN", movementHeader.Messages[1].EM_ApplicationCode);
				AssertEquals("Message Type", "013", movementHeader.Messages[1].EM_MessageType);
				AssertEquals("Direction", "TRX", movementHeader.Messages[1].EM_ReceiveTransmit);
				AssertEquals("Status", "QUE", movementHeader.Messages[1].EM_Status);
				AssertStartsWith("Message Text", "<q1:CC013C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[1].EM_MessageText);
				AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC013C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[1].EM_MessageInterpretation);
			});
		}

		public void TestSendIE014()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			sendingObject.Justification = "Because it's worth it";

			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			AssertEquals("Justification in Action", "Because it's worth it", sendingAction.Justification);
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			var sendingAction2 = new NctsMessageSendingAction(sendingObject);
			sendingAction2.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest;
			var sender2 = new NctsMessageSender(sendingAction2);
			sender2.Send();

			AssertEquals("Message count", 2, movementHeader.Messages.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Application code", "IEN", movementHeader.Messages[1].EM_ApplicationCode);
				AssertEquals("Message Type", "014", movementHeader.Messages[1].EM_MessageType);
				AssertEquals("Direction", "TRX", movementHeader.Messages[1].EM_ReceiveTransmit);
				AssertEquals("Status", "QUE", movementHeader.Messages[1].EM_Status);
				AssertStartsWith("Message Text", "<q1:CC014C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[1].EM_MessageText);
				AssertContains("Justification in XML", "<justification>Because it's worth it</justification>", movementHeader.Messages[1].EM_MessageText);
				AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC014C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[1].EM_MessageInterpretation);
			});
		}

		public void TestSendIE015()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;

			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "015", movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertEquals("Status", "QUE", movementHeader.Messages[0].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC015C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC015C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE026()
		{
			var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var sendingAction = new GuaranteeAccessCodesSendingAction(cusGuarantee);

			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			AssertEquals("Message count", 1, cusGuarantee.Messages.Count);
			AssertEquals("Application code", "IEN", cusGuarantee.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "026", cusGuarantee.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", cusGuarantee.Messages[0].EM_ReceiveTransmit);
			AssertEquals("Status", "QUE", cusGuarantee.Messages[0].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC026C xmlns:q1=\"http://ncts.dgtaxud.ec\">", cusGuarantee.Messages[0].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC026C xmlns:q1=\"http://ncts.dgtaxud.ec\">", cusGuarantee.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE034()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("PRE");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			var nctsSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var nctsSendingAction = new NctsMessageSendingAction(nctsSendingObject);
			nctsSendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.QueryOnGuarantees;
			var nctsSender = new NctsMessageSender(nctsSendingAction);
			AssertNoExceptionThrown("Unexpected type will not rise error", () => { nctsSender.Send(); });

			var sendingAction = new QueryOnGuaranteeSendingAction(nctsHeader);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.QueryOnGuarantees;
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "034", movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertEquals("Status", "QUE", movementHeader.Messages[0].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC034C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC034C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE044()
		{
			var nctsHeader = CreateNctsArrivalHeaderForTest("PRE");
			nctsHeader.ArrivalMovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			var nctsSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var nctsSendingAction = new NctsMessageSendingAction(nctsSendingObject);
			nctsSendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.UnloadingRemarks;
			var nctsSender = new NctsMessageSender(nctsSendingAction);
			AssertNoExceptionThrown("Unexpected type will not rise error", () => { nctsSender.Send(); });

			var sendingAction = new QueryOnGuaranteeSendingAction(nctsHeader);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.UnloadingRemarks;
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			AssertEquals("Message count", 2, nctsHeader.Messages.Count);
			AssertEquals("Application code", "IEN", nctsHeader.Messages[1].EM_ApplicationCode);
			AssertEquals("Message Type", "044", nctsHeader.Messages[1].EM_MessageType);
			AssertEquals("Direction", "TRX", nctsHeader.Messages[1].EM_ReceiveTransmit);
			AssertEquals("Status", "QUE", nctsHeader.Messages[1].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC044C xmlns:q1=\"http://ncts.dgtaxud.ec\">", nctsHeader.Messages[1].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC044C xmlns:q1=\"http://ncts.dgtaxud.ec\">", nctsHeader.Messages[1].EM_MessageInterpretation);
		}

		public void TestSendIE054()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("C00");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.RequestOfRelease;

			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "054", movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertEquals("Status", "QUE", movementHeader.Messages[0].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC054C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC054C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE141()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("PRE");
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			sendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1, 15, 30, 45);
			sendingObject.EnquiryText = "HELLO WORLD";
			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.InformationAboutNonArrivedMovement;

			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", NCTSOutboundEDIMessage.ApplicationCodes.IECustomsNCTS, movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", NCTSOutgoingMessageTypeList.Codes.InformationAboutNonArrivedMovement, movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", NCTSOutboundEDIMessage.Direction.Transmit, movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertEquals("Status", NCTSOutboundEDIMessage.Status.Queued, movementHeader.Messages[0].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC141C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC141C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE170()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("PRE");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.PresentationNotification;

			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "170", movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertEquals("Status", "QUE", movementHeader.Messages[0].EM_Status);
			AssertStartsWith("Message Text", "<q1:CC170C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);
			AssertStartsWith("Message Interpretation", "<html><body><xmp><q1:CC170C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE013_AssignsDeclarationGoodsItemNumbers() => TestSend_GoodsItemsDeclarationItemNumberAreAssigned(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment, true);

		public void TestSendIE015_AssignsDeclarationGoodsItemNumbers() => TestSend_GoodsItemsDeclarationItemNumberAreAssigned(NCTSOutgoingMessageTypeList.Codes.DeclarationData);

		void TestSend_GoodsItemsDeclarationItemNumberAreAssigned(string messageType, bool createIE15Message = false)
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			if (createIE15Message)
			{
				var edmMessage = Factory.New<NCTSInboundEDIMessage>();
				edmMessage.EM_ApplicationCode = "IEN";
				edmMessage.EM_MessageType = "015";
				edmMessage.EM_ReceiveTransmit = "TRX";
				edmMessage.EM_Status = "SNT";
				edmMessage.EM_MessageText = "<message></message>";
				edmMessage.EM_LinkTable = "CusInBondHeader";
				edmMessage.EM_LinkUniqueID = nctsHeader.PK;
				edmMessage.MessageNumberStrategy = new FixedMessageNumberStrategyForTesting("IE000012334");
				edmMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			}

			var nctsBill1 = nctsHeader.Bills.AddNew();
			var goodsItem1 = nctsBill1.GoodsItems.AddNew();
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			var goodsItem2 = nctsBill1.GoodsItems.AddNew();

			var nctsBill2 = nctsHeader.Bills.AddNew();
			var goodsItem3 = nctsBill2.GoodsItems.AddNew();
			var goodsItem4 = nctsBill2.GoodsItems.AddNew();
			goodsItem4.BY_DeclarationGoodsItemNumber = 3;
			var goodsItem5 = nctsBill2.GoodsItems.AddNew();

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject) { MessageType = messageType };
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("GoodsItems 2", 4, goodsItem2.BY_DeclarationGoodsItemNumber);
				AssertEquals("GoodsItems 3", 5, goodsItem3.BY_DeclarationGoodsItemNumber);
				AssertEquals("GoodsItems 5", 6, goodsItem5.BY_DeclarationGoodsItemNumber);
			});
		}

		[TestDate]
		public void TestSendIE013_SettingValuationDate()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject) { MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment };
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);

			nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

			sender.Send();

			AssertEquals(ZDateTime.BrettsBirthday, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		[TestDate]
		public void TestSendIE015_SettingValuationDate()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject) { MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData };
			var sender = new NctsMessageSender(sendingAction);
			nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

			sender.Send();

			AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		[TestDate]
		public void TestSendIEOther_SettingValuationDate()
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest("");
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject) { MessageType = NCTSOutgoingMessageTypeList.Codes.RequestOfRelease };
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();

			AssertEquals(ZDateTime.Empty, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		NctsHeaderMessageSendingObject CreateSendingObjectForTest(ZString customsStatus)
		{
			var nctsHeader = CreateNctsDepartureHeaderForTest(customsStatus);
			return new NctsHeaderMessageSendingObject(nctsHeader);
		}

		NctsHeader CreateNctsDepartureHeaderForTest(ZString customsStatus)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
			SetPrincipal(nctsHeader);
			return nctsHeader;
		}

		NctsHeader CreateNctsArrivalHeaderForTest(ZString customsStatus)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
			SetPrincipal(nctsHeader);
			return nctsHeader;
		}

		void SetPrincipal(NctsHeader nctsHeader)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "0123456789000", "TIR123");
		}

		class FixedMessageNumberStrategyForTesting : IMessageNumberStrategy
		{
			public FixedMessageNumberStrategyForTesting(ZString messageNumber)
			{
				this.messageNumber = messageNumber;
			}
			readonly ZString messageNumber;

			public string GetMessageReferenceNumber() => messageNumber;
		}
	}
}
