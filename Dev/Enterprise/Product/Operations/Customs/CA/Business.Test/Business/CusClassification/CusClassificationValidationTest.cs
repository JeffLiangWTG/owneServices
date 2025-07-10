using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusClassificationValidationTest : Customs.Business.Testing.CusClassificationValidationTest
	{
		public void TestParent()
		{
			CusClassification parent = Factory.New<CusClassification>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckCC_TariffNum()
		{
			var cACClassHeader1 = Factory.New<CACClassHeader>();
			cACClassHeader1.ZA_ClassificationNumber = "1234";
			cACClassHeader1.ZA_AreaCode = "AAA";
			cACClassHeader1.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cACClassHeader1.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var cACClassHeader2 = Factory.New<CACClassHeader>();
			cACClassHeader2.ZA_ClassificationNumber = "1234567890";
			cACClassHeader2.ZA_AreaCode = "BBB";
			cACClassHeader2.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cACClassHeader2.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);

			var helper = new CACExportTariffTestCase(Factory);
			helper.CreateNewTariffIfNotExists("1234567800", "DESC");
			Factory.Save();
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			classification.CC_TariffNum = "1234567800";
			AssertNoMessageErrors(classification.CC_TariffNumInfo);
			classification.CC_TariffNum = "1234567890";
			AssertHasMessageErrorContaining(classification.CC_TariffNumInfo, "Tariff code 1234567890 not found in the export tariff code list.");
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1234567899";
			AssertHasMessageErrorContaining(classification.CC_TariffNumInfo, ZString.Format("Classification was not found or is not valid for {0}.", ZDateTime.Today.ToShortDateString()));
			classification.CC_TariffNum = "1234567890";
			AssertNoMessageErrors(classification.CC_TariffNumInfo);
			classification.CC_TariffNum = "1234";
			AssertHasMessageErrorContaining(classification.CC_TariffNumInfo, "Tariff code must be 10 characters long.");
		}

		public void TestCheckSIMADutiesRequired()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var countervailingRelationShip = universalHelper.CreateTariffRelationship(countervailingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var countervailingRate = universalHelper.CreateRate(countervailingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1234567890";
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			classification.DutiesAndTaxes.DeleteAll();
			classification.Validation.ValidateCC_TariffNum();
			AssertHasWarningContaining(classification.CC_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");

			var tax = classification.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			classification.Validation.ValidateCC_TariffNum();
			AssertNoWarningContaining(classification.CC_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");

			classification.DutiesAndTaxes.DeleteAll();
			classification.OnRefreshSIMAMeasureEvent = null;
			classification.CC_TariffNum = "0123456789";
			classification.DutiesAndTaxes.DeleteAll();
			classification.Validation.ValidateCC_TariffNum();
			AssertNoWarningContaining(classification.CC_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");

			classification.OnRefreshSIMAMeasureEvent = delegate
			{
				return classification.SIMAMeasures.OfType<SIMADumpingNumber>().FirstOrDefault(x => x.CA_DumpingNumber == "AD1408");
			};
			classification.CC_TariffNum = ZString.Empty;
			classification.CC_TariffNum = "0123456789";
			classification.DutiesAndTaxes.DeleteAll();
			classification.Validation.ValidateCC_TariffNum();
			AssertHasWarningContaining(classification.CC_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");
		}
	}
}
