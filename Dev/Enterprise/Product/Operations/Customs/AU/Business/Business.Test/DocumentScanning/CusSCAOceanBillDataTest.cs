using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusSCAOceanBillDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.BaseCusSCAOceanBill), new CusSCAOceanBillData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusSCAOceanBillCollection>("CollectionType", new CusSCAOceanBillData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.All, new CusSCAOceanBillData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "SCA Ocean Bill", new CusSCAOceanBillData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new CusSCAOceanBillData().IsAllowedForUnallocatedeDocs);
		}
	}
}
