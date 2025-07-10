namespace Enterprise.Metadata.Integration
{
	public interface IDummyBOLinkedMetadata
	{
		IMetadata LinkedBO { get; set; }
		IMetadata[] LinkedBOs { get; set; }
	}
}
