using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine
{
	public class OverflowNoteDocument : DocumentWrapper
	{
		OverflowNoteCollection collection;
		public OverflowNoteCollection OverflowNotes => collection ?? (collection = new OverflowNoteCollection(null));
	}
}
