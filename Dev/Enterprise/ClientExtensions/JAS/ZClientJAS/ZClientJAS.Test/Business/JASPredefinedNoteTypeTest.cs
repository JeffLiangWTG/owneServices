using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	internal class JASPredefinedNoteTypeTest : TestCase
	{
		public void TestConstructors()
		{
			JASPredefinedNoteType noteType = new JASPredefinedNoteType("Desc", StmNoteVisibility.DOC, true, true, true);
			AssertEquals("Desc", noteType.Description);
			AssertEquals(StmNoteVisibility.DOC, noteType.DefaultVisibility);
			Assert(noteType.IsOnlyOneAllowed);
			Assert(noteType.IsReadOnlyAfterAdd);
			Assert(noteType.IsTextOnly);
			noteType = new JASPredefinedNoteType("Desc2", StmNoteVisibility.INT, false, false, false, 20);
			AssertEquals("Desc2", noteType.Description);
			AssertEquals(StmNoteVisibility.INT, noteType.DefaultVisibility);
			Assert(!noteType.IsOnlyOneAllowed);
			Assert(!noteType.IsReadOnlyAfterAdd);
			Assert(!noteType.IsTextOnly);
			AssertEquals(20, noteType.TextOnlyMaxLength);
		}
	}
}
