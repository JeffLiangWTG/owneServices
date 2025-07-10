#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class AgencyShipmentTest : CommonShipmentTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.ContainerReleaseNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.ForwardingInstructionNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.WebUserNote);
			return noteTypes;
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new AgencyShipment(); }
		}

		#endregion
	}
}

#endif