using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class DocumentsMenuItemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			string name = "foo #1";
			string contentType = "foo #2";

			DocumentCommand documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "foo #3";

			DocumentsMenuItem item = new DocumentsMenuItem(documentCommand, contentType, name);

			AssertEquals("DocumentCommand should be the same", documentCommand, item.DocumentCommand);
			AssertEquals("ContentType should be the same", contentType, item.ContentType);
			AssertEquals("Name should be the same", name, item.Name);

			item = new DocumentsMenuItem(documentCommand, contentType);
			AssertEquals("Default name equals SU_MenuName property of DocumentCommand", documentCommand.SU_MenuName, item.Name);
		}
	}
}
