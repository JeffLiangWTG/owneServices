using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionOrderCollection))]
	public class AccCollectionOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<AccCollectionOrderCollection>
	{
		protected override AccCollectionOrderCollection GetCollectionToTest()
		{
			return new AccCollectionOrderCollection(Factory);
		}
	}
}
