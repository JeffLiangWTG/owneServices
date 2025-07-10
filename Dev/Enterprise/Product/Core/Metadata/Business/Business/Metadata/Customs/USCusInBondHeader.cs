using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.USCusInBondHeader)]
	public class USCusInBondHeader : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			return noteTypes;
		}
	}
}
