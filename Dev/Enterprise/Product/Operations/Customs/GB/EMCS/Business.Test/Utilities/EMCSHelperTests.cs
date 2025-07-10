using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSHelperTests : TestCaseWithFactory
	{
		public void TestGetDeclarationFromCustomsBusinessResponse()
		{
			var responseHeader = "<eHubTrackingId><<eHubTrackingId>></eHubTrackingId>";
			var eHubTrackingId = "02C64674-184A-4A6D-9CCC-EF53FBB5C512";
			var encodedMessageBody = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ValidMessage));
			var xml = InterchangeBodyText.Replace(ResponseBodyPlaceholder, encodedMessageBody)
				.Replace(ResponseHeaderPlaceHolder, responseHeader)
				.Replace(EHubTrackingIdPlaceholder, eHubTrackingId);
			_ = SetupOutgoingInterchangeAndMessage(declaration, eHubTrackingId, "999");
			AssertDeclarationFromCustomsBusinessResponse(xml);

			responseHeader = "<JobNumber><<JobNumber>></JobNumber>";
			var jobNumber = "B0001000";
			xml = InterchangeBodyText.Replace(ResponseBodyPlaceholder, encodedMessageBody)
				.Replace(ResponseHeaderPlaceHolder, responseHeader)
				.Replace(JobNumberPlaceholder, jobNumber);
			_ = SetupOutgoingInterchangeAndMessage(declaration, eHubTrackingId, "1000");
			AssertDeclarationFromCustomsBusinessResponse(xml);

			responseHeader = "<ServiceReference><<ServiceReference>></ServiceReference>";
			var serviceReference = "SERVICEREFERENCEVALUE";
			xml = InterchangeBodyText.Replace(ResponseBodyPlaceholder, encodedMessageBody)
				.Replace(ResponseHeaderPlaceHolder, responseHeader)
				.Replace(ServiceReferencePlaceholder, serviceReference);
			var message = SetupOutgoingInterchangeAndMessage(declaration, string.Empty, serviceReference, "1001");
			AssertDeclarationFromCustomsBusinessResponse(xml);

			declaration.JE_DeclarationReference = serviceReference;
			message.EM_ApplicationReference = string.Empty;
			AssertDeclarationFromCustomsBusinessResponse(xml);
		}

		void AssertDeclarationFromCustomsBusinessResponse(string xml)
		{
			var customsBusinessResponse = new EMCSCustomsBusinessResponse(xml);
			AssertEquals(declaration, EMCSHelper.GetDeclarationFromCustomsBusinessResponse(Factory, customsBusinessResponse));
		}

		public void TestGetDeclarationFromInboundMessage()
		{
			var eHubTrackingId = "1DEDA5DE-495A-4874-A824-B118F03C2C44";
			var applicationReference = "6358120B-7084-4D74-BDB8-C3C771CA6FD9";
			_ = SetupOutgoingInterchangeAndMessage(declaration, eHubTrackingId, applicationReference, "999");

			CombineAssertions(() =>
			{
				var inboundMessage = Factory.New<EMCSInboundEDIMessage>();
				inboundMessage.EM_ApplicationReference = applicationReference;
				AssertEquals(declaration, EMCSHelper.GetDeclarationFromInboundMessage(inboundMessage));
				inboundMessage.EM_ApplicationReference = "B0001000";
				AssertEquals(declaration, EMCSHelper.GetDeclarationFromInboundMessage(inboundMessage));
				inboundMessage.EM_ApplicationReference = null;
				AssertNull(EMCSHelper.GetDeclarationFromInboundMessage(inboundMessage));
				inboundMessage.EM_ApplicationReference = "B0001001";
				AssertNull(EMCSHelper.GetDeclarationFromInboundMessage(inboundMessage));
			});
		}

		public void TestGetDeclarationFromEADNumber()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			cusEntryNumber.CE_EntryLineReference = "1";
			Factory.Save();

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber2 = CusEntryNumber.New(declaration2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			cusEntryNumber2.CE_EntryNum = "MRN98761234";
			cusEntryNumber2.CE_EntryLineReference = "1";
			Factory.Save();
			var message = Factory.New<EMCSInboundEDIMessage>();
			AssertEquals(declaration2, EMCSHelper.GetDeclarationFromEADNumber(message, "MRN98761234", "1"));
		}

		public void TestGetOriginalMessage()
		{
			var eHubTrackingId = "1DEDA5DE-495A-4874-A824-B118F03C2C44";
			var message = SetupOutgoingInterchangeAndMessage(declaration, eHubTrackingId, "999");
			_ = SetupOutgoingInterchangeAndMessage(declaration, eHubTrackingId, "1000");
			AssertEquals(2, declaration.Messages.NumberOfOutgoingMessages);
			AssertEquals(message, EMCSHelper.GetOriginalMessage(Factory, new ZGuid(eHubTrackingId)));
		}

		public void TestGetApplicationReferenceFromLastOutgoingMessage()
		{
			_ = SetupOutgoingInterchangeAndMessage(declaration, string.Empty, "MESSAGE1REFERENCE", "999");
			_ = SetupOutgoingInterchangeAndMessage(declaration, string.Empty, "MESSAGE2REFERENCE", "1000");
			var messages = declaration.Messages;

			var receivedMessage = messages.AddNew();
			receivedMessage.EM_ApplicationReference = "RECEIVEDMESSAGEREFERENCE";
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			CombineAssertions(() =>
			{
				AssertEquals(3, messages.Count);
				AssertEquals(2, messages.NumberOfOutgoingMessages);
				AssertEquals("MESSAGE2REFERENCE", EMCSHelper.GetApplicationReferenceFromLastOutgoingMessage(messages));
			});
		}

		static EDIMessage SetupOutgoingInterchangeAndMessage(EMCSJobDeclaration declaration, string eHubTrackingId, string messageNum)
		{
			return SetupOutgoingInterchangeAndMessage(declaration, eHubTrackingId, string.Empty, messageNum);
		}

		static EDIMessage SetupOutgoingInterchangeAndMessage(EMCSJobDeclaration declaration, string eHubTrackingId, string applicationReference, string messageNum, string outgoingMessageStatus = EDIMessage.Status.Acknowledged)
		{
			var factory = declaration.Factory;
			var outgoingInterchange = factory.New<EDIInterchange>();
			outgoingInterchange.EI_SessionGUID = new ZGuid(eHubTrackingId);

			var outgoingMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(EMCSOutboundEDIMessage));
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ApplicationReference = applicationReference;
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(factory, EDIMessage.ApplicationCodes.GbCustomsEMCS);
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = outgoingMessageStatus;
			outgoingMessage.EM_MessageNum = messageNum;

			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Acknowledged;
			outgoingInterchange.EI_From = "AR1";
			outgoingInterchange.EI_To = "GOD";
			outgoingInterchange.EI_BodyText = "";

			declaration.Messages.Add(outgoingMessage);
			factory.Save();
			return (outgoingMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B0001000";
			declaration.JE_HouseBill = "TESTHOUSE";
		}

		EMCSJobDeclaration declaration;
		const string ResponseHeaderPlaceHolder = "<<<ResponseHeader>>>";
		const string ResponseBodyPlaceholder = "<<<ResponseBody>>>";
		const string EHubTrackingIdPlaceholder = "<<eHubTrackingId>>";
		const string JobNumberPlaceholder = "<<JobNumber>>";
		const string ServiceReferencePlaceholder = "<<ServiceReference>>";
		const string InterchangeBodyText = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <<<ResponseHeader>>>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    <<<ResponseBody>>>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string ValidMessage = @"[{""encodedMessage"":""PG5zMTpJRTgzNyB4bWxucz0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL05ld01lc3NhZ2VzRGF0YS8zIgp4bWxuczp0bXM9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6VE1TOlYzLjAxIgp4bWxuczpuczE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMDEiCnhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiPgo8bnMxOkhlYWRlcj48L25zMTpIZWFkZXI+CjxuczE6Qm9keT48L25zMTpCb2R5Pgo8L25zMTpJRTgzNz4="",""messageType"":""IE837"",""createdOn"":""2024-02-07T14:36:56.054536Z""}]";
	}
}
