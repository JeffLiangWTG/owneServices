using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	class TemporaryStorageMessagingProviderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageMessagingProviderTest<TemporaryStorageMessagingProvider>
	{
		protected override TemporaryStorageMessagingProvider GetProvider(EU.Business.CusTempStorage.TemporaryStorageHeader header)
		{
			return header.MessagingProvider as TemporaryStorageMessagingProvider;
		}

		public new void TestDefaultingOfMessageType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			header.CustomsStatus = "XYZ";
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("When Message Mode is 'TC' and entry status is not 'REG' then the default message type value should be 'T15'", IETemporaryStorageMessageTypeList.Codes.Declaration, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			header.CustomsStatus = "XYZ";
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("When Message Mode is 'TS' and entry status is not 'REG' then the default message type value should be 'T15'", IETemporaryStorageMessageTypeList.Codes.Declaration, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			header.CustomsStatus = AISEntryStatusList.Codes.Registered;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("When Message Mode is 'TC' and entry status is 'REG' then the default message type value should be 'T13'", IETemporaryStorageMessageTypeList.Codes.Amendment, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			header.CustomsStatus = AISEntryStatusList.Codes.Registered;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("When Message Mode is 'TS' and entry status is 'REG' then the default message type value should be 'T13'", IETemporaryStorageMessageTypeList.Codes.Amendment, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("When Message Mode is 'PN' then the default message type value should be 'T32'", IETemporaryStorageMessageTypeList.Codes.PresentationNotification, sendingObject.MessageType);
		}

		public void TestDeclarationType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("G4G3", sendingObject.DeclarationType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("G4", sendingObject.DeclarationType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("G3", sendingObject.DeclarationType);
		}

		public void TestEntryStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.CustomsStatus = AISEntryStatusList.Codes.Registered;
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("REG", sendingObject.EntryStatus);

			header.CustomsStatus = AISEntryStatusList.Codes.Cancelled;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("CAN", sendingObject.EntryStatus);

			header.CustomsStatus = AISEntryStatusList.Codes.Prelodged;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("PRE", sendingObject.EntryStatus);

			header.CustomsStatus = AISEntryStatusList.Codes.Rejected;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("REJ", sendingObject.EntryStatus);

			header.CustomsStatus = AISEntryStatusList.Codes.Accepted;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("ACC", sendingObject.EntryStatus);
		}

		public void TestMessageStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageStatus = LogicalStatusList.Codes.Accepted;
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("ACC", sendingObject.MessageStatus);

			header.AMA_MessageStatus = LogicalStatusList.Codes.Failed;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("FAL", sendingObject.MessageStatus);

			header.AMA_MessageStatus = LogicalStatusList.Codes.Acknowledged;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("ACK", sendingObject.MessageStatus);

			header.AMA_MessageStatus = LogicalStatusList.Codes.Error;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("ERR", sendingObject.MessageStatus);

			header.AMA_MessageStatus = LogicalStatusList.Codes.Invalid;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("INV", sendingObject.MessageStatus);

			header.AMA_MessageStatus = LogicalStatusList.Codes.Sent;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("SNT", sendingObject.MessageStatus);
		}

		public new void TestSendMessage()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Declaration, ImportDeclarationApplicationCodeList.Codes.V2);
		}

		public void TestSendTS315_UCC6()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Declaration, ImportDeclarationApplicationCodeList.Codes.V2);
		}

		public void TestSendTS314_UCC6()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Invalidation, ImportDeclarationApplicationCodeList.Codes.V2);
		}

		public void TestSendTS313_UCC6()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Amendment, ImportDeclarationApplicationCodeList.Codes.V2);
		}

		public void TestSendTS332_UCC6()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.PresentationNotification, ImportDeclarationApplicationCodeList.Codes.V2);
		}

		public void TestSendTS370_UCC6()
		{
			// To be implemented in future work item
			// Once implemented, this test should fail, change to run TestSendTSMessage() instead
			TestSendTSMessageNotSupported(IETemporaryStorageMessageTypeList.Codes.GoodsStatusReportDeclaration, ImportDeclarationApplicationCodeList.Codes.V2, "Message Type 'T70' is not currently supported.");
		}

		public void TestSendTS315_UCC5()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Declaration, ImportDeclarationApplicationCodeList.Codes.V1);
		}

		public void TestSendTS314_UCC5()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Invalidation, ImportDeclarationApplicationCodeList.Codes.V1);
		}

		public void TestSendTS313_UCC5()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.Amendment, ImportDeclarationApplicationCodeList.Codes.V1);
		}

		public void TestSendTS332_UCC5()
		{
			TestSendTSMessage(IETemporaryStorageMessageTypeList.Codes.PresentationNotification, ImportDeclarationApplicationCodeList.Codes.V1);
		}

		public void TestSendTS370_UCC5()
		{
			// To be implemented in future work item
			// Once implemented, this test should fail, change to run TestSendTSMessage() instead
			TestSendTSMessageNotSupported(IETemporaryStorageMessageTypeList.Codes.GoodsStatusReportDeclaration, ImportDeclarationApplicationCodeList.Codes.V1, "Message Type 'T70' is not currently supported for V1 (UCC5).");
		}

		public void TestSendTSMessage(ZString messageType, ZString manifestType)
		{
			var port1 = Factory.New<RefUNLOCO>();
			port1.RL_Code = "AAA";
			port1.RL_RN_NKCountryCode = "FR";
			var port2 = Factory.New<RefUNLOCO>();
			port2.RL_Code = "BBB";
			port2.RL_RN_NKCountryCode = "IE";
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.AMA_ManifestType = manifestType;
			header.MasterBill.ABL_RL_NKPortOfLoading = "AAA";
			header.MasterBill.ABL_RL_NKPortOfDischarge = "BBB";
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = messageType;
			var provider = GetProvider(header);
			provider.SendMessage(sendingObject, new SendsMessagesToCustomsShutterUpperer(), null);
			AssertEquals(1, header.Messages.Count);
		}

		public void TestSendTSMessageNotSupported(ZString messageType, ZString manifestType, string expectedErrorMessage)
		{
			var port1 = Factory.New<RefUNLOCO>();
			port1.RL_Code = "AAA";
			port1.RL_RN_NKCountryCode = "FR";
			var port2 = Factory.New<RefUNLOCO>();
			port2.RL_Code = "BBB";
			port2.RL_RN_NKCountryCode = "IE";
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.AMA_ManifestType = manifestType;
			header.MasterBill.ABL_RL_NKPortOfLoading = "AAA";
			header.MasterBill.ABL_RL_NKPortOfDischarge = "BBB";
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = messageType;
			var provider = GetProvider(header);
			var shutterUpperer = new SendsMessagesToCustomsShutterUpperer();
			provider.SendMessage(sendingObject, shutterUpperer, null);
			AssertEquals(0, header.Messages.Count);
			AssertEquals(1, shutterUpperer.LastErrors.Count);
			AssertEquals(expectedErrorMessage, shutterUpperer.LastErrorsAsString.Trim());
		}
	}
}
