using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class MergingRulesTesting : TestCaseWithFactory
	{
		public void TestMergeRulesCUS()
		{
			var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.CustomsEntry);
			var declaration = items.JobDeclaration;
			var strategy = new ParentEntryCreationStrategy(declaration, EntryTypeList.Codes.CustomsEntry);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";

			invoiceLine.JI_ProductVersion = "PV0001";
			invoiceLine.TradeUnitPrice = 100.00m;

			var mergeKeys = strategy.GetKeyForLine(invoiceLine);

			AssertEquals(true, !mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, !mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[0].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, !mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[1].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[2].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[3].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, mergeKeys.Contains(new ZString("CCCCC||无必报要素")));
		}

		public void TestMergeRulesREC()
		{
			var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.RecordListing);
			var declaration = items.JobDeclaration;
			var strategy = new ParentEntryCreationStrategy(declaration, EntryTypeList.Codes.RecordListing);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";

			invoiceLine.JI_ProductVersion = "PV0001";
			invoiceLine.TradeUnitPrice = 100.00m;

			var mergeKeys = strategy.GetKeyForLine(invoiceLine);

			AssertEquals(true, !mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, !mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[0].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, !mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[1].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[2].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[3].Selected = true;
			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, mergeKeys.Contains(new ZString("CCCCC||无必报要素")));
		}
	}
}
