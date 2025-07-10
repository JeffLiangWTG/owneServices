using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing
{
	public abstract class BaseEInvoicingDataValidatorTest : TestCaseWithFactory
	{
		public abstract BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest();

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
