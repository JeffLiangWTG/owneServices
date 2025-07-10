using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.Quote)]
	public class Quote : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.QuoteCoverPageText);
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			return noteTypes;
		}
	}
}
