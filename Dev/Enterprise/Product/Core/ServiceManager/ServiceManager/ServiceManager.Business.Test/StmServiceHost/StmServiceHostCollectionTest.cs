using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(StmServiceHostCollection))]
	class StmServiceHostCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmServiceHostCollection(Factory);
		}
	}
}
