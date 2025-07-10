using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FileImportedEventTest : TestCaseWithFactory
	{
		public void TestCompleted()
		{
			FileImportedEventArgs testArgs = new FileImportedEventArgs(3, 4);
			AssertEquals(3, testArgs.Completed);
		}

		public void TestTotal()
		{
			FileImportedEventArgs testArgs = new FileImportedEventArgs(3, 4);
			AssertEquals(4, testArgs.Total);
		}
	}
}
