using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class SendVALMessageWhenStatusIsEmptyTest : DeltaGAutoSendCustomsMessageProcessorTest<SendVALMessageWhenStatusIsEmpty>
	{
		protected override ZString OriginalEntryStatusForProcessing => ZString.Empty;

		protected override ZString UpdatedEntryStatusAfterProcessed => EntryStatusDescriptionCodeList.Codes.ES010;
	}
}
