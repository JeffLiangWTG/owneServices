using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IL;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILInterchangeUnpacker))]
	sealed class ILInterchangeUnpackerTest : InterchangeUnpackerTest<ILInterchangeUnpacker>
	{
		public void TestDeliveryOrder()
		{
			var expectedMessageText = LoadEmbeddedResource("DeliveryOrderResponse_1220_result");
			var interchange = CreateInterchange("DeliveryOrderResponse_1220_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Message Type", ILMessageTypeList.Codes.DLO, message.EM_MessageType);
				AssertEquals("Message SubType", ILEDIMessageSubTypeList.Codes.DeliveryOrderResponse, message.EM_MessageSubType);
				AssertEquals("Message Text", expectedMessageText, message.EM_MessageText);
			});
		}

		public void TestGatePassMovement()
		{
			var expectedMessageText = LoadEmbeddedResource("GatePassMovementResponse_1035");
			var interchange = CreateInterchange("GatePassMovementResponse_1035_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Message Type", ILMessageTypeList.Codes.GPM, message.EM_MessageType);
				AssertEquals("Message SubType", ILEDIMessageSubTypeList.Codes.GatepassMovementResponse, message.EM_MessageSubType);

				AssertEquals("Message Text", expectedMessageText, message.EM_MessageText);
			});
		}

		public void TestImportDeclaration()
		{
			var expectedMessageText = LoadEmbeddedResource("ImportDeclarationResponse_2754");
			var interchange = CreateInterchange("ImportDeclarationResponse_2754_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);
			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Message Type", ILMessageTypeList.Codes.DEC, message.EM_MessageType);
				AssertEquals("Message SubType", ILEDIMessageSubTypeList.Codes.ImportDeclarationResponse, message.EM_MessageSubType);
				XmlComparison.CompareAndAssertXml(expectedMessageText, message.EM_MessageText, "Message text should match");
			});
		}

		public void TestManifest()
		{
			var expectedMessageText = LoadEmbeddedResource("Manifest_1171");
			var interchange = CreateInterchange("Manifest_1171_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Message Type", ILMessageTypeList.Codes.MAN, message.EM_MessageType);
				AssertEquals("Message SubType", "171", message.EM_MessageSubType);
				AssertEquals("Message Text", expectedMessageText, message.EM_MessageText);
			});
		}

		public void TestManifestQuery()
		{
			var expectedMessageText = LoadEmbeddedResource("ManifestQueryResponse_8241");
			var interchange = CreateInterchange("ManifestQueryResponse_8241_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Message Type", ILMessageTypeList.Codes.MAN, message.EM_MessageType);
				AssertEquals("Message SubType", "821", message.EM_MessageSubType);
				AssertEquals("Message Text", expectedMessageText, message.EM_MessageText);
			});
		}

		public void TestInterchangeBody_InValidXML()
		{
			var interchange = Factory.New<ILEDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = "text";
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
				AssertEquals("Error", "The interchange body text is not valid XML.", unpackResult.ErrorReason);
			});
		}

		public void TestInvalidFeedbackMessageName()
		{
			var interchange = CreateInterchange("InvalidFeedbackMessageName_Interchange");

			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Error", "The interchange body text does not contain a valid feedback message name. Extracted value: MN_NG_1222_MSG23_DeliveryOrderFeedBack_Message", unpackResult.ErrorReason);
				AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
			});
		}

		public void TestOutgoingMessageResponseUnpacker_WhenOneMessageMappedFromThree()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var companyWrapper = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(currentCompany);
			var externalPassword = companyWrapper.GetGlbExternalPasswordOrCreateNew();
			externalPassword.GP_MailBoxID = "560038416";
			currentCompany.Factory.Save();

			var expectedMessageText = LoadEmbeddedResource("OutgoingMessageResponse_9101_WhenOneMessageMappedFromThree_result");
			var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenOneMessageMappedFromThree_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions("Xml include DF_NG_8251_Web02_DeclarationStatus_Response, MN_NG_1200_MSG2_DeliveryOrder_Message and MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message", () =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("while MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message is mapped", 1, unpackResult.EdiMessages.Count);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Message Type", ILMessageTypeList.Codes.DLO, message.EM_MessageType);
				AssertEquals("Message SubType", ILEDIMessageSubTypeList.Codes.DeliveryOrderResponse, message.EM_MessageSubType);
				AssertEquals("Message Text", expectedMessageText, message.EM_MessageText);
			});
		}

		public void TestOutgoingMessageResponseUnpacker_WhenAllMessagesNotMapped()
		{
			var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenAllMessagesNotMapped_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions("Xml include DF_NG_8251_Web02_DeclarationStatus_Response, and MN_NG_1200_MSG2_DeliveryOrder_Message Not Mapped", () =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("No Message created", 0, unpackResult.EdiMessages.Count);
			});
		}

		public void TestOutgoingMessageResponseUnpacker_WhenHasNotQ1PrefixAndRowNumbersIsZero()
		{
			var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenRowNumbersIsZero_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions("When Row Numbers Is Zero", () =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("No Message created", 0, unpackResult.EdiMessages.Count);
			});
		}

		public void TestOutgoingMessageResponseUnpacker_WhenHasQ1Prefix()
		{
			var interchange = CreateInterchange("OutgoingMessageResponse_9101_WhenHasQ1Prefix_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions("When Xml Has q1 Prefix and include 3 MN_MSG4_SendManifestFeedBack_Message that are Mapped", () =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("Message created", 4, unpackResult.EdiMessages.Count);
			});
		}

		public void TestCorrelationIdFetchedAndStored()
		{
			var interchange = CreateInterchange("DeliveryOrderResponse_1220_Interchange");
			var unpacker = new ILInterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions("A DeliveryOrderResponse should be created with correct correlation Id", () =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("Message created", 1, unpackResult.EdiMessages.Count);
				var ediMessage = unpackResult.EdiMessages.Single();
				AssertEquals("EM_ApplicationReference equals to correlation Id", "7a420c90-d2e9-450b-ab93-57c7c11ed2be", ediMessage.EM_ApplicationReference);
			});
		}

		public void TestXTERRInterchange()
		{
			const string interchangeMinimumBodyText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Body>
  <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  </UniversalEvent>
</Body>
</UniversalInterchange>
";
			var factory = Factory;

			var shipment = factory.New<ForwardingShipment>();
			var sentMessage = ILBusinessTestHelper.CreateMessage(factory, ILMessageTypeList.Codes.XER);
			sentMessage.EM_MessageSubType = "130";
			sentMessage.EM_LinkedObject = shipment;
			var logger = new LoggingInformation();
			var packer = new ILMessagePacker();
			var sentInterchange = factory.New<ILEDIInterchange>();
			_ = packer.Pack(sentMessage, sentInterchange, logger);

			var receivedInterchange = factory.New<ILEDIInterchange>();
			receivedInterchange.EI_InterchangeNum = "ICS22023001";
			receivedInterchange.EI_BodyText = interchangeMinimumBodyText;
			receivedInterchange.EI_InterchangeType = ILMessageTypeList.Codes.XER;
			receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;
			factory.Save();

			var unpacker = new ILInterchangeUnpacker();
			var unpackResult = unpacker.Unpack(receivedInterchange, null, null, logger);

			CombineAssertions("A UniversalEvent EDI Message should be created", () =>
			{
				Assert("Succeed to unpack", unpackResult.IsSuccess);
				AssertEquals("Message created", 1, unpackResult.EdiMessages.Count);
				var ediMessage = unpackResult.EdiMessages.Single();
				AssertEquals("EM_MessageType is XER", "XER", ediMessage.EM_MessageType);
			});
		}

		ILEDIInterchange CreateInterchange(string embeddedResourceResponseName)
		{
			var interchange = Factory.New<ILEDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = LoadEmbeddedResource(embeddedResourceResponseName);
			return interchange;
		}

		static ZString LoadEmbeddedResource(string embeddedResourceResponseName)
		{
			return new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath($"{embeddedResourceResponseName}.xml"));
		}

		protected override string[] ApplicationCodes => new[] { EDIInterchange.ApplicationCodes.ILCustoms };
	}
}
