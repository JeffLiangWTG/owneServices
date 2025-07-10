using Enterprise.Client.UPE.Business;
using Enterprise.Metadata.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Metadata
{
	public class UPEJobDeclaration : AUJobDeclaration
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.DeclarationNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.RefundNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.ManualBillNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.PreReleaseNotification);
			return noteTypes;
		}
	}
}
