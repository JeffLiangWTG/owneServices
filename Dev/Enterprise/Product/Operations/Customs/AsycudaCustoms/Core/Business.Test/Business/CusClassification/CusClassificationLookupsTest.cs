using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestClassification()
		{
			var parent = Factory.New<CusClassification>();
			AssertEquals(parent.Lookups.Classification, parent);
		}
	}
}
