using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881RequestReasonCodeComplementProvider : ITextAndLanguage
	{
		public static IE881RequestReasonCodeComplementProvider NewOrNull(LsdManualClosureRequestReasonCodeComplementType requestReasonCodeComplement)
			=> requestReasonCodeComplement != null ? new IE881RequestReasonCodeComplementProvider(requestReasonCodeComplement) : null;

		IE881RequestReasonCodeComplementProvider(LsdManualClosureRequestReasonCodeComplementType requestReasonCodeComplement)
		{
			this.requestReasonCodeComplement = requestReasonCodeComplement;
		}
		readonly LsdManualClosureRequestReasonCodeComplementType requestReasonCodeComplement;

		public string Text => requestReasonCodeComplement.Value;

		public string Language => requestReasonCodeComplement.Language;
	}
}
