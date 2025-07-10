#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class WhsDocketTest : EnterpriseBusinessObjectTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
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

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata => new WhsDocket();

		#endregion
	}
}

#endif
