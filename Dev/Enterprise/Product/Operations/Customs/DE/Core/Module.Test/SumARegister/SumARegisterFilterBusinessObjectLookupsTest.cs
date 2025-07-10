using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.Module.Testing
{
	sealed class SumARegisterFilterBusinessObjectLookupsTest : TestCaseWithFactory
	{
		public void TestOwnerReferenceTypeList()
		{
			var ownerReferenceTypeList = lookups.OwnerReferenceTypeList;

			CombineAssertions(() =>
			{
				AssertSame("Cached", ownerReferenceTypeList, lookups.OwnerReferenceTypeList);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					OwnerReferenceTypeList.Codes.AWB,
					OwnerReferenceTypeList.Codes.ULD,
					OwnerReferenceTypeList.Codes.ZZZ
				}, ownerReferenceTypeList.GetAllCodes());
			});
		}

		public void TestCustomsStatusList()
		{
			var list = lookups.CustomsStatusList;

			CombineAssertions(() =>
			{
				AssertType<CustomsStatusList>("Type", list);
				AssertEquals("CodesAsString", "DEL, FIN, LCK, PAC, PRE, TST", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsStatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			lookups = new SumARegisterFilterBusinessObjectLookups(new SumARegisterFilterBusinessObject());
		}

		SumARegisterFilterBusinessObjectLookups lookups;
	}
}
