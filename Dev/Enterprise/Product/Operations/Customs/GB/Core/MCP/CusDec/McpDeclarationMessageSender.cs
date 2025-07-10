using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.MCP
{
	public class McpDeclarationMessageSender : Chief.ChiefDeclarationMessageSender
	{
		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction)
		{
			return new McpTransmissionMessageGenerator(declarationMessageFunction);
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return new string[]
				{
					Enterprise.Customs.GB.Chief.CusRes.CusResAndDtiResponseProcessorServiceCode.Code,	// shared response understanderer
				};
		}

		public override bool CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var mcpThreshold = GBCustomsDataRegistry.Instance.McpWebServiceLockoutThreshold.Value;
			return new Chief.GenericMessagingHarness.CredentialsAndBadgeChecker(sendMessagesToCustoms).CredentialsExistForBadgeAndAreNotLockedOut(declaration.JE_CustomsProfile, mcpThreshold, GatewayList.Codes.MCP_CUSDECOnly);
		}
	}
}
