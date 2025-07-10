using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class EInvoicingEmailNotificationCreatorTest : TestCaseWithFactory
	{
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}