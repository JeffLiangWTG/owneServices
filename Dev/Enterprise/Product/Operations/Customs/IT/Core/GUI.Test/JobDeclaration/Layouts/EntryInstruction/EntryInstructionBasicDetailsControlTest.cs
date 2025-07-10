using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryInstructionBasicDetailsControlTest : TestCaseWithFactory
{
	public void TestPreviousInvoiceAmountBoundCurrencyControl()
	{
		AssertType<ConvertToLocalCurrencyControl>(control.PreviousInvoiceAmountBoundCurrencyUserControl);
	}

	public void TestPreviousInvoiceCurrencyExRateCalcEdit()
	{
		AssertType<ZCalcEdit>(control.PreviousInvoiceCurrencyExRateCalcEdit);
	}

	public void TestFinancialAndBankingDataLine1TextBox()
	{
		AssertType<ZTextBox>(control.FinancialAndBankingDataLine1TextBox);
	}

	public void TestFinancialAndBankingDataLine2TextBox()
	{
		AssertType<ZTextBox>(control.FinancialAndBankingDataLine2TextBox);
	}

	public void TestBoxElectronicDocumentsCheckBox()
	{
		AssertType<ZCheckBox>(control.BoxElectronicDocumentsCheckBox);
		AssertEquals("Alignment", ZContentAlignment.Left, control.BoxElectronicDocumentsCheckBox.CheckAlign);
	}

	public void TestUseDeclarationOfIntentCheckBox()
	{
		AssertType<ZCheckBox>(control.UseDeclarationOfIntentCheckBox);
		AssertEquals("Alignment", ZContentAlignment.Left, control.UseDeclarationOfIntentCheckBox.CheckAlign);
	}

	public void TestClearanceByEntryLineCheckBox()
	{
		AssertType<ZCheckBox>(control.ClearanceByEntryLineCheckBox);
		AssertEquals("Alignment", ZContentAlignment.Left, control.ClearanceByEntryLineCheckBox.CheckAlign);
	}

	public void TestSimplifiedDecAcceptanceDateEdit()
	{
		AssertType<ZDateEdit>(control.SimplifiedDecAcceptanceDateEdit);
	}

	public void TestCurrencyTextBox()
	{
		AssertType<ZTextBox>(control.CurrencyTextBox);
	}

	public void TestValuationCodeTextBox()
	{
		AssertType<ZTextBox>(control.ValuationCodeTextBox);
	}

	public void TestIncotermTextBox()
	{
		AssertType<ZTextBox>(control.IncotermTextBox);
	}

	public void TestTempProcLimitDateEdit()
	{
		AssertType<ZDateEdit>(control.TempProcLimitDateEdit);
	}

	public void TestProcedureCodeDropEdit()
	{
		AssertType<ZDropEdit>(control.ProcedureCodeDropEdit);
	}

	public void TestParticipantTypeDropEdit()
	{
		AssertType<ZDropEdit>(control.ParticipantTypeDropEdit);
	}

	public void TestOtherCustomsInformationLabel()
	{
		AssertType<ZLabel>(control.OtherCustomsInformationLabel);
	}

	public void TestToWarehouseLabel()
	{
		AssertType<ZLabel>(control.ToWarehouseLabel);
	}

	public void TestFromWarehouseLabel()
	{
		AssertType<ZLabel>(control.FromWarehouseLabel);
	}

	public void TestAssessmentDateEdit()
	{
		AssertType<ZDateEdit>(control.AssessmentDateEdit);
	}

	public void TestPreviousInvoiceLabel()
	{
		AssertType<ZLabel>(control.PreviousInvoiceLabel);
	}

	public void TestPresentationOfGoodsDateEdit()
	{
		AssertType<ZDateTimeOffsetEdit>(control.PresentationOfGoodsDateEdit);
	}

	public void TestFromWarehouseUserControl()
	{
		AssertType<FromWarehouseUserControl>(control.FromWarehouseUserControl);
	}

	public void TestToWarehouseUserControl()
	{
		AssertType<ToWarehouseUserControl>(control.ToWarehouseUserControl);
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new EntryInstructionBasicDetailsControl();
	}

	EntryInstructionBasicDetailsControl control;
}
