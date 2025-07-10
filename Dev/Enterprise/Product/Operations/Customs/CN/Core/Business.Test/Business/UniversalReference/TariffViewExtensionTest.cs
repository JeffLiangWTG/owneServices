using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class TariffViewExtensionTest : TestCaseWithFactory
	{
		public void TestInwardSupervisionConditionsString()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "4xA", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "4", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "B", tariff3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", ZString.Empty, TariffViewExtension.InwardSupervisionConditions(null));
				AssertEquals("Tariff1 with 3 ImportCUSRequirement Attributes", "4, A, x", tariff1.InwardSupervisionConditions());
				AssertEquals("Tariff2 with 0 ImportCUSRequirement Attributes", ZString.Empty, tariff2.InwardSupervisionConditions());
				AssertEquals("Tariff3 with 1 ExportCUSRequirement Attribute", ZString.Empty, tariff3.InwardSupervisionConditions());
			});
		}

		public void TestOutwardSupervisionConditions()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "4xA", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "B", tariff3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", ZString.Empty, TariffViewExtension.OutwardSupervisionConditions(null));
				AssertEquals("Tariff1 with 3 ExportCUSRequirement Attributes", "4, A, x", tariff1.OutwardSupervisionConditions());
				AssertEquals("Tariff2 with 0 ExportCUSRequirement Attributes", ZString.Empty, tariff2.OutwardSupervisionConditions());
				AssertEquals("Tariff3 with 1 ImportCUSRequirement Attribute", ZString.Empty, tariff3.OutwardSupervisionConditions());
			});
		}

		public void TestHasSpecialCIQImportRequirement()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "MV", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "W", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "M", tariff2);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "N", tariff3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasSpecialCIQImportRequirement(null));
				AssertEquals("Tariff1 with extra Import Atribute value V", true, tariff1.HasSpecialCIQImportRequirement());
				AssertEquals("Tariff2 with only Import Atribute value M", false, tariff2.HasSpecialCIQImportRequirement());
				AssertEquals("Tariff3 with only Export Atribute", false, tariff3.HasSpecialCIQImportRequirement());
			});
		}

		public void TestHasSpecialCIQExportRequirement()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "NV", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "W", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "N", tariff1);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "M", tariff3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasSpecialCIQExportRequirement(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasSpecialCIQExportRequirement());
				AssertEquals("Tariff2 with only Export Atribute value N", false, tariff2.HasSpecialCIQExportRequirement());
				AssertEquals("Tariff3 with only Import Atribute", false, tariff3.HasSpecialCIQExportRequirement());
			});
		}

		public void TestHasExportCUSRequirementB()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "BQ", tariff1);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasExportCUSRequirementB(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasExportCUSRequirementB());
			});
		}

		public void TestHasImportCUSRequirementA()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "AF", tariff1);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasImportCUSRequirementA(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasImportCUSRequirementA());
			});
		}

		public void TestHasImportCUSRequirementL()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "LM", tariff1);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasImportCIQRequirementL(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasImportCIQRequirementL());
			});
		}

		public void TestHasCommodityTypeMED()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "MED", tariff1);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasCommodityTypeMED(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasCommodityTypeMED());
			});
		}

		public void TestHasCommodityTypeCFCS()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "CFCS", tariff1);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasCommodityTypeCFCS(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasCommodityTypeCFCS());
			});
		}

		public void TestHasCommodityTypeUME()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "UME", tariff1);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasCommodityTypeUME(null));
				AssertEquals("Tariff1 with extra Export Atribute value V", true, tariff1.HasCommodityTypeUME());
			});
		}

		public void TestHasCommodityTypeATP()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "ATP", tariff);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasCommodityTypeATP(null));
				AssertEquals("Tariff with COMMODITYTYPE Atribute value ATP", true, tariff.HasCommodityTypeATP());
			});
		}

		public void TestHasCommodityTypeDGC()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "DGC", tariff);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.HasCommodityTypeDGC(null));
				AssertEquals("Tariff with COMMODITYTYPE Atribute value DGC", true, tariff.HasCommodityTypeDGC());
			});
		}

		public void TestDoesNotSupportTSD()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.SupportsTSD, Constants.UniversalReferenceConstants.CusTariffAttributeValue.NotSupportsTSD, tariff);

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", false, TariffViewExtension.DoesNotSupportTSD(null));
				AssertEquals("Tariff Not Support Two Step Declaration", true, TariffViewExtension.DoesNotSupportTSD(tariff));
				AssertEquals("Tariff2 Support Two Step Declaration", false, TariffViewExtension.DoesNotSupportTSD(tariff2));
			});
		}

		public void TestCIQImportRequirements()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "NVW", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "M", tariff3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", ZString.Empty, TariffViewExtension.CIQImportRequirements(null));
				AssertEquals("Tariff1 with 3 ImportCIQRequirement Attributes", "N, V, W", tariff1.CIQImportRequirements());
				AssertEquals("Tariff2 with 0 ImportCIQRequirement Attributes", ZString.Empty, tariff2.CIQImportRequirements());
				AssertEquals("Tariff3 with 1 ExportCIQRequirement Attribute", ZString.Empty, tariff3.CIQImportRequirements());
			});
		}

		public void TestCIQExportRequirements()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "MVW", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "N", tariff3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Null Tariff", ZString.Empty, TariffViewExtension.CIQImportRequirements(null));
				AssertEquals("Tariff1 with 3 ExportCIQRequirement Attributes", "M, V, W", tariff1.CIQExportRequirements());
				AssertEquals("Tariff2 with 0 ExportCIQRequirement Attributes", ZString.Empty, tariff2.CIQExportRequirements());
				AssertEquals("Tariff3 with 1 ImportCIQRequirement Attribute", ZString.Empty, tariff3.CIQExportRequirements());
			});
		}

		public void TestGetSortedAdditionalInfoAttributes()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "MV", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "W", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "03", "99999", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "02", "11111", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "01", "00000", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "04", OriginalManufacturerNameCNStrategy.AdditionalElementCode, tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "05", OriginalManufacturerNameENStrategy.AdditionalElementCode, tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "06", AntiDumpingDutyRateStrategy.AdditionalElementCode, tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "07", CountervailingDutyRateStrategy.AdditionalElementCode, tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + "08", MeetsPricePromiseStrategy.AdditionalElementCode, tariff1);

			Factory.Save();

			var allCodes = new ZString[] { "00000", "11111", "99999",OriginalManufacturerNameCNStrategy.AdditionalElementCode,
				OriginalManufacturerNameENStrategy.AdditionalElementCode, AntiDumpingDutyRateStrategy.AdditionalElementCode,
				CountervailingDutyRateStrategy.AdditionalElementCode, MeetsPricePromiseStrategy.AdditionalElementCode };

			AssertArrayEqualsByElements(allCodes, tariff1.GetSortedAdditionalInfoAttributes(EnteringOrExiting.Both).Select(x => x.ZZ3_Value).ToArray());
			AssertArrayEqualsByElements(allCodes, tariff1.GetSortedAdditionalInfoAttributes(EnteringOrExiting.Entering).Select(x => x.ZZ3_Value).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "00000", "11111", "99999" }, tariff1.GetSortedAdditionalInfoAttributes(EnteringOrExiting.Exiting).Select(x => x.ZZ3_Value).ToArray());
			AssertArrayEqualsByElements(allCodes.Skip(1).ToArray(), tariff1.GetSortedGoodsSpecModelAttribues(EnteringOrExiting.Both).Select(x => x.ZZ3_Value).ToArray());
			AssertArrayEqualsByElements(allCodes.Skip(1).ToArray(), tariff1.GetSortedGoodsSpecModelAttribues(EnteringOrExiting.Entering).Select(x => x.ZZ3_Value).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "11111", "99999" }, tariff1.GetSortedGoodsSpecModelAttribues(EnteringOrExiting.Exiting).Select(x => x.ZZ3_Value).ToArray());
		}

		public void TestMandatoryGoodsSpecModelAttributesCount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff1 = helper.CreateCustomsTariff("2713200000", "00000", "00010", "00423", "00352", "00009", "00005", "99999");
			var tariff2 = helper.CreateCustomsTariff("2713200010", "00000", "00010", "99999", "00423", "00352", "00009", "00005");

			Factory.Save();

			AssertEquals("MandatoryGoodsSpecModelAttributesCount", 3, tariff1.MandatoryGoodsSpecModelAttributesCount(EnteringOrExiting.Both));
			AssertEquals("MandatoryGoodsSpecModelAttributesCount", 4, tariff2.MandatoryGoodsSpecModelAttributesCount(EnteringOrExiting.Both));
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "HSN");
			Factory.Save();
		}
		UniversalReferenceTestDataHelper helper;
		RefCusTariffType tariffType;
	}
}
