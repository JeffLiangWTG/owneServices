using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CusEntryLineExtendedInformationQuantitiesUserControl))]
sealed class CusEntryLineExtendedInformationQuantitiesUserControlTest : TestCaseWithFactory
{
	public void TestCalcCustomsNetWeightDropEdit()
	{
		using (control)
		{
			var codeEditBox = control.CalcCustomsNetWeightDropEdit;
			AssertNotNull(nameof(codeEditBox), codeEditBox);
			AssertType<ZCalcDropEdit>(control.CalcCustomsNetWeightDropEdit);
			AssertEquals(nameof(codeEditBox.Visible), true, codeEditBox.Visible);
		}
	}

	public void TestCalcAdditionalQuantityDropEdit()
	{
		using (control)
		{
			var codeEditBox = control.CalcAdditionalQuantityDropEdit;
			AssertNotNull(nameof(codeEditBox), codeEditBox);
			AssertType<ZCalcDropEdit>(control.CalcAdditionalQuantityDropEdit);
			AssertEquals(nameof(codeEditBox.Visible), true, codeEditBox.Visible);
		}
	}

	public void TestCalcNetWeightDropEdit()
	{
		using (control)
		{
			var codeEditBox = control.CalcNetWeightDropEdit;
			AssertNotNull(nameof(codeEditBox), codeEditBox);
			AssertType<ZCalcDropEdit>(control.CalcNetWeightDropEdit);
			AssertEquals(nameof(codeEditBox.Visible), true, codeEditBox.Visible);
		}
	}

	public void TestCalcGrossWeightDropEdit()
	{
		using (control)
		{
			var codeEditBox = control.CalcGrossWeightDropEdit;
			AssertNotNull(nameof(codeEditBox), codeEditBox);
			AssertType<ZCalcDropEdit>(control.CalcGrossWeightDropEdit);
			AssertEquals(nameof(codeEditBox.Visible), true, codeEditBox.Visible);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new CusEntryLineExtendedInformationQuantitiesUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	CusEntryLineExtendedInformationQuantitiesUserControl control;
}
