using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	public abstract class DocumentsMenuProviderTest : TestCaseWithFactory
	{
		protected void AssertIDocumentsMenuProvider(object obj, string name)
		{
			Assert(string.Format("{0} should implement IDocumentsMenuProvider", name), obj is IDocumentsMenuProvider);
			AssertNotNull("DocumentsMenuHelper should exist", ((IDocumentsMenuProvider)obj).DocumentsMenuHelper);
		}
	}
}
