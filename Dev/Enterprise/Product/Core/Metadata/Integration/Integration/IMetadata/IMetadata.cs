using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Metadata.Integration
{
	public interface IMetadata
	{
		INoteTypeCollection NoteTypes { get; }
	}
}
