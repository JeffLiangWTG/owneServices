using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN.Testing
{
	public class CIN745ExportMessageBuilderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			myCIN745MessageExportBuilder = CreateMessageBuilderMock();
			itemErrorCollector = new EU.Business.ErrorCollector();
		}
		public void TestGetMessage()
		{
			ZString messageUtf8StartValid = @"<?xml version=""1.0"" encoding=""utf-8""?><Message";

			string generatedExpMessage = myCIN745MessageExportBuilder.GetMessage();

			AssertEquals(true, generatedExpMessage.StartsWith(messageUtf8StartValid));
			AssertContains("<EnveloppeMessage>" +
"<schemaID>MessageCINExp_745</schemaID><schemaVersion>XML</schemaVersion><transactionId>94321-B00175768</transactionId>" +
 "</EnveloppeMessage>" +
 "<EnveloppeCIN>" +
 "<OACI>PR2</OACI><REFERENCE>19301050800349</REFERENCE><MRN_ECS>19FRD3260086362767</MRN_ECS><MAGASIN>ORYSF2</MAGASIN><BUR_DOUANE>FR003370</BUR_DOUANE><DEST_OACI>TAR</DEST_OACI>" +
 "<EDIFACTCIN>" +
 "<Message745>" +
 "<EnvelopeConnexion>" +
 "<date>" + ZDate.Today.ToString("yyyy-MM-dd") + "</date><time>" + ZDateTime.Today.ToString("hh:mm:ss") + "</time><sender>PR2</sender>" +
 "</EnvelopeConnexion>" +
 "<Messages>" +
 "<Message>" +
 "<EnvelopeMessage><schemaId>ID</schemaId><schemaVersion>3</schemaVersion><transactionId>BGM_reference</transactionId>" +
 "</EnvelopeMessage>" +
 "<MessageBody>" +
 "<Header>" +
 "<MessageFrom>FR00050</MessageFrom><MessageTo>FR00060</MessageTo>" +
 "</Header>" +
 "<MRNS>" +
 "<MRN>" +
 "<Level>lvl</Level><Name>name</Name><MrnQuantity>1</MrnQuantity><MrnWeight>2.5</MrnWeight><MrnNumber>123456987</MrnNumber><ExitOfOffice>FR00040</ExitOfOffice>" +
 "</MRN>" +
 "</MRNS>" +
 "</MessageBody>" +
 "</Message>" +
 "</Messages>" +
 "</Message745>" +
 "</EDIFACTCIN>" +
 "</EnveloppeCIN>" +
 "</Message>", generatedExpMessage);
		}

		CIN745ExportMessageBuilder CreateMessageBuilderMock()
		{
			var messageMock = new Mock<ICIN745ExportMessage>();

			#region MessageEnvelope properties mock

			var myMessageEnvelop = new Mock<IMessageEnvelope>();

			myMessageEnvelop.Setup(m => m.SchemaID).Returns("MessageCINExp_745");
			myMessageEnvelop.Setup(m => m.SchemaVersion).Returns("XML");
			myMessageEnvelop.Setup(m => m.TransactionId).Returns("94321-B00175768");
			messageMock.Setup(m => m.MessageEnvelope).Returns(myMessageEnvelop.Object);

			#endregion
			#region CINMessageEnvelop properties mock

			var mycinMessageEnvelop = new Mock<ICINNested745Message>();

			mycinMessageEnvelop.Setup(m => m.OACI).Returns("PR2");
			mycinMessageEnvelop.Setup(m => m.MAGASIN).Returns("ORYSF2");
			mycinMessageEnvelop.Setup(m => m.REFERENCE).Returns("19301050800349");
			mycinMessageEnvelop.Setup(m => m.MRN_ECS).Returns("19FRD3260086362767");
			mycinMessageEnvelop.Setup(m => m.BUR_DOUANE).Returns("FR003370");
			mycinMessageEnvelop.Setup(m => m.DEST_OACI).Returns("TAR");
			mycinMessageEnvelop.Setup(m => m.Level).Returns("lvl");
			mycinMessageEnvelop.Setup(m => m.Name).Returns("name");
			mycinMessageEnvelop.Setup(m => m.MrnNumber).Returns("123456987");
			mycinMessageEnvelop.Setup(m => m.MrnWeight).Returns(2.5);
			mycinMessageEnvelop.Setup(m => m.MrnQuantity).Returns(1);
			mycinMessageEnvelop.Setup(m => m.ExitOfOffice).Returns("FR00040");
			mycinMessageEnvelop.Setup(m => m.OACI_Carrier).Returns("FR00050");
			mycinMessageEnvelop.Setup(m => m.OACI_Shipper).Returns("FR00060");
			mycinMessageEnvelop.Setup(m => m.SchemaID).Returns("ID");
			mycinMessageEnvelop.Setup(m => m.SchemaVersion).Returns("3");
			mycinMessageEnvelop.Setup(m => m.TransactionID).Returns("BGM_reference");
			mycinMessageEnvelop.Setup(m => m.Time).Returns(new ZDateTime(ZDateTime.Today.ToShortTimeString()));
			mycinMessageEnvelop.Setup(m => m.Date).Returns(ZDate.Today);

			messageMock.Setup(m => m.MessageCIN745).Returns(mycinMessageEnvelop.Object);

			#endregion
			var messageBuilderMocked = new CIN745ExportMessageBuilder(messageMock.Object, itemErrorCollector, myTransType);

			return (CIN745ExportMessageBuilder)Convert.ChangeType(messageBuilderMocked, typeof(CIN745ExportMessageBuilder));
		}

		readonly TransactionTypes myTransType = TransactionTypes.Original;
		CIN745ExportMessageBuilder myCIN745MessageExportBuilder;
		EU.Business.ErrorCollector itemErrorCollector;
	}
}
