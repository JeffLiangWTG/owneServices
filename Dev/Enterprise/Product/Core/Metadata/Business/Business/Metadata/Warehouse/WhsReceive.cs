using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.WhsReceive)]
	public class WhsReceive : WhsDocket
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote);
			return noteTypes;
		}
	}
}
