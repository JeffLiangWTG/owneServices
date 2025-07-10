using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CNS
{
	public class CnsDeclarationMessageSender : Chief.ChiefDeclarationMessageSender
	{
		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction)
		{
			return new CnsTransmissionMessageGenerator(declarationMessageFunction);
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
			var cnsThreshold = GBCustomsDataRegistry.Instance.CnsWebServiceLockoutThreshold.Value;
			return new Chief.GenericMessagingHarness.CredentialsAndBadgeChecker(sendMessagesToCustoms).CredentialsExistForBadgeAndAreNotLockedOut(declaration.JE_CustomsProfile, cnsThreshold, GatewayList.Codes.CNS_CUSDECOnly);
		}
	}
}
