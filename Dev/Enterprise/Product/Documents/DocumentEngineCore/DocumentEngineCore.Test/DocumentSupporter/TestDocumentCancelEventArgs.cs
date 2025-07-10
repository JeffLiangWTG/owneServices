using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	public class TestDocumentCancelEventArgs : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			DocumentCancelEventArgs args = new DocumentCancelEventArgs(Factory.New<IStmMenuItem>());
			AssertNotNull(args.MenuItem);
			Assert(!args.Cancel);
			args.Cancel = true;
			Assert(args.Cancel);
		}
	}
}
