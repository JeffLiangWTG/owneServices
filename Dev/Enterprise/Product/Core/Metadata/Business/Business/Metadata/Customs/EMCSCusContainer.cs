using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.EMCSCusContainer)]
	public class EMCSCusContainer : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();

			noteTypes.Add(PredefinedNoteTypes.Instance.ContainerComment);
			noteTypes.Add(PredefinedNoteTypes.Instance.ContainerSealDetails);

			return noteTypes;
		}
	}
}
