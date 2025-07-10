using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class MailboxRequestUnpackerTest : TestCaseWithFactory
	{
		public void TestInvalidBoundary()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = "Invalid Data";

			var logger = new LoggingInformation();
			var unpacker = new MailboxRequestUnpacker(logger);

			var unpackResult = unpacker.Unpack(interchange);
			AssertEquals(0, interchange.ContainedMessages.Count);
			AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
			AssertEquals("No valid boundary found.", unpackResult.ErrorReason);
		}

		public void TestNoAttachments()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyData = GetEmbeddedFileData("ICS2MailboxRequestResponseMessageNoAttachments.mime");

			var logger = new LoggingInformation();
			var unpacker = new MailboxRequestUnpacker(logger);

			var unpackResult = unpacker.Unpack(interchange);
			AssertEquals(0, interchange.ContainedMessages.Count);
			AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
			AssertEquals("No valid zip attachment found.", unpackResult.ErrorReason);
		}

		public void TestValidResponse()
		{
			var sessionGuid = Guid.NewGuid();

			var originalInterchange = Factory.NewWithPrimaryKey<EDIInterchange>(Guid.Parse("b6eb5556-2364-4531-a766-5c6a88e79be2"));
			originalInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			originalInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			originalInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			originalInterchange.EI_InterchangeNum = "ICS22023000";
			originalInterchange.EI_SessionGUID = sessionGuid;

			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_EI = originalInterchange.PK;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IC2;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "ICS2TST00001";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			outgoingMessage.EM_LinkedObject = manifestHeader;

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			interchange.EI_InterchangeType = EUICS2InterchangeTypeList.Codes.MailboxRequest;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_SessionGUID = Guid.NewGuid();

			AssertNotEquals("Precondition", originalInterchange.EI_SessionGUID, interchange.EI_SessionGUID);

			interchange.EI_BodyData = GetEmbeddedFileData("ICS2MailboxRequestResponseMessage.mime");

			var logger = new LoggingInformation();

			var unpacker = new MailboxRequestUnpacker(logger);
			var unpackResult = unpacker.Unpack(interchange);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull("Should create a new message from the interchange data.", interchange.ContainedMessages.FirstOrDefault());
				AssertEquals("Create 1 message(s) from ICS22023001.", logger.Logs.Single().Message);

				Assert("Success", unpackResult.IsSuccess);
				var ediMessages = unpackResult.EdiMessages;
				AssertEquals("A new EdiMessage created", 1, ediMessages.Count);
				var message = ediMessages.Single();

				AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.IC2, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_GB", manifestHeader.AMA_GB, message.EM_GB);
				AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);

				AssertEquals("EM_LinkTable", manifestHeader.TableName, message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", manifestHeader.PK, message.EM_LinkUniqueID);

				AssertEquals("EM_MessageSubType", EUICS2InterchangeTypeList.Codes.MailboxRequest, message.EM_MessageSubType);
				AssertEquals("EM_MessageType - Should pick the correct message type from the attachment.", "R01", message.EM_MessageType);

				var reader = new Customs.Business.Testing.TestFileReader(typeof(MailboxRequestUnpackerTest));
				var xml = reader.GetEmbeddedFileText("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.InterchangeProcessors.TestFiles", "ICS2MailboxRequestResponseMessage.xml");
				AssertXMLEquals("EM_MessageText - Should decompress and extract the xml content from the attachment.", xml, message.EM_MessageText);
			});
		}

		byte[] GetEmbeddedFileData(string filename)
		{
			var reader = new Customs.Business.Testing.TestFileReader(typeof(MailboxRequestUnpackerTest));
			return reader.GetEmbeddedFileData("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.InterchangeProcessors.TestFiles", filename);
		}
	}
}
