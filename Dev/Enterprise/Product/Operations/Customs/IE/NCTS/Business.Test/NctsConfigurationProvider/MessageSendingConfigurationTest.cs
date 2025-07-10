using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
	{
		public override void TestGetNewNctsMessageSendingObjectParent()
		{
			var sendingObjectParent = configuration.GetNewNctsHeaderMessageSendingObjectParent(header);
			AssertType<NctsHeaderMessageSendingObjectParent>("Sending Object Parent Type", sendingObjectParent);
		}

		public override void TestGetShouldSendDefault()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(header);
			AssertEquals("Should Send Default", true, configuration.GetShouldSendDefault(sendingObject));
		}

		public override void TestMessageTypeList()
		{
			var testList = new NCTSOutgoingDepartureMessageTypeList();
			testList.RemoveCode(NCTSOutgoingDepartureMessageTypeList.Codes.QueryOnGuarantees);
			testList.Sort();

			var messageTypeList = configuration.MessageTypeList(header);
			AssertEquals("MessageTypeList", testList.CodesAsString, messageTypeList.CodesAsString);
		}

		public override void TestSetDefaultMessageType()
		{
			AssertDefaultMessageType("", "015");
			AssertDefaultMessageType(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, "141");
			AssertDefaultMessageType(NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, "054");
			AssertDefaultMessageType(NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, "054");
			AssertDefaultMessageType(NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, "054");
			AssertDefaultMessageType(NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, "054");
			AssertDefaultMessageType(NCTS5DepartureCustomsStatusList.Codes.PreLodged, "170");

			header.MovementHeader.BM_CustomsStatus = "XYZ";
			header.MovementReferenceEntryNumber.CE_EntryNum = "";
			var sendingObject = new NctsHeaderMessageSendingObject(header);
			AssertEquals("Default MessageType", "", sendingObject.MessageType);
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			sendingObject = new NctsHeaderMessageSendingObject(header);
			AssertEquals("Default MessageType", NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment, sendingObject.MessageType);

			void AssertDefaultMessageType(string status, string expectedDefaultMessageType)
			{
				header.MovementHeader.BM_CustomsStatus = status;
				var sendObj = new NctsHeaderMessageSendingObject(header);
				AssertEquals("Default MessageType", expectedDefaultMessageType, sendObj.MessageType);
			}
		}

		public override void TestShowJustification()
		{
			AssertEquals(expected: true, configuration.ShowJustification(header));
		}

		protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
