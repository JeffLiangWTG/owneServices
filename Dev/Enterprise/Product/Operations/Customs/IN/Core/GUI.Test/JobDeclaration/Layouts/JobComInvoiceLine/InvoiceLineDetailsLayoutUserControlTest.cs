using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsLayoutUserControl))]
public class InvoiceLineDetailsLayoutUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new InvoiceLineDetailsLayoutUserControl();
		AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
	}

	public void TestControls()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();

		using var form = new ZForm();
		using var control = new InvoiceLineDetailsLayoutUserControl();
		control.SetDataBinding(invoiceLine, "");
		form.Controls.Add(control);
		form.Show();
		AssertEquals(99999999999.99999m, control.UnitPriceCalcFindBox.MaxValue);
		AssertEquals(5, control.UnitPriceCalcFindBox.Decimals);
		AssertEquals(99999999m, control.UnitQuantityCalcDropEdit.MaxValue);
		AssertEquals(0, control.UnitQuantityCalcDropEdit.Decimals);
	}
}
