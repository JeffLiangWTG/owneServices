using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.USCusISFHeader)]
	public class USCusISFHeader : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);

			return noteTypes;
		}
	}
}
