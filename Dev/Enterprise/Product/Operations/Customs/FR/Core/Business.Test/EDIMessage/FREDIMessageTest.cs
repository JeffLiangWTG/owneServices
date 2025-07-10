using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(FREDIMessage))]
	sealed class FREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var message = Factory.New<FREDIMessage>();
			AssertType<FREDIMessageValidation>(message.Validation);
		}

		public void TestOnSaving()
		{
			var message = Factory.New<FREDIMessage>();
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			message.EM_MessageText = "&lt;&lt;MSGNO PLACEHOLDER&gt;&gt; + <<MSGNO PLACEHOLDER>>";
			Factory.Save();
			AssertEquals("1 + 1", message.EM_MessageText);
		}

		public void TestPopulateCorrelationIDOnSaving()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			statement.CorrelationID = "COREL006";
			var message = Factory.New<FREDIMessage>();
			message.EM_LinkedObject = statement;
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			message.EM_MessageText = "~CORRELATIONID~&&&&&~CORRELATIONID~";
			Factory.Save();
			AssertEquals("COREL006&&&&&COREL006", message.EM_MessageText);
		}

		public void TestSetDefaultValues()
		{
			var message = Factory.New<FREDIMessage>();
			AssertEquals(FREDIMessage.ApplicationCodes.FRCustomsMessage, message.EM_ApplicationCode);
		}

		public void TestTestEM_MessageInterpretation()
		{
			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageSubType = MessageSubTypeList.Codes.CIN;
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			message.EM_MessageText = messageText;

			AssertExceptionThrown<NotSupportedException>("set a non-empty interpretation is not allowed", () =>
			{
				message.EM_MessageInterpretation = "This is a manual interpretation";
			});

			message.EM_MessageText = string.Empty;

			AssertNoExceptionThrown(() =>
			{
				message.EM_MessageInterpretation = "This is a manual interpretation";
			});
		}

		public void TestMessageDataObject()
		{
			var messageIMP = Factory.New<DeltaCImportFREDIMessage>();
			messageIMP.EM_MessageType = MessageTypeList.Codes.IMC;
			messageIMP.EM_MessageSubType = MessageSubTypeList.Codes.IMC;

			AssertType<DeltaCImportResponseMessageDataObject>("Import DELTA G1 Droit Commun response message", messageIMP.MessageDataObject);

			messageIMP = Factory.New<DeltaCImportFREDIMessage>();
			messageIMP.EM_MessageText = "XXX";
			messageIMP.EM_MessageSubType = "XXX";

			var messageEXP = Factory.New<DeltaCExportFREDIMessage>();
			messageEXP.EM_MessageType = MessageTypeList.Codes.EXC;
			messageEXP.EM_MessageSubType = MessageSubTypeList.Codes.EXC;

			AssertType<DeltaCExportResponseMessageDataObject>("Import DELTA G1 Droit Commun response message", messageEXP.MessageDataObject);

			messageEXP = Factory.New<DeltaCExportFREDIMessage>();
			messageEXP.EM_MessageText = "XXX";
			messageEXP.EM_MessageSubType = "XXX";
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	public class TestEDIMessage : FREDIMessage
	{
		public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return "test";
		}

		public bool AfterNewObjectIsLinkedCalled;
		protected override void AfterNewObjectIsLinked(BusinessObject newBizObj)
		{
			AfterNewObjectIsLinkedCalled = true;
			base.AfterNewObjectIsLinked(newBizObj);
		}
	}
}
