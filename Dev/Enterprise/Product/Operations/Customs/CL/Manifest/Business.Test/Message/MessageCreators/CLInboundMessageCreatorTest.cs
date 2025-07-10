using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class CLInboundMessageCreatorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var creator = new CLInboundMessageCreator();
			var request = CreateMessage();

			var seaAcceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaAcceptedResponse));
			var interchange = CreateInterchange(MessageTypes.Codes.CHB, CLMessageConstants.CLCustomsForSeaMode, seaAcceptedResponse, "IMP-BL-1.0-0000000001.xml");
			creator.CreateMessagesForInterchange(interchange);
			var createdInterchange = interchange;
			createdInterchange.Reload();

			AssertMessage(createdInterchange, request);

			request.EM_MessageNum = "0000000002";
			var airAcceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.AirAcceptedResponse));
			interchange = CreateInterchange(MessageTypes.Codes.CHE, CLMessageConstants.CLCustomsForAirMode, airAcceptedResponse, "IMP-AWB-1.0-0000000002.xml");
			creator.CreateMessagesForInterchange(interchange);
			createdInterchange = interchange;
			createdInterchange.Reload();

			AssertMessage(createdInterchange, request);
		}

		void AssertMessage(EDIInterchange interchange, CLMessage request)
		{
			var message = interchange.ContainedMessages[0];
			AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
			AssertNotNull("EM_GE", message.EM_GE);
			AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength), message.EM_MessageNum);
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_MessageText", interchange.EI_BodyText, message.EM_MessageText);
			AssertEquals("EM_MessageType", request.EM_MessageType, message.EM_MessageType);
			AssertEquals("EM_LinkTable", request.EM_LinkTable, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", request.EM_LinkUniqueID, message.EM_LinkUniqueID);
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString from, ZString bodyText, ZString fileName)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CLCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = from;
			interchange.EI_To = CLMessageConstants.EHub;
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_HeaderText = $"{{\"custom.FileName\":\"{fileName}\",\"custom.ClientID\":\"HYECHLCMT_SMS\"}}";

			Factory.Save();
			return interchange;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}

		CLMessage CreateMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var request = Factory.New<CLMessage>();
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			request.EM_GB = GlbBranch.CurrentBranch.PK;
			request.EM_IsTestMessage = true;
			request.EM_MessageNum = "0000000001";
			request.EM_MessageText = ZString.Empty;
			request.EM_MessageType = MessageTypes.Codes.CHB;
			request.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			request.EM_Status = EDIMessageStatusList.Codes.Sent;
			request.EM_LinkUniqueID = bill.PK;
			request.EM_LinkTable = AsycudaBill.Schema.TableName;
			request.EM_LinkedObject = bill;

			Factory.Save();
			return request;
		}
	}
}
