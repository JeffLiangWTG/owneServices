using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DataTransfer.BatchProcessor.Testing
{
	public abstract class BaseLoggedDataBatchProcessTestCase : TestCaseWithFactory
	{
		protected StmALog[] GenerateTestLogs(int numberOfLogsToGenerate)
		{
			StmALogCollection logs = new StmALogCollection(Factory);

			for (int i = 0; i < numberOfLogsToGenerate; i++)
			{
				var log = logs.AddNew();
				log.FillWithValidTestData();
			}

			Factory.Save();

			return (StmALog[])logs.ToArray(typeof(StmALog));
		}
	}
}
