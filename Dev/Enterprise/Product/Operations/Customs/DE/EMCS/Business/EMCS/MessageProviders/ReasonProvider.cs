using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ReasonProvider : IEMCSReason
	{
		public ReasonProvider(ReportOfReceiptReason emcsReason)
		{
			this.emcsReason = Argument.NotNull(emcsReason, nameof(emcsReason));
		}
		readonly ReportOfReceiptReason emcsReason;

		public string ReasonCode => emcsReason.CY_Code;

		public ITextAndLanguage ComplementaryInformation => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(emcsReason.CY_Data));
		ITextAndLanguage complementaryInformation;
	}
}
