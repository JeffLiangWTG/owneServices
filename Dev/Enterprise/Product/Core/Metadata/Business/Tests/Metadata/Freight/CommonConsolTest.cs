#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class CommonConsolTest : EnterpriseBusinessObjectTest
	{
#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.PickupInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.AdditionalSecurityInformation);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRules);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.ForwardingInstructionNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation);
			noteTypes.Add(PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.LoadListInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesInternal);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesValidation);
			noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesValidation);
			return noteTypes;
		}

#endregion

#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new CommonConsol(); }
		}

#endregion
	}
}

#endif