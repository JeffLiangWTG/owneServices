using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing;

[TestedType(typeof(SafeFoodLicense))]
sealed class SafeFoodLicenseTest : CusCodeDataTest<SafeFoodLicense>
{
	public void TestValidation()
	{
		AssertEquals("Validation", typeof(SafeFoodLicenseValidation), SafeFoodLicense.Validation.GetType());
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(CusCodeDataTypeList.Codes.SafeFoodLicense, SafeFoodLicense.CY_Type);
	}

	public void TestParents()
	{
		var safeFoodLicenseCollection = new SafeFoodLicenseCollection(OrgHeader);
		var foodLicense = safeFoodLicenseCollection.AddNew();
		AssertEquals(foodLicense.Parent, OrgHeader);
	}

	public void TestCY_Data()
	{
		SafeFoodLicense.CY_Data = "123";
		AssertNoNotifications(SafeFoodLicense.CY_DataInfo);

		AssertEquals(100, SafeFoodLicense.CY_DataInfo.MaxLength);
		AssertEquals("Description", DataBoundResourceStrings.GetDataForProperty(SafeFoodLicense.CY_DataInfo).Caption);
	}

	public void TestCY_Code()
	{
		AssertEquals("License No", DataBoundResourceStrings.GetDataForProperty(SafeFoodLicense.CY_CodeInfo).Caption);
	}

	public void TestISafeFoodLicenseIsCorrectlySetup()
	{
		AssertEquals(typeof(SafeFoodLicense), ObjectFactory.GetType<Integration.Customs.CA.ISafeFoodLicense>());
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return OrgImpAddInfo.Get(factory.NewWithValidTestData<OrgHeader>()).SafeFoodLicenses.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return OrgImpAddInfo.SafeFoodLicenses.AddNew();
	}

	OrgHeader OrgHeader
	{
		get { return orgHeader ??= Factory.New<OrgHeader>(); }
	}
	OrgHeader orgHeader;

	OrgImpAddInfo OrgImpAddInfo
	{
		get { return orgImpAddInfo ??= OrgImpAddInfo.Get(OrgHeader); }
	}
	OrgImpAddInfo orgImpAddInfo;

	SafeFoodLicense SafeFoodLicense
	{
		get { return safeFoodLicense ??= Factory.New<SafeFoodLicense>(); }
	}
	SafeFoodLicense safeFoodLicense;
}
