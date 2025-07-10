using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

sealed class MethodOfPaymentListProviderTest : TestCaseWithFactory
{
	public void TestGetCodeDescriptionPairList()
	{
		var testHelper = new UniversalReferenceTestDataHelper(Factory);
		testHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

		var today = ZDateTime.Today;
		testHelper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", today.AddDays(-1), today.AddDays(1));
		testHelper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "X", today.AddDays(-2), today.AddDays(-1));
		testHelper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "B", today.AddDays(-1), today.AddDays(1));
		testHelper.CreateCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "FOREIGNER", today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var tester = new MethodOfPaymentListProvider().GetCodeDescriptionPairList();
			AssertEquals(2, tester.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "A", "B" }, tester.GetAllCodes());
		});
	}
}
