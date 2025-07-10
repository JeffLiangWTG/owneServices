using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class OrderTest : EnterpriseBusinessObjectTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.ExtraOrderDetails);
			noteTypes.Add(PredefinedNoteTypes.Instance.OrderManagementUpdate);
			noteTypes.Add(PredefinedNoteTypes.Instance.OrderManagementNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			return noteTypes;
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new Order(); }
		}

		#endregion
	}
}
