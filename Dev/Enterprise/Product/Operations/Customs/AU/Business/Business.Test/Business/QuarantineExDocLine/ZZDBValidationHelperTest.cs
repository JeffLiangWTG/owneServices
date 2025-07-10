using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ZZDBValidationHelperTest : TestCaseWithFactory
	{
		public void TestGetCusCodesRequired()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var nprdd = refHelper.CreateNewOrGetExistingCusCodeType("NPRCD", "Dairy");

			var codeBUT = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, nprdd.ZZK_CodeType, "BUT", "BUTTER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, nprdd.ZZK_CodeType, "CHD", "CHEDDAR CHEESE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("ProductType", "Desc.", nprdd.ZZK_CodeType, Core.Constants.CountryCodes.Australia, nprdd.ZZK_CodeType);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(codeBUT.PK, "ProductType", "MIL");
			Factory.Save();

			var values = validationHelper.GetProductTypeValues("BUT", "D");
			Assert(values.Any(x => x.EqualsIgnoringCase("MIL")));
		}

		public void TestGetAHECCAttributes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("NPRCA", "NEXDOCS Product Category AHECC");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType, "DC0045", "DC0045", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType, "DC0275", "DC0275", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AHECCC", "AHECC Code Desc.", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "AHECCC", "04021010");
			Factory.Save();

			var value = validationHelper.GetAHECCValues("DC0045").Single();
			AssertEquals("04021010", value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationHelper = new ZZDBValidationHelper(Factory);
		}

		ZZDBValidationHelper validationHelper;
	}
}
