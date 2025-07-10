using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaIEAutoValidationMessageSenderTest : FRAutoValidationMessageSenderTest
	{
		protected override ZString MessageType => DeltaIESendMessageSubTypeList.Codes.PresentationNotification;

		protected override ZString CandidateEntriesRequiredStatus => DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;

		protected override ZString UnsuitableEntryStatus => DeltaIEImportCusEntryStatusList.Codes.Amending;

		protected override bool IsUCC6 => true;
	}
}
