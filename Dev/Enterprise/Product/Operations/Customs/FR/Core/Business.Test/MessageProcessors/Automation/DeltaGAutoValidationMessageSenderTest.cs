using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaGAutoValidationMessageSenderTest : FRAutoValidationMessageSenderTest
	{
		protected override ZString MessageType => EntryActionCodeList.Codes.VAA;

		protected override ZString CandidateEntriesRequiredStatus => EntryStatusDescriptionCodeList.Codes.ES050;

		protected override ZString UnsuitableEntryStatus => EntryStatusDescriptionCodeList.Codes.ES060;

		protected override bool IsUCC6 => false;
	}
}
