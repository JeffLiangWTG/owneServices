using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.IN.GUI.Testing;

abstract class BaseOrganisationPlugInAbstractTest : ZPlugInGenericTest
{
	public void TestName()
	{
		using var plugIn = GetPlugInToTest();
		AssertEquals(ExpectedPlugInName, plugIn.Name);
	}

	public void TestType()
	{
		using var plugIn = GetPlugInToTest();
		AssertType(ExpectPlugInUserControl, plugIn.UserControl);
	}

	public void TestChangeTheVisibility()
	{
		using var plugIn = GetPlugInToTest();
		CombineAssertions(() =>
		{
			AssertEquals("Should enable for IN", true, plugIn.Enabled);
			using var plugIn2 = GetPlugInToTest(Core.Constants.CountryCodes.Australia);
			AssertEquals("Should not enable for other country", false, plugIn2.Enabled);
		});
	}

	protected override ZPlugIn GetPlugInToTest() => GetPlugInToTest(Core.Constants.CountryCodes.India);

	ZPlugIn GetPlugInToTest(string countryCode)
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		organisation.MainAddress.OA_RN_NKCountryCode = countryCode;
		var countryData = Factory.New<OrgCountryData>();
		countryData.OV_OH_OrgHeader = organisation.PK;
		countryData.OV_RN_NKClientCountryRelation = countryCode;
		return CreateNewPlugIn(organisation);
	}

	protected abstract Type ExpectPlugInUserControl { get; }

	protected virtual string ExpectedPlugInName => "Customs Defaults";

	protected abstract BaseOrganisationPlugIn CreateNewPlugIn(OrgHeader organisation);
}
