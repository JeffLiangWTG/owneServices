using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ConfirmationTypeListTest : TestCaseWithFactory
	{
		public void TestGetDescriptionFromValue()
		{
			AssertEquals(ConfirmationTypeList.Descriptions.Yes, ConfirmationTypeList.GetDescriptionFromValue(true));
			AssertEquals(ConfirmationTypeList.Descriptions.No, ConfirmationTypeList.GetDescriptionFromValue(false));
		}

		public void TestGetYesAndNoList()
		{
			AssertArrayEqualsByElements(new[] { ConfirmationTypeList.Codes.Yes, ConfirmationTypeList.Codes.No }, ConfirmationTypeList.GetYesAndNoList(Factory).GetAllCodes());
			AssertSame("Cached YesAndNoList", ConfirmationTypeList.GetYesAndNoList(Factory), ConfirmationTypeList.GetYesAndNoList(Factory));
		}
	}
}
