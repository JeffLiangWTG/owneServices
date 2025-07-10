using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.CommissionManagement.Business.Testing
{
	class CommissionSaveProcessorTest : TestCaseWithFactory
	{
		public void TestSaveStatus()
		{
			var processor = new CommissionSaveProcessor(Factory);
			processor.Saving += new DataTransferProcessor.SavingEventHandler((int percentageCompleted, string status) =>
			{
				AssertEquals("Saving entity commissions. This may take a long time depending on the amount being imported.", status);
				AssertEquals(1, percentageCompleted);
			});

			processor.SavingComplete += new DataTransferProcessor.SavingCompleteEventHandler((int percentageCompleted, string status) =>
			{
				AssertEquals("Entity commissions saved.", status);
				AssertEquals(100, percentageCompleted);
			});

			processor.Import();
		}
	}
}
