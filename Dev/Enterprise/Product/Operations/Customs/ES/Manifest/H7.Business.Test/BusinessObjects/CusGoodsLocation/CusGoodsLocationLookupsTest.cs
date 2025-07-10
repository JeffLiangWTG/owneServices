using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class CusGoodsLocationLookupsTest : TestCaseWithFactory
	{
		public void TestCusGoodsLocationQualifierList()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			var list = cusGoodsLocation.Lookups.QualifierList;

			CombineAssertions(() =>
			{
				AssertEquals(1, list.Count);

				Assert(list.ContainsCode("Y"));
				AssertEquals("Authorization Number", list.GetDescriptionFromCode("Y"));
			});
		}

		public void TestCusGoodsLocationTypeList()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			var list = cusGoodsLocation.Lookups.TypeList;

			CombineAssertions(() =>
			{
				AssertEquals(1, list.Count);

				Assert(list.ContainsCode("B"));
				AssertEquals("Authorized Place", list.GetDescriptionFromCode("b"));
			});
		}
	}
}
