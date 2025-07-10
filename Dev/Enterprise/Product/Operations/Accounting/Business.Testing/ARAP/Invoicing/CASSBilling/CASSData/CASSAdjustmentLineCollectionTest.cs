using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CASSAdjustmentLineCollection))]
	public class CASSAdjustmentLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CASSAdjustmentLineCollection>
	{
		protected override CASSAdjustmentLineCollection GetCollectionToTest()
		{
			return new CASSAdjustmentLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CASSAdjustmentLine();
		}
	}
}
