#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class ForwardingShipmentTest : EnterpriseBusinessObjectTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride);
			noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillChargesOverride);
			noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillFollowOnOverride);
			noteTypes.Add(PredefinedNoteTypes.Instance.PortMessageRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.EmissionsCalculationLog);
			return noteTypes;
		}

		public void TestNoteTypes_UseShareProperty()
		{
			var noteTypes = ExpectedNoteTypes();

			var metadata = NewMetadata;
			((ForwardingShipment)metadata).EnableBoleroEHBLIntegration = false;

			var actual = metadata.NoteTypes;
			AssertContainsExactElementsInAnyOrder(noteTypes, actual);

			noteTypes.Add(PredefinedNoteTypes.Instance.OriginalBillNotes);

			metadata = NewMetadata;
			((ForwardingShipment)metadata).EnableBoleroEHBLIntegration = true;

			actual = metadata.NoteTypes;
			AssertContainsExactElementsInAnyOrder(noteTypes, actual);
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new ForwardingShipment(); }
		}

		#endregion
	}
}

#endif
