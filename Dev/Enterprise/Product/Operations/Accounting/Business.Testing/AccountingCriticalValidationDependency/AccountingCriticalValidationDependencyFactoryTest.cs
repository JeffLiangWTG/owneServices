using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.AccountingCriticalValidationDependency.Testing
{
	public class AccountingCriticalValidationDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestGetIAccountingCriticalValidationDependencyFactory()
		{
			AssertType<AccountingCriticalValidationDependencyFactory>(ObjectFactory.Get<IAccountingCriticalValidationDependencyFactory>());
		}
	}
}
