using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC413BImportOperationWrapper : ICC413BCciOperation
	{
		CC413BImportOperationWrapper(CusEntryHeader entryHeader)
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

		public string PresentationNotificationEstimatedDateAndTime => presentationNotificationEstimatedDateAndTime ?? (presentationNotificationEstimatedDateAndTime = entryInstruction.CEI_DateForDuty.ToString("yyyy-MM-ddTHH:mm:ss"));
		string presentationNotificationEstimatedDateAndTime;

		public string LanguageCode => languageCode ?? (languageCode = declaration.JE_DeclarationLanguage);
		string languageCode;

		public string CustomsRegistrationNumber => crn ?? (crn = entryHeader.CRN);
		string crn;

		public string MRN => mrn ?? (mrn = entryHeader.MRN) ;
		string mrn;

		public static CC413BImportOperationWrapper New(CusEntryHeader entryHeader) => entryHeader == null ? null : new CC413BImportOperationWrapper(entryHeader);
	}
}
