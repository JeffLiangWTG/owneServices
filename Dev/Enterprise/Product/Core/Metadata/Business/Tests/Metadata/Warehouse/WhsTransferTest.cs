#if DEBUG

using Enterprise.Metadata.Integration;

namespace Enterprise.Metadata.Business.Tests
{
	public class WhsTransferTest : WhsDocketTest
	{
		protected override bool SupportsUnmatchedOrgNoteType => false;
		protected override bool SupportsUnrecognisedAdditionalReferenceType => false;

		protected override IMetadata NewMetadata => new WhsTransfer();
	}
}

#endif
