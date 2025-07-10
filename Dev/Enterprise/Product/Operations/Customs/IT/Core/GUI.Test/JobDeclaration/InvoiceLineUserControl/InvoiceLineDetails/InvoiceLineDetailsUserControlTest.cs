using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
	}

	public void TestPortTaxRateDropEdit()
	{
		AssertType<ZDropEdit>(control.PortTaxRateDropEdit);
	}

	public void TestGoodsOriginDropEdit()
	{
		AssertType<ZDropEdit>(control.GoodsOriginDropEdit);
	}

	public void TestOriginCountryStateUserControl()
	{
		AssertType<OriginCountryStateUserControl>(control.OriginCountryStateUserControl);
	}

	public void TestVatTypeAndDescriptionUserControl()
	{
		AssertType<VatTypeAndDescriptionUserControl>(control.VatTypeAndDescriptionUserControl);
	}

	public void TestCountryOfDestinationDropEdit()
	{
		AssertType<ZDropEdit>(control.CountryOfDestinationDropEdit);
	}

	public void TestCountryOfExportDropEdit()
	{
		control.CountryOfExportDropEdit.AssertThisControl(c => c
			.OfType<ZDropEdit>()
			.WithBindTo("JI_RN_NKCountryOfExport")
			.WithCaption("Country of Export")
			.WithFullDescription("Country of Export of the goods being moved."));
	}

	public void TestInvoiceNumberDropEdit()
	{
		control.InvoiceNumberDropEdit.AssertThisControl(c => c
			.OfType<ZDropEdit>()
			.WithBindTo("JI_Calc_Invoice"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new InvoiceLineDetailsUserControl();
	}

	InvoiceLineDetailsUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
