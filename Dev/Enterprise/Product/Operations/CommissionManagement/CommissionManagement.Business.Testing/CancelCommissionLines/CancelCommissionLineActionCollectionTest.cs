using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CancelCommissionLineActionCollection))]
	public class CancelCommissionLineActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CancelCommissionLineActionCollection>
	{
		protected override CancelCommissionLineActionCollection GetCollectionToTest()
		{
			return new CancelCommissionLineActionCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			return new CancelCommissionLineAction(commissionLine);
		}
	}
}
