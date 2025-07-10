using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Metadata.Integration
{
	public interface ICommonShipmentLinkedMetadata
	{
		IMetadata[] DeclarationsForNoteTypes { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Metadata architecture requires array")]
		IMetadata[] MetadataExtensions { get; set; }
	}
}
