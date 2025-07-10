#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class AUJobDeclarationTest : BaseJobDeclarationTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.MarksAndNumbers);
			return noteTypes;
		}

		public void TestNoteTypes_UseShareProperty()
		{
			var noteTypes = ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.MarksAndNumbers);

			var metadata = NewMetadata;
			((IAUJobDeclarationShareProperty)metadata).IsImportCMR = true;
			((IAUJobDeclarationShareProperty)metadata).IsExWarehouse = true;
			var actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(noteTypes, actual);

			metadata = NewMetadata;
			((IAUJobDeclarationShareProperty)metadata).IsImportCMR = true;
			((IAUJobDeclarationShareProperty)metadata).IsTransportModeOther = true;
			actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(noteTypes, actual);
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new AUJobDeclaration(); }
		}

		#endregion
	}
}

#endif