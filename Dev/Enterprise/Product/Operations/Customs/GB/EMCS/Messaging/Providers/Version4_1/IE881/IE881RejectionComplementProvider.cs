using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881RejectionComplementProvider : ITextAndLanguage
	{
		public static IE881RejectionComplementProvider NewOrNull(LsdManualClosureRejectionComplementType rejectionComplement)
			=> rejectionComplement != null ? new IE881RejectionComplementProvider(rejectionComplement) : null;

		IE881RejectionComplementProvider(LsdManualClosureRejectionComplementType rejectionComplement)
		{
			this.rejectionComplement = rejectionComplement;
		}
		readonly LsdManualClosureRejectionComplementType rejectionComplement;

		public string Text => rejectionComplement.Value;

		public string Language => rejectionComplement.Language;
	}
}
