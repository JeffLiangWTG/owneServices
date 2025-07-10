using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class VatTypeAndDescriptionUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
	}

	public void TestVatTypeDropEdit()
	{
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Control Type", control.VatTypeDropEdit);
			AssertEquals("Label Visible", false, control.VatTypeDropEdit.GetExtension<ZLabelCaptionRenderer>().Visible);
		});
	}

	public void TestVatRateDescriptionTextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.VatRateDescriptionTextBox);
			AssertEquals("Label Visible", false, control.VatRateDescriptionTextBox.GetExtension<ZLabelCaptionRenderer>().Visible);
		});
	}

	public void TestIExtendedControl()
	{
		CombineAssertions(() =>
		{
			var extendedControlSupporter = (IExtendedControl)control;
			AssertSame("Host", control, extendedControlSupporter.Host);
			AssertType<DefaultControlExtensionCollection>("Extensions", extendedControlSupporter.Extensions);
		});
	}

	public void TestIResourceStringBindingMember()
	{
		var resourceStringBindingMemberSupporter = (IResourceStringBindingMember)control;
		AssertEquals("ResourceStringBindingMember", "JI_ZZF_NKTaxType", resourceStringBindingMemberSupporter.ResourceStringBindingMember);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new VatTypeAndDescriptionUserControl();
	}

	VatTypeAndDescriptionUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
