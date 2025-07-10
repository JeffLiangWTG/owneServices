using CargoWise.Definitions;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomNoteTypeItem))]
	sealed class NoteTypeItemTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestAcceptsOnlyWesternEuropeanChars()
		{
			CustomNoteTypeItem note = BizObj as CustomNoteTypeItem;
			note.DefaultVisibility = "PUB";
			note.NoteName = "hello";
			note.RunPreSaveValidation();
			AssertNoErrors(note);

			note.NoteName = "日本の";
			note.RunPreSaveValidation();
			AssertHasError(note.NoteNameInfo, "Note Type Description only accepts Western European languages characters.");
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CustomNoteTypeItem result = new CustomNoteTypeItem();
			result.IsTextOnly = ZBool.True;
			result.IsAppendingNote = ZBool.True;
			result.IsReadOnlyAfterAdd = ZBool.False;
			result.ForceRead = ZBool.False;
			result.DefaultVisibility = nameof(StmNoteVisibility.PRV);
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
