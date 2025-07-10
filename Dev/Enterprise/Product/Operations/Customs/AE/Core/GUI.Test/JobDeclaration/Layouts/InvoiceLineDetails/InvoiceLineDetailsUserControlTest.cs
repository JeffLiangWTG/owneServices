using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new InvoiceLineDetailsUserControl();
		AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
	}

	public void TestControls()
	{
		using var control = new InvoiceLineDetailsUserControl();
		AssertType<ZDropEdit>(control.Find(c => c.Name == "GoodsConditionDropEdit").First());
	}
}
