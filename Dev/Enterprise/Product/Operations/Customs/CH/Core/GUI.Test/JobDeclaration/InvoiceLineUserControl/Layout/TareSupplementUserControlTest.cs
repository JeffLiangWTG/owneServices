using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

class TareSupplementUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new TareSupplementUserControl())
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new TareSupplementUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("TareSupplementCalcEdit.Visible", true, control.TareSupplementCalcEdit.Visible);
				AssertEquals("TareSupplementCheckBox.Visible", true, control.TareSupplementCheckBox.Visible);
			});
		}
	}
}
