using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	class CSAManualReleaseStrategy : PreSaveDialogStrategy
	{
		public CSAManualReleaseStrategy(JobDeclaration source)
		{
			this.source = source;
		}
		readonly JobDeclaration source;

		protected override ContinueWithSave RunPreSaveAction()
		{
			var manualReleaseBO = new ManualReleaseCancelBO(source.ManualReleaseText, source.Factory);
			manualReleaseBO.ManualReleaseDate = source.JE_EntryAuthorisationDate;
			manualReleaseBO.ManualReleaseReason = EDIReleaseImportEntryStatusList.Descriptions.AuthorisedToDeliver;
			source.ManualReleaseText = manualReleaseBO.NoteText;
			if (source.ReleaseEntryHeader is CusEntryHeader entryHeader)
			{
				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver;
				entryHeader.CH_EntryReleaseDate = source.JE_EntryAuthorisationDate;
			}
			source.JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver;
			return ContinueWithSave.Yes;
		}

		protected override bool ShouldRunPreSaveAction()
		{
			return source != null && source.IsCSA
				&& !source.JE_EntryAuthorisationDate.IsEmpty
				&& source.JE_EntryStatus != EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver;
		}
	}
}
