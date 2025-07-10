using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(UndoCancelCommissionLineActionCollection))]
	public class UndoCancelCommissionLineActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UndoCancelCommissionLineActionCollection>
	{
		protected override UndoCancelCommissionLineActionCollection GetCollectionToTest()
		{
			return new UndoCancelCommissionLineActionCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			return new UndoCancelCommissionLineAction(commissionLine);
		}
	}
}

