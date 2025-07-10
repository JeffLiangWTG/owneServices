using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE819ReasonProvider : IEMCSReason
	{
		public IE819ReasonProvider(IAlertOrRejectReason alertOrRejectReason)
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
