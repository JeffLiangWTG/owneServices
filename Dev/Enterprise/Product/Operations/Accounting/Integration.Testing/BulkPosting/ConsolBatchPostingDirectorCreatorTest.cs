using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Integration.Testing
{
	public class ConsolBatchPostingDirectorCreatorTest : TestCaseWithFactory
	{
		public void TestInstantiation()
		{
			ConsolBatchPostingDirectorCreator testCreator = new ConsolBatchPostingDirectorCreator();
			IConsolBatchPostingDirector testResult = testCreator.GetNewConsolBatchPostingDirector();

			AssertNotNull(testResult);
			AssertEquals(ObjectFactory.Get<IConsolBatchPostingDirector>().GetType(), testResult.GetType());
		}
	}
}
