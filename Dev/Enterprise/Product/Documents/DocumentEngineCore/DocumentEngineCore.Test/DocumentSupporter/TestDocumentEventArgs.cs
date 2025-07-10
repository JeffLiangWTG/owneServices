using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	public class TestDocumentEventArgs : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			DocumentEventArgs args = new DocumentEventArgs(Factory.New<IStmMenuItem>());
			AssertNotNull(args.MenuItem);
		}
	}
}
