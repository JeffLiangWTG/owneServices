using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class Send415MessageWhenStatusIsEmptyTest : DeltaIEAutoSendCustomsMessageProcessorTest<Send415MessageWhenStatusIsEmpty>
	{
		protected override ZString OriginalEntryStatusForProcessing => ZString.Empty;

		protected override ZString UpdatedEntryStatusAfterProcessed => EntryStatusDescriptionCodeList.Codes.ES010;
	}
}
