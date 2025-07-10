using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class HSExtensionCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_JobComInvoiceLine()
		{
			SetTariffAdditionalCode();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "0301929090";
			AssertEquals(1, invoiceLine.HSExtensionCodeCollection.Count);
			var hsExtensionCode_CAT = invoiceLine.HSExtensionCodeCollection[0];
			AssertEquals("01, 02", hsExtensionCode_CAT.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("There are 3 SCA rows.", hsExtensionCode_CAT.Lookups.CY_CodeList.GetDescriptionFromCode("01"));
			AssertEquals("There are 1 SCA rows.", hsExtensionCode_CAT.Lookups.CY_CodeList.GetDescriptionFromCode("02"));

			hsExtensionCode_CAT.CY_Code = "01";
			AssertEquals("CY_Order => CAT:0, SCA:1, SCA:2", 3, invoiceLine.HSExtensionCodeCollection.Count);
			var hsExtensionCode_SCA = invoiceLine.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 1).FirstOrDefault();
			AssertEquals("1N, 1R", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value1", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("1N"));
			AssertEquals("Test Value2", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("1R"));
			hsExtensionCode_SCA = invoiceLine.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 2).FirstOrDefault();
			AssertEquals("2L", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value3", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2L"));

			hsExtensionCode_CAT.CY_Code = "02";
			AssertEquals("CY_Order => CAT:0, SCA:2", 2, invoiceLine.HSExtensionCodeCollection.Count);
			hsExtensionCode_SCA = invoiceLine.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 2).FirstOrDefault();
			AssertEquals("2S", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value4", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2S"));

			invoiceLine.JI_Tariff = "0106209000";
			AssertEquals(1, invoiceLine.HSExtensionCodeCollection.Count);
			hsExtensionCode_CAT = invoiceLine.HSExtensionCodeCollection[0];
			AssertEquals("01", hsExtensionCode_CAT.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("There are 2 SCA rows.", hsExtensionCode_CAT.Lookups.CY_CodeList.GetDescriptionFromCode("01"));

			hsExtensionCode_CAT.CY_Code = "01";
			AssertEquals("CY_Order => CAT:0, SCA:2", 2, invoiceLine.HSExtensionCodeCollection.Count);
			hsExtensionCode_SCA = invoiceLine.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 2).FirstOrDefault();
			AssertEquals("2Q, 2T", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value5", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2Q"));
			AssertEquals("Test Value6", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2T"));
		}

		public void TestCodeList_CusClassPartPivot()
		{
			SetTariffAdditionalCode();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "0301929090";
			AssertEquals(1, pivot.HSExtensionCodeCollection.Count);
			var hsExtensionCode_CAT = pivot.HSExtensionCodeCollection[0];
			AssertEquals("01, 02", hsExtensionCode_CAT.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("There are 3 SCA rows.", hsExtensionCode_CAT.Lookups.CY_CodeList.GetDescriptionFromCode("01"));
			AssertEquals("There are 1 SCA rows.", hsExtensionCode_CAT.Lookups.CY_CodeList.GetDescriptionFromCode("02"));

			hsExtensionCode_CAT.CY_Code = "01";
			AssertEquals("CY_Order => CAT:0, SCA:1, SCA:2", 3, pivot.HSExtensionCodeCollection.Count);
			var hsExtensionCode_SCA = pivot.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 1).FirstOrDefault();
			AssertEquals("1N, 1R", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value1", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("1N"));
			AssertEquals("Test Value2", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("1R"));
			hsExtensionCode_SCA = pivot.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 2).FirstOrDefault();
			AssertEquals("2L", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value3", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2L"));

			hsExtensionCode_CAT.CY_Code = "02";
			AssertEquals("CY_Order => CAT:0, SCA:2", 2, pivot.HSExtensionCodeCollection.Count);
			hsExtensionCode_SCA = pivot.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 2).FirstOrDefault();
			AssertEquals("2S", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value4", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2S"));

			pivot.CI_TariffNum = "0106209000";
			AssertEquals(1, pivot.HSExtensionCodeCollection.Count);
			hsExtensionCode_CAT = pivot.HSExtensionCodeCollection[0];
			AssertEquals("01", hsExtensionCode_CAT.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("There are 2 SCA rows.", hsExtensionCode_CAT.Lookups.CY_CodeList.GetDescriptionFromCode("01"));

			hsExtensionCode_CAT.CY_Code = "01";
			AssertEquals("CY_Order => CAT:0, SCA:2", 2, pivot.HSExtensionCodeCollection.Count);
			hsExtensionCode_SCA = pivot.HSExtensionCodeCollection.Where(x => x.Category == "SCA" && x.CY_Order == 2).FirstOrDefault();
			AssertEquals("2Q, 2T", hsExtensionCode_SCA.Lookups.CY_CodeList.CodesAsString);
			AssertEquals("Test Value5", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2Q"));
			AssertEquals("Test Value6", hsExtensionCode_SCA.Lookups.CY_CodeList.GetDescriptionFromCode("2T"));
		}

		void SetTariffAdditionalCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0301929090", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAdditionalCodeView(tariff1, "CAT", "01", dataGrouping: Core.Constants.CountryCodes.KoreaSouth, description: "There are 3 SCA rows.");
			helper.CreateTariffAdditionalCodeView(tariff1, "CAT", "02", dataGrouping: Core.Constants.CountryCodes.KoreaSouth, description: "There are 1 SCA rows.");
			CreateSubCategoryAdditionalCodeView(tariff1, "1N", "Test Value1", "01");
			CreateSubCategoryAdditionalCodeView(tariff1, "1R", "Test Value2", "01");
			CreateSubCategoryAdditionalCodeView(tariff1, "2L", "Test Value3", "01");
			CreateSubCategoryAdditionalCodeView(tariff1, "2S", "Test Value4", "02");

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0106209000", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAdditionalCodeView(tariff2, "CAT", "01", dataGrouping: Core.Constants.CountryCodes.KoreaSouth, description: "There are 2 SCA rows.");
			CreateSubCategoryAdditionalCodeView(tariff2, "2Q", "Test Value5", "01");
			CreateSubCategoryAdditionalCodeView(tariff2, "2T", "Test Value6", "01");

			void CreateSubCategoryAdditionalCodeView(TariffView tariffView, string code, string codeDescription, string parentAdditionalCode)
			{
				var subCategory = helper.CreateTariffAdditionalCodeView(tariffView, "SCA", code, dataGrouping: Core.Constants.CountryCodes.KoreaSouth, description: codeDescription);
				subCategory.ZY2_ZY3_NKParentCategory = "CAT";
				subCategory.ZY2_ParentAdditionalCode = parentAdditionalCode;
			}
		}
	}
}
