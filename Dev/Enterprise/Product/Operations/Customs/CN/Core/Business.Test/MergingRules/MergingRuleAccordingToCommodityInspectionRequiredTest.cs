using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class MergingRuleAccordingToCommodityInspectionRequiredTest : TestCaseWithFactory
	{
		public void TestGetKeyForLine()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var newFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(newFactory);

			var tariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			newFactory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "MV", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "W", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "M", tariff3);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "N", tariff3);

			newFactory.Save();

			CombineAssertions("IMP + BTH", () =>
			{
				var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.Both);
				var declaration = items.JobDeclaration;
				declaration.CustomsEntryInstructions.AddNew().CEI_CEI_Parent = items.EntryInstruction.PK;
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = items.EntryInstruction.PK;

				helper.CreateAdditionalElement("00423", "针入度");
				helper.CreateAdditionalElement("00352", "加工方法");

				helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
				helper.CreateCustomsTariff("2713200001", "00000", "00423", "00352", "99999");
				helper.CreateCustomsTariff("2713200002", "00000", "00423", "00352", "99999");

				Factory.Save();

				var testItem = new MergingRuleAccordingToSpecialCIQRequired();

				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";
				invoiceLine.XC_GoodsSpecModel2 = "|DDDDD|无必报要素";
				var mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("case1", 2, mergeKeys.Count());
				AssertEquals("case1", "CCCCC||无必报要素", mergeKeys.ElementAt(0));
				AssertEquals("case1", "|DDDDD|无必报要素", mergeKeys.ElementAt(1));

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("no ciq requirement at all", 0, mergeKeys.Count());

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("case3", 0, mergeKeys.Count());
			});

			CombineAssertions("IMP + CUS", () =>
			{
				var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.CustomsEntry);
				var declaration = items.JobDeclaration;
				var invoiceLine = declaration.InvoiceLines.AddNew();

				helper.CreateAdditionalElement("00423", "针入度");
				helper.CreateAdditionalElement("00352", "加工方法");

				helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
				helper.CreateCustomsTariff("2713200001", "00000", "00423", "00352", "99999");
				helper.CreateCustomsTariff("2713200002", "00000", "00423", "00352", "99999");

				Factory.Save();

				var testItem = new MergingRuleAccordingToSpecialCIQRequired();

				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				invoiceLine.XC_GoodsSpecModel2 = "|DDDDD|无必报要素";
				invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";

				var mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals(1, mergeKeys.Count());
				AssertEquals("CCCCC||无必报要素", mergeKeys.ElementAt(0));

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("no ciq requirement at all", 0, mergeKeys.Count());

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("case3", 0, mergeKeys.Count());
			});

			CombineAssertions("EXP + REC", () =>
			{
				var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export, DecTypeList.Codes.RecordListing);
				var declaration = items.JobDeclaration;
				var invoiceLine = declaration.InvoiceLines.AddNew();

				helper.CreateAdditionalElement("00423", "针入度");
				helper.CreateAdditionalElement("00352", "加工方法");

				helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
				helper.CreateCustomsTariff("2713200001", "00000", "00423", "00352", "99999");
				helper.CreateCustomsTariff("2713200002", "00000", "00423", "00352", "99999");

				Factory.Save();

				var testItem = new MergingRuleAccordingToSpecialCIQRequired();

				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				invoiceLine.XC_GoodsSpecModel = "|DDDDD|无必报要素";
				invoiceLine.XC_GoodsSpecModel2 = "CCCCC||无必报要素";
				var mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals(1, mergeKeys.Count());
				AssertEquals("case1", "|DDDDD|无必报要素", mergeKeys.ElementAt(0));

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("no ciq requirement at all", 0, mergeKeys.Count());

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("case3", 0, mergeKeys.Count());
			});
		}
	}
}
