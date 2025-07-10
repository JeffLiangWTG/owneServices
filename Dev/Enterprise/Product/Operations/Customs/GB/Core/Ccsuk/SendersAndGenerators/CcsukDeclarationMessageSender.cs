using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Ccsuk.ServiceTask;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Ccsuk.Declaration
{
	public class CcsukDeclarationMessageSender : ChiefDeclarationMessageSender
	{
		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
		{
			return new CcsukTransmissionMessageGenerator(declarationMessageFunction);
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return new string[]
				{
						CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode,
						Enterprise.Customs.GB.Chief.CusRes.CusResAndDtiResponseProcessorServiceCode.Code,
						CcsukServiceTaskConstants.CcsukServiceTaskCode
				};
		}

		public override bool CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(EU.Business.Declaration.JobDeclaration declaration, Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return new Chief.GenericMessagingHarness.CredentialsAndBadgeChecker(sendMessagesToCustoms).CredentialsExistForBadgeAndAreNotLockedOut(declaration.JE_CustomsProfile, -1, GatewayList.Codes.CCSUKviaNTMsgGW);
		}
	}
}
