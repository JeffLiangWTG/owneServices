using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class AmountAndUnitControlTest : TestCaseWithFactory
{
	public void TestAmountCalEdit()
	{
		AssertType<ZCalcEdit>(control.AmountCalcEdit);
	}

	public void TestUnitOfMeasureTextBox()
	{
		AssertType<ZTextBox>(control.UnitOfMeasureTextBox);
		AssertEquals("UnitOfMeasureText", "KG", control.UnitOfMeasureTextBox.Text);

		control.UnitOfMeasureText = "GRAM";
		AssertEquals("UnitOfMeasureText", "GRAM", control.UnitOfMeasureTextBox.Text);
	}

	public void TestBinding()
	{
		var declaration = Factory.New<JobDeclaration>();
		control.SetDataBinding(declaration, "CustomsEntryHeaders.TotalNetWeightInKG");

		var amountBindingMember = control.AmountCalcEdit.GetBindingMember();
		AssertEquals("Binding Member", "CustomsEntryHeaders.TotalNetWeightInKG", amountBindingMember);
	}

	public void TestAmountCalcEditCaption()
	{
		var sut = control.AmountCalcEdit;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("AmountCalcEdit caption", sut.CaptionResourceString.Caption);
		AssertEquals("AmountCalcEdit caption visible", expected: false, labelCaptionVisible);
	}

	public void TestUnitOfMeasureTextBoxCaption()
	{
		var sut = control.UnitOfMeasureTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("UnitOfMeasureTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("UnitOfMeasureTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestImplementIExtendedControl()
	{
		AssertEquals("Implements IExtendedControl", expected: true, control is IExtendedControl);
		AssertEquals("Host", control, control.Host);
		AssertNotNull("Extensions", control.Extensions);
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new AmountAndUnitControl();
	}

	AmountAndUnitControl control;
}
