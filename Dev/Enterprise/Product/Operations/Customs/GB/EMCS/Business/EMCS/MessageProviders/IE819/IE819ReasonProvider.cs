using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE819ReasonProvider : IEMCSReason
	{
		public IE819ReasonProvider(IAlertOrRejectReason alertOrRejectReason)
		{
			this.alertOrRejectReason = Argument.NotNull(alertOrRejectReason, nameof(alertOrRejectReason));
		}
		readonly IAlertOrRejectReason alertOrRejectReason;

		public string ReasonCode => alertOrRejectReason.Reason;

		public ITextAndLanguage ComplementaryInformation => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(alertOrRejectReason.Information));
		ITextAndLanguage complementaryInformation;
	}
}
