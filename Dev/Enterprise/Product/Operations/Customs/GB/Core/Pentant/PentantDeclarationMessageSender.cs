using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Pentant
{
	public class PentantDeclarationMessageSender : Chief.ChiefDeclarationMessageSender
	{
		public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction)
		{
			return new PentantTransmissionMessageGenerator(declarationMessageFunction);
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			return [PentantConstants.Code]; // Pentant downloader
		}

		public override bool CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var threshold = GBCustomsDataRegistry.Instance.PentantWebServiceLockoutThreshold.Value;
			return new Chief.GenericMessagingHarness.CredentialsAndBadgeChecker(sendMessagesToCustoms).CredentialsExistForBadgeAndAreNotLockedOut(declaration.JE_CustomsProfile, threshold, GatewayList.Codes.Pentant);
		}
	}
}
