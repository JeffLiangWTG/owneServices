using Enterprise.Metadata.Integration;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.DummyBO)]
	public class DummyBO : EnterpriseBusinessObject, IDummyBOShareProperty, IDummyBOLinkedMetadata
	{
		public bool IsDummy { get; set; }

		public IMetadata LinkedBO { get; set; }
		public IMetadata[] LinkedBOs { get; set; }
	}
}

