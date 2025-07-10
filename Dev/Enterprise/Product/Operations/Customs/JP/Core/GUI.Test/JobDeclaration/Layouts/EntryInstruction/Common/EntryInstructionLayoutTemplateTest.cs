using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(EntryInstructionLayoutTemplate))]
sealed class EntryInstructionLayoutTemplateTest : TestCase
{
	public void TestControls()
	{
		using var control = new EntryInstructionLayoutTemplate();
		CombineAssertions(() =>
		{
			TestHelper.AssertControlExists(control, "WeightzCalcDropEdit", ".");
			TestHelper.AssertControlExists(control.FindSingle<ZCalcDropEdit>("WeightzCalcDropEdit"), "UnitDropEdit", "CEI_GrossWeightUnit");
			TestHelper.AssertControlExists(control.FindSingle<ZCalcDropEdit>("WeightzCalcDropEdit"), "AmountCalcEdit", "CEI_GrossWeight");
			TestHelper.AssertControlExists(control, "CargoQuantityCalcDropEdit", ".");
			TestHelper.AssertControlExists(control.FindSingle<ZCalcDropEdit>("CargoQuantityCalcDropEdit"), "UnitDropEdit", "CEI_CargoQuantityUnit");
			TestHelper.AssertControlExists(control.FindSingle<ZCalcDropEdit>("CargoQuantityCalcDropEdit"), "AmountCalcEdit", "CEI_CargoQuantity");

			Assert("CustomsInspectionCodeTextBox Visible", control.FindSingle<ZTextBox>("CustomsInspectionCodeTextBox").Visible);
			Assert("TradeTypeFirstCharDropEdit Visible", control.FindSingle<ZDropEdit>("TradeTypeFirstCharDropEdit").Visible);
			Assert("TradeTypeSecondCharDropEdit Visible", control.FindSingle<ZDropEdit>("TradeTypeSecondCharDropEdit").Visible);
			Assert("TradeTypeThirdCharDropEdit Visible", control.FindSingle<ZDropEdit>("TradeTypeThirdCharDropEdit").Visible);

			var cargoQuantityCalcDropEdit = control.FindSingle<ZCalcDropEdit>("CargoQuantityCalcDropEdit");
			Assert("CargoQuantityCalcDropEdit Decimals", cargoQuantityCalcDropEdit.Decimals == 0);
			Assert("CargoQuantityCalcDropEdit MaxValue", (int)cargoQuantityCalcDropEdit.MaxValue == 99999999);
			Assert("CargoQuantityCalcDropEdit AllowNegative", !cargoQuantityCalcDropEdit.AllowNegative);

			Assert("WeightzCalcDropEdit AllowNegative", !control.FindSingle<ZCalcDropEdit>("WeightzCalcDropEdit").AllowNegative);
		});
	}
}

