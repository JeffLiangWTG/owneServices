#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class CAJobDeclarationTest : BaseJobDeclarationTest
	{
		#region TestNoteTypes

		public void TestNoteTypes_UseShareProperty()
		{
			var noteTypes = ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3);
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD);
			noteTypes.Add(PredefinedNoteTypes.Instance.LeadSheetComments);
			noteTypes.Add(PredefinedNoteTypes.Instance.ManualSubmission);
			noteTypes.Add(PredefinedNoteTypes.Instance.ManualRelease);
			noteTypes.Add(PredefinedNoteTypes.Instance.ManualCancel);

			var metadata = NewMetadata;
			((ICAJobDeclarationShareProperty)metadata).IsImport = true;
			var actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(noteTypes, actual);
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new CAJobDeclaration(); }
		}

		#endregion
	}
}

#endif
