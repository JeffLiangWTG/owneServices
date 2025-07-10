using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionBatchCollection))]
	public class AccCollectionBatchCollectionTest : ActiveBusinessObjectCollectionTestCase<AccCollectionBatchCollection>
	{
		protected override AccCollectionBatchCollection GetCollectionToTest()
		{
			return new AccCollectionBatchCollection(Factory);
		}
	}
}
