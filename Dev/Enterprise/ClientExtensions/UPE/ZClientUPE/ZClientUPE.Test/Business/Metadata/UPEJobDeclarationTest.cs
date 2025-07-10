using Enterprise.Client.UPE.Business;
using Enterprise.Metadata.Business.Tests;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Metadata.Testing
{
	public class UPEJobDeclarationTest : AUJobDeclarationTest
	{
		protected override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.DeclarationNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.RefundNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.ManualBillNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.PreReleaseNotification);
			return noteTypes;
		}

		protected override IMetadata NewMetadata
		{
			get
			{
				return new UPEJobDeclaration();
			}
		}
	}
}
