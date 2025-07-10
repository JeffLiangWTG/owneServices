using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(DependentAccTransLinePayCollection))]
	public class DependentAccTransLinePayCollectionTest : ActiveBusinessObjectCollectionTestCase<DependentAccTransLinePayCollection>
	{
		protected override DependentAccTransLinePayCollection GetCollectionToTest()
		{
			return new DependentAccTransLinePayCollection(Factory.New<APInvoiceLine>());
		}
	}
}
