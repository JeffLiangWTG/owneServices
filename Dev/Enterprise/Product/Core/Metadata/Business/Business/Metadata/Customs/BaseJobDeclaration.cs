using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.BaseJobDeclaration)]
	public class BaseJobDeclaration : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.CertificateOfOriginNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.PickupInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
			noteTypes.Add(PredefinedNoteTypes.Instance.CartageHistoryNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsQuarantineMessagingRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRules);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesInternal);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesValidation);
			AddMarksAndNumbersNoteType(noteTypes);
			return noteTypes;
		}

		protected virtual void AddMarksAndNumbersNoteType(NoteTypeCollection noteTypes)
		{
			noteTypes.Add(PredefinedNoteTypes.Instance.MarksAndNumbers);
		}
	}
}
