using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(PMVFieldsUserControl))]
public class PMVFieldsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new PMVFieldsUserControl();
		AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
	}

	public void TestControls()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();

		using var form = new ZForm();
		using var control = new PMVFieldsUserControl();
		control.SetDataBinding(invoiceLine, "");
		form.Controls.Add(control);
		form.Show();
		AssertEquals(999.99m, control.ValuationMarkupCalcEdit.MaxValue);
		AssertEquals(2, control.ValuationMarkupCalcEdit.Decimals);
		AssertEquals(9999999999999.99m, control.PMVCalcEdit.MaxValue);
		AssertEquals(2, control.PMVCalcEdit.Decimals);
		AssertEquals("%", control.PercentageLabel.Text);
	}
}
