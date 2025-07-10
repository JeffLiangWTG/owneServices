using Enterprise.Client.JAS.Business;
using Enterprise.Metadata.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Metadata
{
	class JASForwardingConsol : ForwardingConsol
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(JASPredefinedNoteTypes.Instance.JXCExportLog);
			return noteTypes;
		}
	}
}
