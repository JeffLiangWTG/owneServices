using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusPermitHeaderValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCPH_QtyValIndicator()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_QtyValIndicator = ZString.Empty;
			AssertNoNotifications("CPH_QtyValIndicator can be empty.", permit.CPH_QtyValIndicatorInfo);

			permit.CPH_QtyValIndicator = "%";
			AssertHasErrorContaining("CPH_QtyValIndicator cannot be invalid code.", permit.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCPH_FullType()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800"), new TestSupportingDocumentCodeList("3200"));
			Factory.Save();

			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_FullType = "6600";
			AssertHasErrorContaining(permit.CPH_FullTypeInfo, ListValidation.InvalidCodeError);

			permit.CPH_FullType = "2800";
			AssertNoErrorContaining(permit.CPH_FullTypeInfo, ListValidation.InvalidCodeError);

			permit.CPH_FullType = "";
			AssertHasErrorContaining(permit.CPH_FullTypeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCPH_Type()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_Type = "!@#$";
			AssertNoNotifications("CPH_Type should not do any validation as it is readonly.", permit.CPH_TypeInfo);

			permit.CPH_Type = "";
			AssertNoNotifications("CPH_Type should not do any validation as it is readonly.", permit.CPH_TypeInfo);
		}

		public void TestCheckCPH_SubType()
		{
			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_SubType = "!@#";
			AssertNoNotifications("CPH_SubTyp should not do any validation as it is readonly.", permit.CPH_SubTypeInfo);

			permit.CPH_SubType = "";
			AssertNoNotifications("CPH_SubTyp should not do any validation as it is readonly.", permit.CPH_SubTypeInfo);
		}

		public void TestCheckCPH_UnitOfMeasure()
		{
			string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");

				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "Tax Unit of Quantity");
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Unit of Quantity");

				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "KGM", "Kilogram", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "LTR", "Liter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "WAT", "Watt", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GBP", "UK Pound", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTR", "Metre", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic Metres", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var permit = Factory.New<CusPermitHeader>();
				AssertNoMessageErrorContaining(permit.CPH_UnitOfMeasureInfo, "The code you have selected is not in the list");

				permit.CPH_UnitOfMeasure = "AAA";
				AssertHasMessageErrorContaining(permit.CPH_UnitOfMeasureInfo, "The code you have selected is not in the list");

				permit.CPH_UnitOfMeasure = "KGM";
				AssertNoMessageErrorContaining(permit.CPH_UnitOfMeasureInfo, "The code you have selected is not in the list");
			}
		}
		public void TestCheckCPH_QtyValIndicator()
		{
			var permit1 = Factory.New<CusPermitHeader>();
			permit1.CPH_QtyValIndicator = "ZZZ";
			AssertHasErrorContaining(permit1.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);
			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			AssertNoErrorContaining(permit1.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);

			permit1.CPH_Type = "REB";
			permit1.CPH_QtyValIndicator = ZString.Empty;
			AssertNoErrors(permit1.CPH_QtyValIndicatorInfo);
		}
	}
}
