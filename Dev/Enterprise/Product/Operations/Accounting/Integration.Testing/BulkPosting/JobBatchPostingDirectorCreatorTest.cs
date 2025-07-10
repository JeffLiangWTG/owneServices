using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Integration.Testing
{
	public class JobBatchPostingDirectorCreatorTest : TestCaseWithFactory
	{
		public void TestInstantiation()
		{
			JobBatchPostingDirectorCreator testCreator = new JobBatchPostingDirectorCreator();
			IJobBatchPostingDirector testResult = testCreator.GetNewJobBatchPostingDirector();

			AssertNotNull(testResult);
			AssertEquals(ObjectFactory.GetType<IJobBatchPostingDirector>(), testResult.GetType());
		}
	}
}
