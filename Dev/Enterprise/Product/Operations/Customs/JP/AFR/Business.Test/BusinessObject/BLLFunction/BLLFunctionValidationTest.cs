using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BLLFunctionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJPM_BillOfLadingNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill3");
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);

			bllFunction.JPM_BillOfLadingNumber = ZString.Empty;
			AssertHasErrorContaining(bllFunction.JPM_BillOfLadingNumberInfo, MandatoryValidation.MustBeEntered);

			bllFunction.JPM_BillOfLadingNumber = "bill3";
			AssertHasErrorContaining(bllFunction.JPM_BillOfLadingNumberInfo, ListValidation.InvalidCodeError);

			bllFunction.JPM_BillOfLadingNumber = "bill1";
			AssertNoNotifications(bllFunction.JPM_BillOfLadingNumberInfo);
		}

		public void TestCheckJPM_ChangeReasonCode()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);

			bllFunction.JPM_ChangeReasonCode = ZString.Empty;
			AssertHasMessageErrorContaining(bllFunction.JPM_ChangeReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			bllFunction.JPM_ChangeReasonCode = "X";
			AssertHasMessageErrorContaining(bllFunction.JPM_ChangeReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			bllFunction.JPM_ChangeReasonCode = "1";
			AssertNoNotifications(bllFunction.JPM_ChangeReasonCodeInfo);
		}
	}
}
