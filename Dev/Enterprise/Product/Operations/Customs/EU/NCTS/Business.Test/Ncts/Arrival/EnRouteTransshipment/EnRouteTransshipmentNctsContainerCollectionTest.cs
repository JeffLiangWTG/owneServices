using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(EnRouteTransshipmentNctsContainerCollection))]
	public class EnRouteTransshipmentNctsContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<EnRouteTransshipmentNctsContainerCollection>
	{
		public virtual void TestMaster()
		{
			var collection = GetCollectionToTest();
			AssertType<EnRouteTransshipment>(collection.Master);
		}

		public void TestDefaultParentTableCode()
		{
			var collection = GetCollectionToTest();
			var container = collection.AddNew();
			AssertEquals(CusInBondEventSchema.Constants.Prefix, container.BC_ParentTableCode);
		}

		protected override EnRouteTransshipmentNctsContainerCollection GetCollectionToTest()
		{
			var enRouteTransshipment = Factory.New<EnRouteTransshipment>();
			return enRouteTransshipment.Containers;
		}
	}
}
