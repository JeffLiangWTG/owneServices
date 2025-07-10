using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN.Testing
{
	public class CIN755ExportMessageBuilderTest : TestCaseWithFactory
	{
		readonly TransactionTypes myTransType = TransactionTypes.Original;
		CIN755ExportMessageBuilder myCIN755MessageExportBuilder;
		const string edifactCinMessage = @"UNH+203661200<<MSGNO PLACEHOLDER>>+755:2";

		protected override void SetUp()
		{
			itemErrorCollector = new EU.Business.ErrorCollector();
			myCIN755MessageExportBuilder = CreateMessageBuilderMock<CIN755ExportMessageBuilder>();
		}
		public void TestGetMessage()
		{
			ZString messageUtf8StartValid = @"<?xml version=""1.0"" encoding=""utf-8""?><Message";

			string generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();

			AssertEquals(true, generatedExpMessage.StartsWith(messageUtf8StartValid));
		}

		public void TestPopulateMessageEnvelop()
		{
			string generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();

			var messageExpEnvelopPart = @"<EnveloppeMessage><schemaID>MessageCINExp_755</schemaID><schemaVersion>EDIFACT</schemaVersion><transactionId>94321-B00175768</transactionId></EnveloppeMessage>";

			AssertContains(messageExpEnvelopPart, generatedExpMessage);
		}
		public void TestPopulateMessageCINEnvelop()
		{
			string generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();

			var messageExpCINEnvelopPart = @"<EnveloppeCIN><OACI>PR2</OACI><REFERENCE>19301050800349</REFERENCE><MRN_ECS>19FRD3260086362767</MRN_ECS><MAGASIN>ORYSF2</MAGASIN><BUR_DOUANE>FR003370</BUR_DOUANE><DEST_OACI>TAR</DEST_OACI><NUM_LTA>05786602865</NUM_LTA>"
										+ "<EDIFACTCIN>UNH+203661200";

			AssertContains(messageExpCINEnvelopPart, generatedExpMessage);
		}

		public void TestNUM_LTAIsEmpty()
		{
			itemErrorCollector.WipeErrors();
			string generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();
			AssertEquals("", itemErrorCollector.GetErrorsAsString());

			mycinMessageEnvelop.Setup(m => m.NUM_LTA).Returns("");
			var errorMessage = "MAWB is empty.";
			generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();
			AssertContains(errorMessage, itemErrorCollector.GetErrorsAsString());
		}

		public void TestMRN_ECSIsEmpty()
		{
			itemErrorCollector.WipeErrors();
			string generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();
			AssertEquals("", itemErrorCollector.GetErrorsAsString());

			mycinMessageEnvelop.Setup(m => m.MRN_ECS).Returns("");
			var errorMessage = "MRN is empty.";
			generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();
			AssertContains(errorMessage, itemErrorCollector.GetErrorsAsString());
		}

		public void TestMAGASINIsEmpty()
		{
			itemErrorCollector.WipeErrors();
			string generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();
			AssertEquals("", itemErrorCollector.GetErrorsAsString());

			mycinMessageEnvelop.Setup(m => m.MAGASIN).Returns("");
			var errorMessage = "MAGASIN is empty.";
			generatedExpMessage = myCIN755MessageExportBuilder.GetMessage();
			AssertContains(errorMessage, itemErrorCollector.GetErrorsAsString());
		}

		T CreateMessageBuilderMock<T>()
		{
			var messageMock = new Mock<ICIN755ExportMessage>();

			#region MessageEnvelop properties mock

			var myMessageEnvelop = new Mock<IMessageEnvelope>();

			myMessageEnvelop.Setup(m => m.SchemaID).Returns("MessageCINExp_755");
			myMessageEnvelop.Setup(m => m.SchemaVersion).Returns("EDIFACT");
			myMessageEnvelop.Setup(m => m.TransactionId).Returns("94321-B00175768");
			messageMock.Setup(m => m.MessageEnvelope).Returns(myMessageEnvelop.Object);

			#endregion

			#region CINMessageEnvelop properties mock

			mycinMessageEnvelop = new Mock<ICINNested755Envelope>();

			mycinMessageEnvelop.Setup(m => m.OACI).Returns("PR2");
			mycinMessageEnvelop.Setup(m => m.REFERENCE).Returns("19301050800349");
			mycinMessageEnvelop.Setup(m => m.MRN_ECS).Returns("19FRD3260086362767");
			mycinMessageEnvelop.Setup(m => m.MAGASIN).Returns("ORYSF2");
			mycinMessageEnvelop.Setup(m => m.BUR_DOUANE).Returns("FR003370");
			mycinMessageEnvelop.Setup(m => m.DEST_OACI).Returns("TAR");
			mycinMessageEnvelop.Setup(m => m.NUM_LTA).Returns("05786602865");
			mycinMessageEnvelop.Setup(m => m.CIN).Returns(edifactCinMessage);

			messageMock.Setup(m => m.NestedCINMessageEnvelope).Returns(mycinMessageEnvelop.Object);

			#endregion
			var messageBuilderMocked = new CIN755ExportMessageBuilder(messageMock.Object, itemErrorCollector, myTransType);

			return (T)Convert.ChangeType(messageBuilderMocked, typeof(T));
		}

		EU.Business.ErrorCollector itemErrorCollector;
		Mock<ICINNested755Envelope> mycinMessageEnvelop;
	}
}
