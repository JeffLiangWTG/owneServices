using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class SendEAVMessageWhenStatusIs055Test : DeltaGAutoSendCustomsMessageProcessorTest<SendEAVMessageWhenStatusIs055>
	{
		protected override ZString OriginalEntryStatusForProcessing => EntryStatusDescriptionCodeList.Codes.ES055;

		protected override ZString UpdatedEntryStatusAfterProcessed => EntryStatusDescriptionCodeList.Codes.ES055;
	}
}
