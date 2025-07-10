#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class WhsReceiveTest : WhsDocketTest
	{
		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote);
			return noteTypes;
		}

		protected override IMetadata NewMetadata => new WhsReceive();
	}
}

#endif
