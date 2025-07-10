using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Common.JP.AFR;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BLLFunctionLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestChangeReasonCodeList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = BLLFunction.New(header, BLLFunctionCode.RegisterSplit);

			foreach (ICodeDescription codeDescription in new AFRBLLChangeReasonCodeList())
			{
				AssertEquals(true, bllFunction.Lookups.ChangeReasonCodeList.ContainsCode(codeDescription.Code));
			}
		}

		public void TestRegisteredBillList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill3");
			var bllFunction = BLLFunction.New(header, BLLFunctionCode.RegisterSplit);

			AssertEquals(true, bllFunction.Lookups.RegisteredBillList.ContainsCode("bill1"));
			AssertEquals(true, bllFunction.Lookups.RegisteredBillList.ContainsCode("bill2"));
			AssertEquals(false, bllFunction.Lookups.RegisteredBillList.ContainsCode("bill3"));
		}
	}
}
