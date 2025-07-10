using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmDataCollection))]
	sealed class StmDataCollectionTest : ActiveBusinessObjectCollectionTestCase<StmDataCollection>
	{
		protected override StmDataCollection GetCollectionToTest()
		{
			return new StmDataCollection(Factory, new ZQuery());
		}
	}
}
