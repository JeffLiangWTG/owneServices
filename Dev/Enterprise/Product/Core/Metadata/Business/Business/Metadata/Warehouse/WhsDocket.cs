using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.WhsDocket)]
	public class WhsDocket : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);

			if (SupportsUnmatchedOrgNoteType)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
			}

			if (SupportsUnrecognisedAdditionalReferenceType)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
			}

			return noteTypes;
		}

		protected virtual bool SupportsUnmatchedOrgNoteType => true;

		protected virtual bool SupportsUnrecognisedAdditionalReferenceType => true;
	}
}
