using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestClassification()
		{
			CusClassification parent = Factory.New<CusClassification>();
			AssertEquals(parent.Lookups.Classification, parent);
		}
	}
}
