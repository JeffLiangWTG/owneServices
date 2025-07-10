using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class StatementMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessage()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER1";
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.DAY, "F9A9FB22");

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer.PK;
			statement.B2_EntryFilerCode = "DGI002";
			statement.B2_CheckNo = "AUPK";
			statement.B2_ImporterCustomsID = "FR4021885690004";
			statement.B2_PaymentType = "R";
			statement.B2_PeriodStartDate = new ZDate(2021, 08, 01);
			statement.B2_StatementType = "J";
			statement.B2_BranchDesignation = "IMP";

			var errorCollector = new EU.Business.ErrorCollector();
			var objectToSend = new StatementMessageSendingObject(statement);
			new StatementMessageSender(objectToSend, errorCollector).Send();

			var message = statement.Messages[0];
			AssertEquals(StatementStatusList.Codes.PendingResponse, statement.B2_Status);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(MessageTypeList.Codes.DCG, message.EM_MessageType);
			AssertEquals(MessageTypeList.Codes.DCG, message.EM_MessageSubType);
			AssertContains("<schemaID>MessageDcg</schemaID>", message.EM_MessageText);
		}

		public void TestFailToSendMessage()
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "IMPORTER1";
			importer1.OH_IsConsignee = true;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer1.PK;
			statement.B2_EntryFilerCode = "DGI002";
			statement.B2_CheckNo = "AUPK";
			statement.B2_ImporterCustomsID = "FR4021885690004";
			statement.B2_PaymentType = "R";
			statement.B2_PeriodStartDate = new ZDate(2021, 08, 01);
			statement.B2_StatementType = "J";
			statement.B2_BranchDesignation = "IMP";
			statement.B2_Status = StatementStatusList.Codes.Incomplete;

			var errorCollector1 = new EU.Business.ErrorCollector();
			var objectToSend1 = new StatementMessageSendingObject(statement);
			new StatementMessageSender(objectToSend1, errorCollector1).Send();

			AssertEquals("No message was sent.", 0, statement.Messages.Count);
			AssertEquals("Status was reverted.", StatementStatusList.Codes.Incomplete, statement.B2_Status);

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "IMPORTER2";
			importer2.OH_IsConsignee = true;
			importer2.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.DAY, "E2068819");
			importer2.SetupCusCode(OrgCusCode.CodeTypes.BrokerageRegistration, "CBR Number");
			statement.B2_OH_Importer = importer2.PK;

			var errorCollector2 = new EU.Business.ErrorCollector();
			var objectToSend2 = new StatementMessageSendingObject(statement);
			new StatementMessageSender(objectToSend2, errorCollector2).Send();
			var message = statement.Messages[0];
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(MessageTypeList.Codes.DCG, message.EM_MessageType);
			AssertEquals(MessageTypeList.Codes.DCG, message.EM_MessageSubType);
			AssertContains("<schemaID>MessageDcg</schemaID>", message.EM_MessageText);
			AssertEquals("Status was updated to PND.", StatementStatusList.Codes.PendingResponse, statement.B2_Status);
		}
	}
}
