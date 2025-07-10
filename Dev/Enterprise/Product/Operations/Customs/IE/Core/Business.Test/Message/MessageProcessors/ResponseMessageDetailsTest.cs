using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ResponseMessageDetailsTest : TestCaseWithFactory
	{
		public void TestGetResponseDetail_MessageAcknowledge_IEE_ERR()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Status.Error);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEE/ERR", () =>
			{
				AssertEquals("Processor Type", typeof(ErrorMessageProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IEI_ERR()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsImport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Status.Error);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEI/ERR", () =>
			{
				AssertEquals("Processor Type", typeof(ErrorMessageProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IE5_ERR()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsUCC5Import, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Status.Error);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IE5/ERR", () =>
			{
				AssertEquals("Processor Type", typeof(ErrorMessageProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IEM_ERR()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsEMCS, "801", EDIMessage.Status.Error);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEM/ERR", () =>
			{
				AssertEquals("Processor Type", typeof(ErrorMessageProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IEE_ACK()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, CommonInterchangeTypeList.Codes.MessageAcknowledge);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEE/ACK", () =>
			{
				AssertEquals("Processor Type", typeof(MessageAcknowledgementProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IEI_ACK()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsImport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, CommonInterchangeTypeList.Codes.MessageAcknowledge);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEI/ACK", () =>
			{
				AssertEquals("Processor Type", typeof(MessageAcknowledgementProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IE5_ACK()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsUCC5Import, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, CommonInterchangeTypeList.Codes.MessageAcknowledge);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IE5/ACK", () =>
			{
				AssertEquals("Processor Type", typeof(MessageAcknowledgementProcessor), responseDetail.ProcessorType);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IEM_ACK()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsEMCS, "801", CommonInterchangeTypeList.Codes.MessageAcknowledge);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEM/ACK", () =>
			{
				AssertEquals("Processor Type", "Enterprise.Customs.IE.EMCS.Business.MessageAcknowledgementProcessor", responseDetail.ProcessorType.FullName);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_MessageAcknowledge_IEN_ACK()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsNCTS, "928", CommonInterchangeTypeList.Codes.MessageAcknowledge);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEN/ACK", () =>
			{
				AssertEquals("Processor Type", "Enterprise.Customs.IE.NCTS.Business.MessageAcknowledgementProcessor", responseDetail.ProcessorType.FullName);
				AssertNull("XmlObjectType", responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_IEP()
		{
			var responseDetail = ResponseMessageDetails.GetResponseDetail(EDIMessage.ApplicationCodes.IECustomsPBN, "CPB", string.Empty);
			CombineAssertions("ResponseMessageDetails.GetResponseDetail should have returned correct result for IEP/CPB", () =>
			{
				AssertEquals("Processor Type", "Enterprise.Customs.IE.PBN.Business.CreateAndUpdatePBNMessageProcessor", responseDetail.ProcessorType.FullName);
				AssertEquals("Object type", typeof(CreateAndUpdatePBNMessageDefinition), responseDetail.XmlObjectType);
			});
		}

		public void TestGetResponseDetail_InvalidInputs()
		{
			AssertNoExceptionThrown("Empty inputs", () => { ResponseMessageDetails.GetResponseDetail(string.Empty, string.Empty, string.Empty); });
			AssertNoExceptionThrown("Invalid inputs", () => { ResponseMessageDetails.GetResponseDetail("XXX", "XXX", "XXX"); });
		}

		public void TestGetXmlRootNameMapping()
		{
			var mapping = new ResponseMessageDetails().GetXmlRootNameMapping();

			(var applicationCode, var messageType) = mapping[GetKey("http://ecs.dgtaxud.ec", "CC917C")];
			AssertEquals("applicationCode", EDIMessage.ApplicationCodes.IECustomsExport, applicationCode);
			AssertEquals("messageType", "917", messageType);

			(applicationCode, messageType) = mapping[GetKey("http://ncts.dgtaxud.ec", "CC917C")];
			AssertEquals("applicationCode", EDIMessage.ApplicationCodes.IECustomsNCTS, applicationCode);
			AssertEquals("messageType", "917", messageType);

			(applicationCode, messageType) = mapping[GetKey("http://www.ros.ie/schemas/customs", "IM917")];
			AssertEquals("applicationCode", EDIMessage.ApplicationCodes.IECustomsImport, applicationCode);
			AssertEquals("messageType", "917", messageType);

			(applicationCode, messageType) = mapping[GetKey("http://www.ros.ie/schemas/customs/IM415VH7", "IM415V")];
			AssertEquals("applicationCode", EDIMessage.ApplicationCodes.IECustomsImport, applicationCode);
			AssertEquals("messageType", "15V", messageType);

			(applicationCode, messageType) = mapping[GetKey("http://www.ros.ie/schemas/customs/IM917", "IM917")];
			AssertEquals("applicationCode", EDIMessage.ApplicationCodes.IECustomsUCC5Import, applicationCode);
			AssertEquals("messageType", "917", messageType);

			(applicationCode, messageType) = mapping[GetKey("urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE917:V3.13", "IE917")];
			AssertEquals("applicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, applicationCode);
			AssertEquals("messageType", "917", messageType);
		}

		string GetKey(string ns, string elementName) => ns + elementName;
	}
}
