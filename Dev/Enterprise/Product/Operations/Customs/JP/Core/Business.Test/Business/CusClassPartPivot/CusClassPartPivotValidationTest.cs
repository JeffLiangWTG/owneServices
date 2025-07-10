using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotValidation))]
	sealed class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCI_TradeControlOrderAppendix()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingValue = Core.Constants.CountryCodes.Japan;
			var codeTypeValue = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix;
			helper.CreateNewOrGetExistingCusCodeType(codeTypeValue, "Testdescription");
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingValue, codeTypeValue, "Code", "TestDescription", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			var cusClassPartPivot = Factory.New<CusClassPartPivot>();
			cusClassPartPivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
			cusClassPartPivot.CI_TradeControlOrderAppendix = "*";
			AssertHasMessageError(cusClassPartPivot.CI_TradeControlOrderAppendixInfo, ListValidation.InvalidCodeMessageError);

			cusClassPartPivot.CI_TradeControlOrderAppendix = (cusClassPartPivot.Lookups.TradeControlOrderAppendixList as CodeDescriptionPairList)[0].Code;
			AssertNoMessageError(cusClassPartPivot.CI_TradeControlOrderAppendixInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_StorageType()
		{
			var cusClassPartPivot = Factory.New<CusClassPartPivot>();
			cusClassPartPivot.CI_StorageType = "*";
			AssertHasMessageError(cusClassPartPivot.CI_StorageTypeInfo, ListValidation.InvalidCodeMessageError);

			cusClassPartPivot.CI_StorageType = cusClassPartPivot.Lookups.StorageTypeList[0].Code;
			AssertNoMessageError(cusClassPartPivot.CI_StorageTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_DomesticConsumptionTaxExemptionCode()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportConsumptionTaxExemptionCode, "B");

			var cusClassPartPivot = Factory.New<CusClassPartPivot>();
			ValidationTestHelper.AssertInvalidCodeMessageError(cusClassPartPivot.CI_DomesticConsumptionTaxExemptionCodeInfo, "*", cusClassPartPivot.Lookups.ConsumptionTaxExemptionIDList[0].Code);
		}

		public void TestCheckCI_FEFTAArticle48()
		{
			var cusClassPartPivot = Factory.New<CusClassPartPivot>();
			cusClassPartPivot.CI_FEFTAArticle48 = "*";
			AssertHasMessageError(cusClassPartPivot.CI_FEFTAArticle48Info, ListValidation.InvalidCodeMessageError);

			cusClassPartPivot.CI_FEFTAArticle48 = cusClassPartPivot.Lookups.FEFTAArticle48List[0].Code;
			AssertNoMessageError(cusClassPartPivot.CI_FEFTAArticle48Info, ListValidation.InvalidCodeMessageError);
		}
	}
}
