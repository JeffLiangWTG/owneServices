using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Test
{
	sealed class UniversalReferenceHelperTest : TestCaseWithFactory
	{
		public void TestGetRefCusRateCodePairList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateType1 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ExciseTax);
			var refCusRateCode11 = helper.LoadOrCreateNewCusRateCode(Factory, "C01", refCusRateType1.PK);
			var refCusRateCode12 = helper.LoadOrCreateNewCusRateCode(Factory, "C02", refCusRateType1.PK);
			var refCusRateCode13 = helper.LoadOrCreateNewCusRateCode(Factory, "C03", refCusRateType1.PK);

			var refCusRateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.GST);
			var refCusRateCode21 = helper.LoadOrCreateNewCusRateCode(Factory, "C04", refCusRateType2.PK);
			var refCusRateCode22 = helper.LoadOrCreateNewCusRateCode(Factory, "C05", refCusRateType2.PK);

			var list = UniversalReferenceHelper.GetRefCusRateCodePairList(Factory, DutyAndTaxTypes.Codes.ExciseTax, ZDateTime.Today);
			AssertEquals(4, list.Count);

			list = UniversalReferenceHelper.GetRefCusRateCodePairList(Factory, DutyAndTaxTypes.Codes.GST, ZDateTime.Today);
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("C04"));
			AssertEquals(true, list.ContainsCode("C05"));
		}
	}
}
