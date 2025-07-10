using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.AgencyShipment)]
	public class AgencyShipment : CommonShipment
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.ContainerReleaseNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.ForwardingInstructionNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.WebUserNote);
			return noteTypes;
		}
	}
}
