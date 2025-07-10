using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.DtbTransport)]
	public class DtbTransport : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation);
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);

			return noteTypes;
		}
	}
}
