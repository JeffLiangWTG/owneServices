using System.Xml;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class MessageHelperTest : TestCaseWithFactory
{
	public void TestValidateMessageXML()
	{
		var xml = "<element>value</element>";
		var (errorMessage, xmlDoc) = MessageHelper.ValidateMessageXML(xml);

		CombineAssertions(() =>
		{
			AssertEquals("Valid XML", string.Empty, errorMessage);
			AssertEquals("Valid XML", "value", xmlDoc.SelectSingleNode("//element[1]").InnerText);

			xml = string.Empty;
			(errorMessage, _) = MessageHelper.ValidateMessageXML(xml);
			AssertEquals("Empty XML", "The Message XML is Empty", errorMessage);

			xml = "<element>value</wrongelement>";
			(errorMessage, _) = MessageHelper.ValidateMessageXML(xml);
			AssertEquals("Invalid XML", "The Message XML is not a valid XML", errorMessage);
		});
	}

	public void TestGetMessageTypeByXml()
	{
		var xmlDocument = new XmlDocument();

		CombineAssertions(() =>
		{
			xmlDocument.LoadXml(@"<?xml version=""1.0"" encoding=""UTF-8""?><CC004C><messageType>CC004C</messageType></CC004C>");
			AssertEquals("CC004C", "004", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<AcknowledgementMessage><correlationId>correlationId</correlationId></AcknowledgementMessage>");
			AssertEquals("AcknowledgementMessage", "ACK", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<CC928C><correlationId>correlationId</correlationId></CC928C>");
			AssertEquals("CC928C", "928", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<ns2:CC928C xmlns:ns2=\"http://ecs.dgtaxud.ec\"><correlationId>correlationId</correlationId></ns2:CC928C>");
			AssertEquals("CC928C with namespace", "928", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<UniversalInterchange><Event>Event</Event></UniversalInterchange>");
			AssertEquals("Customs Service Error Universal Event", "UER", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<CCxxxC><WrongElement>CC007C</WrongElement></CCxxxC>");
			AssertEquals("Wrong Element", "", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<CCxxxC><messageType>AI44E</messageType></CCxxxC>");
			AssertEquals("Invalid MessageType", "", MessageHelper.GetMessageTypeByXml(xmlDocument));

			xmlDocument.LoadXml("<IE906><messageType>IE906</messageType></IE906>");
			AssertEquals("IE906", "906", MessageHelper.GetMessageTypeByXml(xmlDocument));
		});
	}

	public void TestLocateEntryHeaderByLRNFallbackToMRN()
	{
		var entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BEE00000000012J1");
		Factory.Save();
		CombineAssertions(() =>
		{
			AssertNull("Empty", MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(new BusinessObjectFactory(), GetInboundProvider(ZString.Empty, ZString.Empty)));
			AssertEquals("By MRN", entry.PK, MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(new BusinessObjectFactory(), GetInboundProvider(ZString.Empty, "22BEE00000000012J1")).PK);
			AssertEquals("By LRN", entry.PK, MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(new BusinessObjectFactory(), GetInboundProvider("22045281480600000001", ZString.Empty)).PK);
		});
	}

	public void TestLocateHeaderByEdiInterchange()
	{
		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		Factory.Save();
		var sessionGUID = ZGuid.BrettsGuid;
		var ediMessageBadStatus = Factory.New<BEMessage>();
		ediMessageBadStatus.EM_Status = "FAL";
		var ediMessageOld = Factory.New<BEMessage>();
		ediMessageOld.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
		var ediMessage = Factory.New<BEMessage>();
		ediMessage.EM_Status = "SNT";
		ediMessage.EM_LinkTable = cusEntryHeader.TableName;
		ediMessage.EM_LinkUniqueID = cusEntryHeader.PK;
		var interchangeOutgoing = Factory.New<BECInterchange>();
		interchangeOutgoing.EI_SessionGUID = sessionGUID;
		interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
		interchangeOutgoing.EI_From = "DEJOS";
		interchangeOutgoing.EI_To = "DEFRANS";
		ediMessage.EM_EI = interchangeOutgoing.PK;
		var interchangeIncoming = Factory.New<BECInterchange>();
		interchangeIncoming.EI_SessionGUID = sessionGUID;
		interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		interchangeIncoming.EI_From = "DEFRANS";
		interchangeIncoming.EI_To = "DEJOS";
		Factory.Save();
		AssertEquals(cusEntryHeader.PK, MessageHelper.LocateHeaderByEdiInterchange(interchangeIncoming).PK);
	}

	public void TestLocateHeaderByLRNOrMRNFallbackInterchange()
	{
		var entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BE000000000012J1");

		var mockProvider = new Mock<IInboundProvider>();
		Factory.Save();

		var sessionGUID = ZGuid.BrettsGuid;
		var ediMessageOld = Factory.New<BEMessage>();
		ediMessageOld.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
		var ediMessage = Factory.New<BEMessage>();
		ediMessage.EM_Status = "SNT";
		ediMessage.EM_LinkTable = entry.TableName;
		ediMessage.EM_LinkUniqueID = entry.PK;
		var interchangeOutgoing = Factory.New<BECInterchange>();
		interchangeOutgoing.EI_SessionGUID = sessionGUID;
		interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
		interchangeOutgoing.EI_From = "DEJOS";
		interchangeOutgoing.EI_To = "DEFRANS";
		ediMessage.EM_EI = interchangeOutgoing.PK;
		var interchangeIncoming = Factory.New<BECInterchange>();
		interchangeIncoming.EI_SessionGUID = sessionGUID;
		interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		interchangeIncoming.EI_From = "DEJOS";
		interchangeIncoming.EI_To = "DEFRANS";
		Factory.Save();

		CombineAssertions(() =>
		{
			mockProvider.Setup(m => m.LRN).Returns(string.Empty);
			mockProvider.Setup(m => m.MRN).Returns(string.Empty);
			AssertEquals("By Interchange", entry.PK, MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

			interchangeIncoming.EI_SessionGUID = new ZGuid();
			mockProvider.Setup(m => m.LRN).Returns(string.Empty);
			mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
			AssertEquals("By MRN", entry.PK, MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

			mockProvider.Setup(m => m.LRN).Returns("123");
			mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
			AssertEquals("By MRN", entry.PK, MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

			mockProvider.Setup(m => m.MRN).Returns(string.Empty);
			mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
			AssertEquals("By LRN", entry.PK, MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

			mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
			mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
			AssertEquals("By LRN", entry.PK, MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);
		});
	}

	static IInboundProvider GetInboundProvider(ZString lrn, ZString mrn)
	{
		var mockProvider = new Mock<ICC556CDataProvider>();
		mockProvider.Setup(m => m.LRN).Returns(lrn);
		mockProvider.Setup(m => m.MRN).Returns(mrn);
		return mockProvider.Object;
	}
}
