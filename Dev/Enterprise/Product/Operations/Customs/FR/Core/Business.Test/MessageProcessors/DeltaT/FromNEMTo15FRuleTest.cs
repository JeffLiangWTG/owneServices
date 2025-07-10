using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FromNEMTo15FRuleTest : TestCaseWithFactory
	{
		public void TestFromAntToVAARule()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();

			var sendCustom = new FRSendNCTSMessageProcessorForTest(header);
			Assert(!sendCustom.AutoSendNctsMessageRules.Any(x => x.CanSendMessage(header)));

			header.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.BoardingNotification;
			sendCustom = new FRSendNCTSMessageProcessorForTest(header);
			Assert(sendCustom.AutoSendNctsMessageRules.Any(x => x.CanSendMessage(header)));

			var rule = sendCustom.AutoSendNctsMessageRules.FirstOrDefault();
			AssertType<NctsMessageFunctionSet.DeclarationDataMessage>(rule.NewMessageFunction);
		}
	}
}
