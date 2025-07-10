using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Reports.Testing
{
	public abstract class CustomsOfficesCodeListProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BankCode");
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			var today = ZDateTime.Today;
			testHelper.CreateCusCodeList(CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LT3", today.AddDays(-1), today.AddDays(1));
			testHelper.CreateCusCodeList(CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LT2", today.AddDays(-2), today.AddDays(-1));
			testHelper.CreateCusCodeList(CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LT1", today.AddDays(-1), today.AddDays(1));
			testHelper.CreateCusCodeList(CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "XX4", today.AddDays(-1), today.AddDays(1));
			testHelper.CreateCusCodeList("XY", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AU5", today.AddDays(-1), today.AddDays(1));
			Factory.Save();

			CombineAssertions(() =>
			{
				var tester = GetProvider().GetCodeDescriptionPairList();
				AssertContainsExactElementsInExactOrder(new string[] { "LT1", "LT3" }, tester.GetAllCodes());
			});
		}

		protected abstract CustomsOfficesCodeListProvider GetProvider();
		protected abstract ZString CountryCode { get; }
	}
}
