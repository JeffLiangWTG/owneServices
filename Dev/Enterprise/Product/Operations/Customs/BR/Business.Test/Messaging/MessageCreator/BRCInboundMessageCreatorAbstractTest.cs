using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business.Testing
{
	abstract class BRCInboundMessageCreatorAbstractTest : TestCaseWithFactory
	{
		protected abstract ZString InterchangeType { get; }

		protected virtual ZString TransportType => EDIInterchange.TransportType.xT;

		protected EDIInterchange ProcessEDIInterchange(ZString messageText, ZGuid sessionGuid = default, ZGuid branchPK = default)
		{
			var interchange = Factory.NewWithValidTestData<BREDIInterchange>();
			interchange.EI_From = "BRCustoms.TEST";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.BRCustoms;
			interchange.EI_InterchangeType = InterchangeType;
			interchange.EI_TransportType = TransportType;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_HeaderText = "{\"custom.OriginalMsgId\":\"\",\"custom.MessageSubType\":\"\"}";
			interchange.EI_BodyText = messageText;
			interchange.EI_SessionGUID = sessionGuid.IsDefault ? ZGuid.NewZGuid() : sessionGuid;
			interchange.EI_InterchangeNum = Guid.NewGuid().ToString("N");
			if (branchPK.IsValid)
			{
				interchange.EI_GB = branchPK;
			}
			Factory.Save();

			var processor = new BRCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.BRCustoms });
			processor.ExecuteBatch();
			logger = processor.Logger;
			interchange.Reload();
			return interchange;
		}

		protected LoggingInformation logger;

		protected void AssertCreateMessageFromInterchange(ZString messageText, ZString expectedMessageText, string expectedMessageType = null, string expectedMessageSubType = "XXX", ZGuid outgoingInterchangeSessionGuid = default)
		{
			var interchange = ProcessEDIInterchange(messageText, outgoingInterchangeSessionGuid);
			CombineAssertions(() =>
			{
				AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);
			});
			AssertEDIMessageCreated(interchange.ContainedMessages[0], expectedMessageText, expectedMessageType, expectedMessageSubType);
		}

		protected void AssertEDIMessageCreated(EDIMessage message, ZString expectedMessageText, string expectedMessageType = null, string expectedMessageSubType = "XXX", string messageNum = null)
		{
			CombineAssertions(() =>
			{
				var interchange = message.Interchange;
				AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", expectedMessageType ?? InterchangeType, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIInterchangeStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_EI", interchange.PK, message.EM_EI);
				AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_MessageNum", messageNum ?? interchange.EI_InterchangeNum, message.EM_MessageNum);
				AssertXMLEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
			});
		}

		protected ZString CreateUniversalInterchangeXml(string eventType = Events.InterchangeAcknowledgedCode, string messageType = null, string responseType = null, string reason = null, string responseMessage = null)
		{
			return UniversalEventTestDataHelper.CreateUniversalInterchangeXml(eventType, messageType ?? InterchangeType, responseType, reason, responseMessage);
		}

		protected string EncodeAndZipMessages(params string[] messages)
		{
			var inputStreams = messages.Select((m, i) => CreateZipStream($"JsonFile_{i++}.JsOn", m))
				.Append(CreateZipStream("NotAJsonFile.Txt", "NOT A JSON FILE!!!")).ToArray();

			using var outputStream = new MemoryStream();
			new ZipCreator().ZipStream(inputStreams, outputStream);
			inputStreams.ForEach(x => x.Stream.Dispose());
			return Convert.ToBase64String(outputStream.ToArray());

			static ZipStream CreateZipStream(string filename, string content)
			{
				return new ZipStream(filename, new MemoryStream(Encoding.UTF8.GetBytes(content)));
			}
		}
	}
}
