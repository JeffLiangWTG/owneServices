using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class MergingRuleAccordingToSpecModelTest : TestCaseWithFactory
	{
		public void TestGetKeyForLine()
		{
			CombineAssertions("IMP + BTH", () =>
			{
				var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.Both);
				var declaration = items.JobDeclaration;
				declaration.CustomsEntryInstructions.AddNew().CEI_CEI_Parent = items.EntryInstruction.PK;
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = items.EntryInstruction.PK;

				var testItem = new MergingRuleAccordingToSpecModel();
				invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";
				invoiceLine.XC_GoodsSpecModel2 = "|DDDDD|无必报要素";
				var mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals("case1", 2, mergeKeys.Count());
				AssertEquals("case1", "CCCCC||无必报要素", mergeKeys.ElementAt(0));
				AssertEquals("case1", "|DDDDD|无必报要素", mergeKeys.ElementAt(1));
			});

			CombineAssertions("IMP + CUS", () =>
			{
				var items = EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.CustomsEntry);
				var declaration = items.JobDeclaration;
				var invoiceLine = declaration.InvoiceLines.AddNew();

				var testItem = new MergingRuleAccordingToSpecModel();
				invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";
				invoiceLine.XC_GoodsSpecModel2 = "|DDDDD|无必报要素";

				var mergeKeys = testItem.GetKeysForLine(invoiceLine);
				AssertEquals(1, mergeKeys.Count());
				AssertEquals("CCCCC||无必报要素", mergeKeys.ElementAt(0));
			});
		}
	}
}
