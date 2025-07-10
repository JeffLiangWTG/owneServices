using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	sealed class MXMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessageEmptyAndInvalidBody()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();

			var seaFirstMessageResponse = CreateResponseMessage(bill.PK, MessageTypes.Codes.MXA, "BODY");
			var seaFinalMessageResponse = CreateResponseMessage(bill.PK, MessageTypes.Codes.MXD, "");

			processor.ExecuteBatch();
			seaFirstMessageResponse.Reload();
			seaFinalMessageResponse.Reload();

			AssertEquals(EDIMessageStatusList.Codes.Discarded, seaFirstMessageResponse.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Discarded, seaFinalMessageResponse.EM_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessBillNotFound()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();

			var firstSeaBody = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFirstResponse));
			var seaFirstMessageResponse = CreateResponseMessage(bill.PK, MessageTypes.Codes.MXA, firstSeaBody, false);
			processor.ExecuteBatch();
			seaFirstMessageResponse.Reload();
			AssertEquals(EDIMessageStatusList.Codes.Failed, seaFirstMessageResponse.EM_Status);

			var finalSeaBody = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFinalResponse));
			var seaFinalMessageResponse = CreateResponseMessage(bill.PK, MessageTypes.Codes.MXD, finalSeaBody, false);
			processor.ExecuteBatch();
			seaFinalMessageResponse.Reload();
			AssertEquals(EDIMessageStatusList.Codes.Failed, seaFinalMessageResponse.EM_Status);
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			return header;
		}

		EDIMessage CreateResponseMessage(ZGuid billPK, ZString messageType, ZString bodyText, bool isBOlinked = true)
		{
			var requestInterchange = CreateInterchange(new ZGuid("834C5994-DA19-4CB2-97C6-61B375F7452D"), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MessageTypes.Codes.MXA, MXMessageConstants.MXCustomsForSeaMode);
			var requestMessage = CreateMessage(bodyText, requestInterchange.PK, billPK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXA);
			requestMessage.EM_MessageNum = "1996";

			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, messageType, MXMessageConstants.MXCustomsForSeaMode);
			return CreateMessage(bodyText, responseInterchange.PK, billPK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, messageType, isBOlinked);
		}

		MXInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status, ZString interchangeType, ZString eiFrom)
		{
			var interchange = Factory.New<MXInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;

			Factory.Save();
			return interchange;
		}

		MXMessage CreateMessage(ZString bodyText, ZGuid interchangePK, ZGuid billPK, ZString direction, ZString status, ZString messageType, bool isBOlinked = true)
		{
			var message = Factory.New<MXMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;
			message.EM_EI = interchangePK;

			if (isBOlinked)
			{
				message.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
				message.EM_LinkUniqueID = billPK;
			}

			Factory.Save();
			return message;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			var doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}

		readonly MXBranchMessageProcessor processor = new MXBranchMessageProcessor { Logger = new LoggingInformation() };
	}
}
