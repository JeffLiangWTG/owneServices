#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class WhsOrderTest : WhsDocketTest
	{
		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.PickingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.RTUSRequestLog);
			return noteTypes;
		}

		protected override IMetadata NewMetadata => new WhsOrder();
	}
}

#endif
