using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	[TestedType(typeof(AccPayableOrderHeaderCollection))]
	public class AccPayableOrderHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<AccPayableOrderHeaderCollection>
	{
		protected override AccPayableOrderHeaderCollection GetCollectionToTest()
		{
			return new AccPayableOrderHeaderCollection(Factory);
		}
	}
}
