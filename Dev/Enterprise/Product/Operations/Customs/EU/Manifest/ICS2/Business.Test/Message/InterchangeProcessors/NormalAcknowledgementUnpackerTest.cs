using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class NormalAcknowledgementUnpackerTest : TestCaseWithFactory
	{
		public void TestUnpack_ValidResponse_PopulateEM_MessageData()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			interchange.EI_InterchangeType = "TST";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_SessionGUID = Guid.NewGuid();

			var messageData = ics2EmptyQueryResponseData;
			interchange.EI_BodyData = messageData;
			var unpackResult = new NormalAcknowledgementUnpacker().Unpack(interchange);
			Factory.Save();
			CombineAssertions(() =>
			{
				var message = unpackResult.EdiMessages.Single();
				Assert("Success", unpackResult.IsSuccess);
				AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.IC2, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_MessageType", "TST", message.EM_MessageType);
				AssertEquals("EM_MessageData", messageData, message.EM_MessageData);
				AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);
				AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
				AssertSame("Message linked to the interchange", message, interchange.ContainedMessages.Single());
			});
		}

		public void TestUnpack_ValidResponse_PopulateEM_MessageText()
		{
			using (SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
				interchange.EI_InterchangeType = "TST";
				interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;
				interchange.EI_InterchangeNum = "ICS22023001";
				interchange.EI_SessionGUID = Guid.NewGuid();

				var messageData = ics2EmptyQueryResponseData;
				interchange.EI_BodyData = messageData;
				var unpackResult = new NormalAcknowledgementUnpacker().Unpack(interchange);
				Factory.Save();
				CombineAssertions(() =>
				{
					var message = unpackResult.EdiMessages.Single();
					Assert("Success", unpackResult.IsSuccess);
					AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.IC2, message.EM_ApplicationCode);
					AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
					AssertEquals("EM_MessageType", "TST", message.EM_MessageType);
					AssertEquals("EM_MessageText", MessageEncoding.UTF8WithoutBOM.GetString(messageData), message.EM_MessageText);
					AssertEquals("EM_MessageData", ZBlob.Empty, message.EM_MessageData);
					AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);
					AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
					AssertSame("Message linked to the interchange", message, interchange.ContainedMessages.Single());
				});
			}
		}

		public void TestUnpack_MessageNumberTruncation()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "ICS220230019374294293842394872394873298";
			interchange.EI_BodyData = ics2EmptyQueryResponseData;

			var unpacker = new NormalAcknowledgementUnpacker();
			var unpackResult = unpacker.Unpack(interchange);

			var message = unpackResult.EdiMessages.Single();
			CombineAssertions(() =>
			{
				Assert("Success", unpackResult.IsSuccess);
				AssertEquals("Message Number Truncated", "ICS22023001937429429384239487239487", message.EM_MessageNum);
			});
		}

		byte[] ics2EmptyQueryResponseData => ics2EmptyQueryResponseDataCached ?? (ics2EmptyQueryResponseDataCached = EUICS2MessageTestHelper.ICS2EmptyQueryResponseData);
		byte[] ics2EmptyQueryResponseDataCached;
	}
}
