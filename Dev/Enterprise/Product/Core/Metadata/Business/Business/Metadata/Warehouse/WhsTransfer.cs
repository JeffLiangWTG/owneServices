using Enterprise.Metadata.Integration;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.WhsTransfer)]
	public class WhsTransfer : WhsDocket
	{
		protected override bool SupportsUnmatchedOrgNoteType => false;
		protected override bool SupportsUnrecognisedAdditionalReferenceType => false;
	}
}
