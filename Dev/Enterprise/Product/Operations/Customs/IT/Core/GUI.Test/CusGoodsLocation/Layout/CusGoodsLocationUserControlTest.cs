using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class CusGoodsLocationUserControlTest : TestCaseWithFactory
{
	public void TestAdditionalIdentifierDropEdit()
	{
		var additionalIdentifierDropEdit = control.AdditionalIdentifierDropEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", additionalIdentifierDropEdit);
			AssertEquals("BindTo", nameof(CusGoodsLocation.CGL_AdditionalIdentifier), additionalIdentifierDropEdit.BindTo);
			AssertEquals("ShowDescriptionBox", false, additionalIdentifierDropEdit.ShowDescriptionBox);
		});
	}

	public void TestOrganizationAddressControl()
	{
		var organizationAddressControl = control.OrganizationAddressControl;
		CombineAssertions(() =>
		{
			AssertType<ZDocAddressControl>("Type", organizationAddressControl);
			AssertEquals("BindTo", nameof(CusGoodsLocation.Address), organizationAddressControl.BindTo);
			AssertEquals("BindToOrganisations", "Lookups.OrganisationList", organizationAddressControl.BindToOrganisations);
			AssertEquals("Caption", "Organization", organizationAddressControl.CaptionResourceString.Caption);
		});
	}

	public void TestOverrideCheckBox()
	{
		var overrideCheckBox = control.OverrideCheckBox;
		CombineAssertions(() =>
		{
			AssertType<ZCheckBox>("Type", overrideCheckBox);
			AssertEquals("BindTo", $"{nameof(CusGoodsLocation.Address)}.{nameof(CusGoodsLocationAddress.E2_AddressOverride)}", overrideCheckBox.BindTo);
			AssertEquals("Caption", "Override", overrideCheckBox.CaptionResourceString.Caption);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new CusGoodsLocationUserControl();
	}
	CusGoodsLocationUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
