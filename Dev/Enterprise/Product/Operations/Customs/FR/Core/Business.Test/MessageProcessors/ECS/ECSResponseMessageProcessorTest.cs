using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class ECSResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessagesProcessed_Valid()
		{
			var helper = new ECSMessageTestHelper();
			var exitDetail1 = helper.CreateCusExitDetail(Factory, "T1", "ECS-MRN001", "TTT");
			var exitDetail2 = helper.CreateCusExitDetail(Factory, "T2", "ECS-MRN002", "TTT");
			var exitDetail3 = helper.CreateCusExitDetail(Factory, "T3", "ECS-MRN003", "TTT");
			var exitDetail4 = helper.CreateCusExitDetail(Factory, "T4", "ECS-MRN004", "TTT");

			var ediMessage1 = helper.CreateFREDIMessage(Factory, GlbBranch.CurrentBranch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, "",
				EDIMessageStatusList.Codes.Queued, helper.CreateEIMessageBodyWithReponseDatasNode("MessageReponseIE507", "ECS-MRN001", "ARRIVE DEST", ZDate.Today));
			var ediMessage2 = helper.CreateFREDIMessage(Factory, GlbBranch.CurrentBranch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, "",
				EDIMessageStatusList.Codes.Queued, helper.CreateEIMessageBodyWithReponseDatasNode("MessageReponseIE507", "ECS-MRN002", "SORTIE EFFECTIVE", ZDate.Today));
			var ediMessage3 = helper.CreateFREDIMessage(Factory, GlbBranch.CurrentBranch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, "",
				EDIMessageStatusList.Codes.Queued, helper.CreateEIMessageBodyWithReponseEtatNode("MessageNotificationEtat", "ECS-MRN003", "ARRIVE DEST", ZDate.Today));
			var ediMessage4 = helper.CreateFREDIMessage(Factory, GlbBranch.CurrentBranch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, "",
				EDIMessageStatusList.Codes.Queued, helper.CreateEIMessageBodyWithReponseEtatNode("MessageNotificationEtat", "ECS-MRN004", "SORTIE EFFECTIVE", ZDate.Today));

			Factory.Save();

			var processor = new ECSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage1);
			processor.ProcessMessage(ediMessage2);
			processor.ProcessMessage(ediMessage3);
			processor.ProcessMessage(ediMessage4);
			Factory.Save();

			AssertEquals(exitDetail1.PK, ediMessage1.EM_LinkedObject.PK);
			AssertEquals(exitDetail2.PK, ediMessage2.EM_LinkedObject.PK);
			AssertEquals(exitDetail3.PK, ediMessage3.EM_LinkedObject.PK);
			AssertEquals(exitDetail4.PK, ediMessage4.EM_LinkedObject.PK);
			exitDetail1.Reload();
			exitDetail2.Reload();
			exitDetail3.Reload();
			exitDetail4.Reload();

			helper.AssertCusExitDetail(exitDetail1, "ARR", false, ZDate.Empty);
			helper.AssertCusExitDetail(exitDetail2, "EXT", true, ZDate.Today);
			helper.AssertCusExitDetail(exitDetail3, "ARR", false, ZDate.Empty);
			helper.AssertCusExitDetail(exitDetail4, "EXT", true, ZDate.Today);
		}
	}
}
