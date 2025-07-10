using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC415BImportOperationWrapper : ICC415BCciOperation
	{
		CC415BImportOperationWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.declaration = Argument.NotNull(entryHeader.Declaration, nameof(declaration));
			this.entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryInstruction));
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;

		public string LRN => lrn ?? (lrn = entryHeader.CorrelationID);
		string lrn;

		public string DeclarationType => declarationType ?? (declarationType = declaration.JE_EntryStyle);
		string declarationType;

		public string AdditionalDeclarationType => additionalDeclarationType ?? (additionalDeclarationType = entryInstruction.CEI_SubStyle);
		string additionalDeclarationType;

		public string PresentationNotificationEstimateDateAndTime => presentationNotificationEstimateDateAndTime ?? (presentationNotificationEstimateDateAndTime = entryInstruction.CEI_DateForDuty.ToString("yyyy-MM-ddTHH:mm:ss"));
		string presentationNotificationEstimateDateAndTime;

		public string LanguageCode => languageCode ?? (languageCode = declaration.JE_DeclarationLanguage);
		string languageCode;

		public static CC415BImportOperationWrapper New(CusEntryHeader entryHeader) => entryHeader == null ? null : new CC415BImportOperationWrapper(entryHeader);
	}
}
