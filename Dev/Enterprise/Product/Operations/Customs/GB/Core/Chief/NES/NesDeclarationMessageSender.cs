using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Chief.NES
{
	public class NesDeclarationMessageSender : ChiefDeclarationMessageSender
	{
		public override IMessageGenerator<CusEntryHeader> GetTransmissionGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
		{
			return new NesTransmissionMessageGenerator(declarationMessageFunction);
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return new[]
				{
					NesConstants.Code
				};
		}

		protected override JobDeclarationMessageManager GetDeclarationManager(IMessageGenerator<CusEntryHeader> transmissionGenerator, JobDeclaration declaration)
		{
			return new Business.GBJobDeclarationMessageManager(declaration, transmissionGenerator);
		}

		public override bool CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(JobDeclaration declaration, Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return new GenericMessagingHarness.CredentialsAndBadgeChecker(sendMessagesToCustoms).CredentialsExistForBadgeAndAreNotLockedOut(declaration.JE_CustomsProfile, -1, GatewayList.Codes.NES);
		}
	}
}
