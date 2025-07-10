using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class UNDGCodesControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobComInvoiceLine), Control.BindingSource.DataSourceType);
	}

	public void TestTextBox()
	{
		AssertEquals("BindTo", nameof(JobComInvoiceLine.UNDGCodes), Control.UNDGCodesTextBox.BindTo);
	}

	public void TestMoreButton()
	{
		AssertEquals("Caption", "More\u2026", Control.MoreButton.CaptionResourceString.Caption);
	}

	public void TestMoreButtonClickEvent_Click() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var invoiceLineUserControl = new ExportInvoiceLineUserControl())
		{
			form.Controls.Add(invoiceLineUserControl);
			form.Show();
			var undgControl = invoiceLineUserControl.FindSingle<UNDGCodesUserControl>();
			undgControl.MoreButton.PerformClick();
			AssertType<UNDGDataItemForm>("Dangerous Goods form shown", ZFormModaliser.LastFormShownForTest);
			Assert(true);
		}
	});

	protected override void TearDown()
	{
		base.TearDown();
		control?.Dispose();
	}

	UNDGCodesUserControl Control => control ??= new UNDGCodesUserControl();
	UNDGCodesUserControl control;
}
