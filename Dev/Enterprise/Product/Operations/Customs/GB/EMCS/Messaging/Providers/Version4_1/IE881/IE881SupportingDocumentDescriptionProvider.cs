using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881SupportingDocumentDescriptionProvider : ITextAndLanguage
	{
		public static IE881SupportingDocumentDescriptionProvider NewOrNull(LsdSupportingDocumentDescriptionType supportingDocumentDescription)
			=> supportingDocumentDescription != null ? new IE881SupportingDocumentDescriptionProvider(supportingDocumentDescription) : null;

		IE881SupportingDocumentDescriptionProvider(LsdSupportingDocumentDescriptionType supportingDocumentDescription)
		{
			this.supportingDocumentDescription = supportingDocumentDescription;
		}
		readonly LsdSupportingDocumentDescriptionType supportingDocumentDescription;

		public string Text => supportingDocumentDescription.Value;

		public string Language => supportingDocumentDescription.Language;
	}
}
