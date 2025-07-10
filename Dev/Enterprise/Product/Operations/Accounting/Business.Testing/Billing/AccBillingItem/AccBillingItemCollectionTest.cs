using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Billing.Testing
{
	[TestedType(typeof(AccBillingItemCollection))]
	public class AccBillingItemCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccBillingItemCollection(Factory.New<AccBillingHeader>(), Factory);
		}

		#endregion
	}
}
