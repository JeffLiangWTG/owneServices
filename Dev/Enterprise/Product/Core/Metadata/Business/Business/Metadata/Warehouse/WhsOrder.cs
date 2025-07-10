using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.WhsOrder)]
	public class WhsOrder : WhsDocket
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.PickingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.RTUSRequestLog);
			return noteTypes;
		}
	}
}
