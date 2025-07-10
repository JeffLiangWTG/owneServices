using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED819ReasonProvider : IEMCSReason
	{
		public ED819ReasonProvider(IAlertOrRejectReason alertOrRejectReason)
		{
			this.alertOrRejectReason = Argument.NotNull(alertOrRejectReason, nameof(alertOrRejectReason));
			helper = new Message819ReasonProviderHelper(alertOrRejectReason);
		}
		readonly IAlertOrRejectReason alertOrRejectReason;
		readonly Message819ReasonProviderHelper helper;

		public string ReasonCode => helper.ReasonCode;

		public ITextAndLanguage ComplementaryInformation => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(alertOrRejectReason.Information));
		ITextAndLanguage complementaryInformation;
	}
}
