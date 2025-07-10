using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeaderCollection))]
	sealed class CusLPCOHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusLPCOHeaderCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			var coll = GetCollectionToTest();

			var lpco1 = coll.AddNew();
			lpco1.CPH_Number = "111";

			var lpco2 = coll.AddNew();
			lpco2.CPH_Number = "222";

			var lpco3 = coll.AddNew();
			lpco3.CPH_Number = "333";

			var lpco4 = coll.AddNew();
			lpco4.CPH_Number = "444";

			AssertContainsExactElementsInAnyOrder(new[] { lpco1, lpco2, lpco3, lpco4 }, coll);

			lpco2.CPH_RN_NKCountryCode = "AR";
			AssertCollectionNotContains(new[] { lpco2 }, coll);

			lpco3.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
			AssertCollectionNotContains(new[] { lpco3 }, coll);

			lpco4.CPH_Type = "CHP";
			AssertCollectionNotContains(new[] { lpco4 }, coll);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<CusLPCOHeader>();
		}

		protected override CusLPCOHeaderCollection GetCollectionToTest()
		{
			return new CusLPCOHeaderCollection(Factory);
		}
	}
}
