using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DataSaveProcessorTest : TestCaseWithFactory
	{
		public void TestSaveStatus()
		{
			var orgsSaveProcessor = new DataSaveProcessor(Factory, "In progress", "Completed");
			orgsSaveProcessor.Saving += new DataTransferProcessor.SavingEventHandler((int percentageCompleted, string status) =>
			{
				AssertEquals("In progress", status);
				AssertEquals(1, percentageCompleted);
			});

			orgsSaveProcessor.SavingComplete += new DataTransferProcessor.SavingCompleteEventHandler((int percentageCompleted, string status) =>
			{
				AssertEquals("Completed", status);
				AssertEquals(100, percentageCompleted);
			});

			orgsSaveProcessor.Import();
		}
	}
}
