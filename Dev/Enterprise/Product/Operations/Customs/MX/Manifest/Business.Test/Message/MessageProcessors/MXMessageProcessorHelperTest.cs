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
	sealed class MXMessageProcessorHelperTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookForAsycudaBillResponse()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();

			var seaFirstResponseMessage = CreateSeaResponseMessage(bill.PK, MessageTypes.Codes.MXA, GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFirstResponse)));
			var seaFinalResponseMessage = CreateSeaResponseMessage(bill.PK, MessageTypes.Codes.MXD, GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFinalResponse)));

			var processor = new MXBranchMessageProcessor { Logger = new LoggingInformation() };
			processor.ExecuteBatch();

			seaFirstResponseMessage.Reload();
			seaFinalResponseMessage.Reload();

			AssertMessage(seaFirstResponseMessage, bill);
			AssertMessage(seaFinalResponseMessage, bill);
		}

		void AssertMessage(EDIMessage message, AsycudaBill bill)
		{
			CombineAssertions(() =>
			{
				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			return header;
		}

		EDIMessage CreateSeaResponseMessage(ZGuid billPK, ZString messageType, ZString bodyText)
		{
			var requestInterchange = CreateInterchange(ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MessageTypes.Codes.MXA, MXMessageConstants.MXCustomsForSeaMode);
			var requestMessage = CreateMessage(bodyText, requestInterchange.PK, billPK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXA);
			return CreateResponseMessage(requestInterchange.EI_SessionGUID, billPK, messageType, MXMessageConstants.MXCustomsForSeaMode, bodyText);
		}

		EDIMessage CreateResponseMessage(ZGuid sessionGUID, ZGuid billPK, ZString messageType, ZString eiFrom, ZString bodyText)
		{
			var responseInterchange = CreateInterchange(sessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, messageType, eiFrom);
			return CreateMessage(bodyText, responseInterchange.PK, billPK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, messageType);
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

		MXMessage CreateMessage(ZString bodyText, ZGuid interchangePK, ZGuid billPK, ZString direction, ZString status, ZString messageType)
		{
			var message = Factory.New<MXMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			message.EM_ApplicationReference = "MAN0000001";
			message.EM_MessageNum = "000000001";
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;
			message.EM_EI = interchangePK;
			message.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			message.EM_LinkUniqueID = billPK;

			Factory.Save();
			return message;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			var doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}
	}
}
