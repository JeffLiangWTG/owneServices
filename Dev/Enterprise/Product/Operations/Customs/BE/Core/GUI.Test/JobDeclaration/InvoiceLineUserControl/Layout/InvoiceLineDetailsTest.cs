using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class InvoiceLineDetailsTest : TestCaseWithFactory
{
	public void TestUCRReferenceTextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", control.UCRReferenceTextBox);
			AssertEquals("BindTo", nameof(JobComInvoiceLine.ZG_UCRReference), control.UCRReferenceTextBox.BindTo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new InvoiceLineDetails();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	InvoiceLineDetails control;
}
