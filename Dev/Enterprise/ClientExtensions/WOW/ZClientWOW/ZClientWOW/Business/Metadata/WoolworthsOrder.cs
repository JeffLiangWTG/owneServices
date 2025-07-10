using Enterprise.Metadata.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.Wow.Metadata
{
	class WoolworthsOrder : Order
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			return noteTypes;
		}
	}
}
