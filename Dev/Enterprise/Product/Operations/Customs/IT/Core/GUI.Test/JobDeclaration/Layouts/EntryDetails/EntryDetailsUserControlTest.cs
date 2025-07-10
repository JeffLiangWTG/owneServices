using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryDetailsUserControlTest : TestCaseWithFactory
{
	public void TestEntryTypeTextBox()
	{
		AssertType<ZTextBox>(control.EntryTypeTextBox);
	}

	public void TestReferenceNumberTextBox()
	{
		AssertType<ZTextBox>(control.ReferenceNumberTextBox);
	}

	public void TestIssueDateDateEdit()
	{
		AssertType<ZDateEdit>(control.IssueDateDateEdit);
	}

	public void TestIncotermTextBox()
	{
		AssertType<ZTextBox>(control.IncotermTextBox);
	}

	public void TestGrossWeightUserControl()
	{
		AssertType<AmountAndUnitControl>(control.GrossWeightUserControl);

		var bindingMember = control.GrossWeightUserControl.GetBindingMember();
		AssertEquals("BindingMember", "CustomsEntryHeaders.TotalGrossWeightInKG", bindingMember);
	}

	public void TestNetWeightUserControl()
	{
		AssertType<AmountAndUnitControl>(control.NetWeightUserControl);

		var bindingMember = control.NetWeightUserControl.GetBindingMember();
		AssertEquals("BindingMember", "CustomsEntryHeaders.TotalNetWeightInKG", bindingMember);
	}

	public void TestCustomsQuantityUserControl()
	{
		AssertType<AmountAndUnitControl>(control.CustomsQuantityUserControl);

		var bindingMember = control.CustomsQuantityUserControl.GetBindingMember();
		AssertEquals("BindingMember", "CustomsEntryHeaders.TotalCustomsQuantity", bindingMember);
	}

	public void TestInvoiceAmountUserControl()
	{
		AssertType<InvoiceAmountUserControl>(control.InvoiceAmountUserControl);
	}

	public void TestFreightAdjustmentCalcEdit()
	{
		AssertType<ZCalcEdit>(control.FreightAdjustmentCalcEdit);
	}

	public void TestMessageStatusDropEdit()
	{
		AssertType<MessageStatusUserControl>(control.MessageStatusUserControl);
	}

	public void TestControlChannelDropEdit()
	{
		AssertType<ZDropEdit>(control.ControlChannelDropEdit);
	}

	public void TestMessageTypeDropEdit()
	{
		AssertType<ZDropEdit>(control.MessageTypeDropEdit);
	}
	public void TestWarehouseStatusDropEdit()
	{
		AssertType<ZDropEdit>(control.WarehouseStatusDropEdit);
	}

	public void TestRegistrationNumberTextBox()
	{
		AssertType<ZTextBox>(control.RegistrationNumberTextBox);
	}

	public void TestCustomsOfficeTextBox()
	{
		AssertType<ZTextBox>(control.CustomsOfficeTextBox);
	}

	public void TestReleaseCodeTextBox()
	{
		AssertType<ZTextBox>(control.ReleaseCodeTextBox);
	}

	public void TestA93Grid()
	{
		AssertType<ZGrid>(control.A93Grid);
	}

	public void TestExitDateDateEdit()
	{
		AssertType<ZDateEdit>(control.ExitDateDateEdit);
	}

	public void TestExitOfficeDropEdit()
	{
		AssertType<ExitOfficeUserControl>(control.ExitOfficeUserControl);
	}

	public void TestExitStatusDropEdit()
	{
		AssertType<ExitStatusUserControl>(control.ExitStatusUserControl);
	}

	public void TestReferenceLabel()
	{
		AssertType<GroupLabel>(nameof(control.ReferenceLabel), control.ReferenceLabel);
	}

	public void TestTotalsLabel()
	{
		AssertType<GroupLabel>(nameof(control.TotalsLabel), control.TotalsLabel);
	}

	public void TestStatusLabel()
	{
		AssertType<GroupLabel>(nameof(control.StatusLabel), control.StatusLabel);
	}

	public void TestCustomsLabel()
	{
		AssertType<GroupLabel>(nameof(control.CustomsLabel), control.CustomsLabel);
	}

	public void TestA93Label()
	{
		AssertType<GroupLabel>(nameof(control.A93Label), control.A93Label);
	}

	public void TestExitLabel()
	{
		AssertType<GroupLabel>(nameof(control.ExitLabel), control.ExitLabel);
	}

	public void TestA93GridColumns()
	{
		var grid = control.A93Grid;
		CombineAssertions("Columns should not be null in A93 Grid", () =>
		{
			AssertNotNull("C9_TransactionType", grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_TransactionType));
			AssertNotNull("A93Number", grid.GetColumnStyle(CusEntryPayInfo.Schema.A93Number));
			AssertNotNull("MethodOfPayment", grid.GetColumnStyle(CusEntryPayInfo.Schema.MethodOfPayment));
			AssertNotNull("C9_PaymentAmount", grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_PaymentAmount));
			AssertNotNull("C9_PaymentDate", grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_PaymentDate));
		});

		CombineAssertions("Columns setup", () =>
		{
			Assert("C9_TransactionType", grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_TransactionType).IsVisible);
			Assert("A93Number", grid.GetColumnStyle(CusEntryPayInfo.Schema.A93Number).IsVisible);
			Assert("MethodOfPayment", grid.GetColumnStyle(CusEntryPayInfo.Schema.MethodOfPayment).IsVisible);
			Assert("C9_PaymentAmount", grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_PaymentAmount).IsVisible);
			AssertEquals("C9_PaymentAmount Decimals", 2, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_PaymentAmount)).Decimals);
			Assert("C9_PaymentDate", grid.GetColumnStyle(CusEntryPayInfo.Schema.C9_PaymentDate).IsVisible);
		});
	}

	public void TestEntryStatusDropEdit()
	{
		AssertType<ZDropEdit>(control.EntryStatusDropEdit);
	}

	public void TesteSubmittedDateDateEdit()
	{
		AssertType<ZDateEdit>(control.SubmittedDateDateEdit);
	}

	public void TestMRNTextBox()
	{
		AssertType<ZTextBox>(control.MRNTextBox);
	}

	public void TestReleaseDateDateEdit()
	{
		AssertType<ZDateEdit>(control.ReleaseDateDateEdit);
	}

	public void TestInvoiceAmountUserControlCaption()
	{
		var sut = control.InvoiceAmountUserControl;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertEquals("InvoiceAmountUserControl caption", "Invoice Amount", sut.CaptionResourceString.Caption);
		AssertEquals("InvoiceAmountUserControl caption visible", expected: true, labelCaptionVisible);
	}

	public void TestMessageStatusUserControlCaption()
	{
		var sut = control.MessageStatusUserControl;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertEquals("MessageStatusUserControl caption", "Message Status", sut.CaptionResourceString.Caption);
		AssertEquals("MessageStatusUserControl caption visible", expected: true, labelCaptionVisible);
	}

	public void TestExitOfficeUserControlCaption()
	{
		var sut = control.ExitOfficeUserControl;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertEquals("ExitOfficeUserControl caption", "Exit Office", sut.CaptionResourceString.Caption);
		AssertEquals("ExitOfficeUserControl caption visible", expected: true, labelCaptionVisible);
	}

	public void TestExitStatusUserControlCaption()
	{
		var sut = control.ExitStatusUserControl;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertEquals("ExitStatusUserControl caption", "Exit Status", sut.CaptionResourceString.Caption);
		AssertEquals("ExitStatusUserControl caption visible", expected: true, labelCaptionVisible);
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new EntryDetailsUserControl();
	}

	EntryDetailsUserControl control;
}
