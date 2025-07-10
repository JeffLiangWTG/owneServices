using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	sealed class MXInboundMessageCreatorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var seaBodyText = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaRejectedWithEnvelopeFirstResponse));
			var interchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MXMessageConstants.MXCustomsForSeaMode, MessageTypes.Codes.MXA, seaBodyText);
			var creator = new MXInboundMessageCreator();
			creator.CreateMessagesForInterchange(interchange);
			interchange.Reload();

			AssertMessage(interchange);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLinkRequestSeaErrorMessageByTrackingId()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			var bill = header.Bills.AddNew();

			var headerTextErrorNotification = GetExpectedMessageTXT(Path.Combine(BaseSourcePath, MXMessagingConstants.HeaderTextErrorNotification));

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MXMessageConstants.MXCustomsForSeaMode, MessageTypes.Codes.MXD, "");
			var requestMessage = CreateMessage("", requestInterchange.PK, bill.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXD);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MXMessageConstants.MXCustomsForSeaMode, MXMessageConstants.XER, headerTextErrorNotification);

			var creator = new MXInboundMessageCreator();
			creator.CreateMessagesForInterchange(responseInterchange);
			responseInterchange.Reload();

			var responseMessage = responseInterchange.ContainedMessages[0];

			CombineAssertions(() =>
			{
				AssertEquals(requestMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
				AssertEquals(requestMessage.EM_LinkTable, responseMessage.EM_LinkTable);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLinkRequestAirMessageByTrackingId()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			var bill = header.Bills.AddNew();

			var firstAirBody = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirAcceptedFirstResponse));
			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MXMessageConstants.MXCustomsForAirMode, MessageTypes.Codes.MXE, "");
			var requestMessage = CreateMessage(firstAirBody, requestInterchange.PK, bill.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXE);
			var airBodyText = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirAcceptedFirstResponse));
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MXMessageConstants.MXCustomsForAirMode, MessageTypes.Codes.MXF, airBodyText);

			var creator = new MXInboundMessageCreator();
			creator.CreateMessagesForInterchange(responseInterchange);
			responseInterchange.Reload();

			var responseMessage = responseInterchange.ContainedMessages[0];

			CombineAssertions(() =>
			{
				AssertEquals(requestMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
				AssertEquals(requestMessage.EM_LinkTable, responseMessage.EM_LinkTable);
			});
		}

		void AssertMessage(MXInterchange interchange)
		{
			var message = interchange.ContainedMessages[0];

			CombineAssertions(() =>
			{
				AssertEquals(message.EM_GB, interchange.EI_GB);
				AssertNotNull(message.EM_GE);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
				AssertEquals(message.EM_EI, interchange.PK);
			});
		}

		MXInterchange CreateInterchange(ZString direction, ZString status, ZString eiFrom, ZString interchangeType, ZString bodyText)
		{
			var interchange = Factory.New<MXInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = "eHub";
			if (interchangeType == MXMessageConstants.XER)
			{
				interchange.EI_HeaderText = bodyText;
			}
			else
			{
				interchange.EI_BodyText = bodyText;
			}
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = new ZGuid("834C5994-DA19-4CB2-97C6-61B375F7452D");

			Factory.Save();
			return interchange;
		}

		MXMessage CreateMessage(ZString bodyText, ZGuid interchangePK, ZGuid billPK, ZString direction, ZString status, ZString messageType)
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

			if (direction == EDIMessage.Direction.Transmit)
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

		public static ZString GetExpectedMessageTXT(ZString path)
		{
			StreamReader sr = new StreamReader(path);
			return sr.ReadToEnd();
		}
	}
}
