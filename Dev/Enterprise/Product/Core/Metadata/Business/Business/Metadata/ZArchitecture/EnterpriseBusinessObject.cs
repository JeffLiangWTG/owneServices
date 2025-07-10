using Enterprise.Integration.ZArchitecture;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.EnterpriseBusinessObject)]
	public class EnterpriseBusinessObject : IMetadata, IEnterpriseBusinessObjectMetadata
	{
		public INoteTypeCollection NoteTypes
		{
			get
			{
				return predefinedNoteTypes ?? (predefinedNoteTypes = InitializeNoteTypes());
			}
		}

		NoteTypeCollection predefinedNoteTypes;

		protected virtual NoteTypeCollection InitializeNoteTypes()
		{
			return new NoteTypeCollection();
		}
	}
}