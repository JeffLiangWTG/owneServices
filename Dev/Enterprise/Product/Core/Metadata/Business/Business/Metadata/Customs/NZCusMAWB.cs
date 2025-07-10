using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.NZCusMAWB)]
	public class NZCusMAWB : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsDeliveryInstructions);
			return noteTypes;
		}
	}
}
