using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSUniversalHelperTest : TestCaseWithFactory
	{
		public void TestGetEMCSPackTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateOrGetLanguage(Core.Constants.CountryCodes.Germany, "German");
			Factory.Save();
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "BO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "AM", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(code, Core.Constants.CountryCodes.Germany, "Ampulle, ungeschützt");
			Factory.Save();

			var packTypeList = Factory.GetEMCSPackTypeList(Core.Constants.CountryCodes.Germany);
			CombineAssertions(() =>
			{
				AssertEquals("List", "AE, AM, BO", packTypeList.CodesAsString);
				AssertSame("Cached", packTypeList, Factory.GetEMCSPackTypeList(Core.Constants.CountryCodes.Germany));
				AssertEquals("German description", "Ampulle, ungeschützt", packTypeList.GetDescriptionFromCode("AM"));
			});
		}

		public void TestGetExciseProductCodes()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunGrouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "B000", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "C000", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			Factory.Save();

			var list = Factory.GetExciseProductCodes(Core.Constants.CountryCodes.Latvia);
			AssertContainsExactElementsInAnyOrder("Codes", new[] { "B000", "C000" }, list.Select(c => c.ZZD_Code));
			AssertEquals("Cached", list, Factory.GetExciseProductCodes(Core.Constants.CountryCodes.Latvia));
		}

		public void TestGetPackageIsCountable()
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var boCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "BO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			boCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);
			var vqCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var package = Factory.New<EMCSPackage>();

			CombineAssertions(() =>
			{
				package.B5_UnitType = "BO";
				Assert("Countable", package.IsCountable());

				package.B5_UnitType = "VQ";
				Assert("Not countable", !package.IsCountable());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		}
		UniversalReferenceTestDataHelper helper;
		RefDataGrouping eunGrouping;
	}
}
