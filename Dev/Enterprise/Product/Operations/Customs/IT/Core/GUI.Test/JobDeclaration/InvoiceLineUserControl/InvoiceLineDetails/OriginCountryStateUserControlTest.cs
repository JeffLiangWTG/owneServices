using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class OriginCountryStateUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
	}

	public void TestGoodsOriginDropEdit()
	{
		var goodsOriginDropEdit = control.GoodsOriginDropEdit;

		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>(goodsOriginDropEdit);

			AssertNull("Caption", goodsOriginDropEdit.CaptionResourceString.Caption);
			AssertEquals("Caption Visible", false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(goodsOriginDropEdit));
		});
	}

	public void TestOriginStateDropEdit()
	{
		var originStateDropEdit = control.OriginStateDropEdit;

		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>(originStateDropEdit);

			AssertNull("Caption", originStateDropEdit.CaptionResourceString.Caption);
			AssertEquals("Caption Visible", false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(originStateDropEdit));
		});
	}

	public void TestIExtendedControl()
	{
		AssertEquals("Control implements IExtendedControl", true, control is IExtendedControl);

		IExtendedControl extendedControl = control;
		CombineAssertions(() =>
		{
			AssertSame("Host", control, extendedControl.Host);
			AssertType<DefaultControlExtensionCollection>("Extensions", extendedControl.Extensions);
		});
	}

	public void TestIResourceStringBindingMember()
	{
		AssertEquals("Control implements IResourceStringBindingMember", true, control is IResourceStringBindingMember);

		IResourceStringBindingMember resourceStringBindingMember = control;
		AssertEquals("ResourceStringBindingMember", "JI_CountryOfOrigin", resourceStringBindingMember.ResourceStringBindingMember);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new OriginCountryStateUserControl();
	}

	OriginCountryStateUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
