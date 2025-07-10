#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class ForwardingConsolTest : CommonConsolTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CarrierBookingRequest);
			noteTypes.Add(PredefinedNoteTypes.Instance.PortMessageRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees);
			return noteTypes;
		}

		public void TestNoteTypes_UseShareProperty()
		{
			var noteTypes = ExpectedNoteTypes();

			var metadata = NewMetadata;
			((IForwardingConsolShareProperty)metadata).EnableBoleroEBLIntegration = false;

			var actual = metadata.NoteTypes;
			AssertContainsExactElementsInAnyOrder(noteTypes, actual);

			noteTypes.Add(PredefinedNoteTypes.Instance.OriginalBillNotes);

			metadata = NewMetadata;
			((IForwardingConsolShareProperty)metadata).EnableBoleroEBLIntegration = true;

			actual = metadata.NoteTypes;
			AssertContainsExactElementsInAnyOrder(noteTypes, actual);
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new ForwardingConsol(); }
		}

		#endregion
	}
}

#endif
