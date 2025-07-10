using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniversalReferenceHelperTest : TestCaseWithFactory
	{
		public void TestErrata51Enabled()
		{
			AssertEquals(false, UniversalReferenceHelper.Errata51Enabled());
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata51Enabled());
			}
		}

		public void TestErrata53Enabled()
		{
			AssertEquals(false, UniversalReferenceHelper.Errata53Enabled());
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());
			}
		}

		public void TestGetPostcodeDeliveryClassificationAttribute() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("AUPC", "AQIS Postcodes", "AU");
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2001", "Sydney", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2026", "Bondi", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PostcodeDeliveryClassification", "Postcode Delivery Classification", "AUPC", "AU");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, "PostcodeDeliveryClassification", "Metro");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, "PostcodeDeliveryClassification", "");
			Factory.Save();

			AssertEquals("Invalid Postcode", "Rural", UniversalReferenceHelper.GetPostcodeDeliveryClassificationAttribute(Factory, "ZZ"));
			AssertEquals("No Postcode find in Reference", "Rural", UniversalReferenceHelper.GetPostcodeDeliveryClassificationAttribute(Factory, "2032"));
			AssertEquals("Has 'PostcodeDeliveryClassification' attribute", "Metro", UniversalReferenceHelper.GetPostcodeDeliveryClassificationAttribute(Factory, "2001"));
			AssertEquals("Attribute 'PostcodeDeliveryClassification' value is empty", string.Empty, UniversalReferenceHelper.GetPostcodeDeliveryClassificationAttribute(Factory, "2026"));
		});
	}
}
