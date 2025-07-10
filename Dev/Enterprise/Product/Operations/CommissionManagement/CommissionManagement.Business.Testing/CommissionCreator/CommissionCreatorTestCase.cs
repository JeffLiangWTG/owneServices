using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public abstract class CommissionCreatorTestCase : TestCaseWithFactory
	{
		#region Implementation

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

		#endregion
	}
}
