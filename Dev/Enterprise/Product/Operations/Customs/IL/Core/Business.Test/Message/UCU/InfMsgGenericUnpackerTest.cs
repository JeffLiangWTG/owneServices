using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(InfMsgGenericUnpacker))]
	sealed class InfMsgGenericUnpackerTest : TestCaseWithFactory
	{
		public void TestWhenInterchangeTypeIncorrect()
		{
			var interchange = CreateTestInterchange("INF", "INF_MSG_Generic_BusinessError_Interchange.xml");
			var unpacker = new InfMsgGenericUnpacker();
			var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
			var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, ZString.Empty, null, null, new LoggingInformation());

			Assert("Unpack should fail", !unpackResult.IsSuccess);
			AssertEquals("Error reason should be as expected", "The type of interchange is incorrect.", unpackResult.ErrorReason);
		}

		public void TestWhenStatusNotSuccess()
		{
			var interchange = CreateTestInterchange("GEN", "INF_MSG_Generic_BusinessError_Interchange.xml");
			var unpacker = new InfMsgGenericUnpacker();
			var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
			var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, ZString.Empty, null, null, new LoggingInformation());

			Assert("Unpack should fail", !unpackResult.IsSuccess);
			AssertEquals("Error reason should be as expected", "The status of the message is not success.", unpackResult.ErrorReason);
		}

		public void TestWhenStatusSuccess()
		{
			var dcaParameters = new DCAParameters() { AllServices = false };
			dcaParameters.Services.Add(new DCAService() { Name = "SendMN_MSG1171_SendManifestFeedBack_Message" });
			using (ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters))
			{
				var interchange = CreateTestInterchange("GEN", "INF_MSG_Generic_Success_Interchange.xml");
				var unpacker = new InfMsgGenericUnpacker();
				var messageBody = ILBusinessTestHelper.GetMessageBody(interchange.EI_BodyText);
				var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(interchange.EI_BodyText);
				var unpackResult = unpacker.Unpack(interchange, messageBody, messageResponseHeader, ZString.Empty, null, null, new LoggingInformation());

				Assert("Unpack should succeed", unpackResult.IsSuccess);

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, "GEN");
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "910");
				var messages = interchange.Factory.Load<EDIMessage>(query);

				AssertNotNull("Messages were created", messages);
				AssertEquals("There was 1 message created", 1, messages.Length);
			}
		}

		ILEDIInterchange CreateTestInterchange(string messageType, string interchangeXmlFileName)
		{
			var interchange = Factory.NewWithValidTestData<ILEDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;
			interchange.EI_InterchangeType = messageType;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_Status = ILEDIInterchange.Status.Queued;
			interchange.EI_BodyText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath(interchangeXmlFileName));
			return interchange;
		}
	}
}
