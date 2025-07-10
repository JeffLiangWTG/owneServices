using System.Linq;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2InterchangeUnpacker))]
	sealed class ICS2InterchangeUnpackerTest : InterchangeUnpackerTest<ICS2InterchangeUnpacker>
	{
		public void TestUnpackMailboxRequest()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = "Invalid Data";
			interchange.EI_InterchangeType = EUICS2InterchangeTypeList.Codes.MailboxRequest;

			var unpacker = new ICS2InterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				AssertEquals(0, interchange.ContainedMessages.Count);
				AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
				AssertEquals("No valid boundary found.", unpackResult.ErrorReason);
			});
		}

		public void TestUnpackNormalAcknowledgement()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = "Invalid Data";
			interchange.EI_InterchangeType = "TST";

			var unpacker = new ICS2InterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				AssertEquals(1, interchange.ContainedMessages.Count);
				AssertEquals(1, unpackResult.EdiMessages.Count);
				Assert("Success", unpackResult.IsSuccess);

				var ediMessage = unpackResult.EdiMessages.Single();
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, ediMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
				AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, ediMessage.EM_MessageNum);
				AssertEquals("EM_MessageType", "TST", ediMessage.EM_MessageType);
				AssertEquals("EM_MessageData", interchange.GetEI_BodyDataReader().ToByteArray(), ediMessage.EM_MessageData);
			});
		}

		public void TestUnpackXTFailure()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = "<UniversalEvent>Test</UniversalEvent>";
			interchange.EI_InterchangeType = Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;

			var unpacker = new ICS2InterchangeUnpacker();
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Interchange contained messages count", 1, interchange.ContainedMessages.Count);
				AssertEquals("Unpack result", expected: true, unpackResult.IsSuccess);

				var ediMessage = unpackResult.EdiMessages.Single();
				AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.IC2, ediMessage.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, ediMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
				AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, ediMessage.EM_MessageNum);
				AssertEquals("EM_MessageType", interchange.EI_InterchangeType, ediMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", interchange.EI_InterchangeType, ediMessage.EM_MessageSubType);
				AssertEquals("EM_MessageData", interchange.GetEI_BodyDataReader().ToByteArray(), ediMessage.EM_MessageData);
				AssertEquals("EM_IsActive", expected: true, ediMessage.EM_IsActive);
			});
		}

		protected override string[] ApplicationCodes => new[] { EDIInterchange.ApplicationCodes.IC2 };
	}
}
