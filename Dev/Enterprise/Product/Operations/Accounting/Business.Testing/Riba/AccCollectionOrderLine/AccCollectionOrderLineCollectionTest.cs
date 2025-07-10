using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionOrderLineCollection))]
	public class AccCollectionOrderLineCollectionTest : ActiveBusinessObjectCollectionTestCase<AccCollectionOrderLineCollection>
	{
		protected override AccCollectionOrderLineCollection GetCollectionToTest()
		{
			return new AccCollectionOrderLineCollection(Factory);
		}
	}
}
