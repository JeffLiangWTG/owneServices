using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.USJobDeclaration)]
	public class USJobDeclaration : BaseJobDeclaration
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageToPrintOn7501);
			return noteTypes;
		}
	}
}
