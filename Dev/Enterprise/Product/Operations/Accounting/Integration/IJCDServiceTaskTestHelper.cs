#if DEBUG

using CargoWise.Data;

namespace Enterprise.Accounting.Integration
{
	public interface IJCDServiceTaskTestHelper
	{
		void InitialiseAndRunJCDServiceTask(DbConnection connection);

		void RunJCDServiceTaskForTableCreation(DbConnection connection);
	}
}

#endif
