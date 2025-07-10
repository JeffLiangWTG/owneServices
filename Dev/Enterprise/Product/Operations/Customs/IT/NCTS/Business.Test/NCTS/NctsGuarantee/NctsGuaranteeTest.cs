using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsGuarantee))]
sealed class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => GetNewPhase5Guarantee(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewPhase5Guarantee(factory);

	public void TestValidationType_Phase5()
	{
		var nctsGuarantee = GetNewPhase5Guarantee(Factory);
		AssertType<NctsGuaranteePhase5Validation>(nctsGuarantee.Validation);
	}

	public void TestValidationType_Phase4()
	{
		var nctsGuarantee = GetNewPhase4Guarantee();
		AssertType<NctsGuaranteePhase4Validation>(nctsGuarantee.Validation);
	}

	public void TestLookupType_Phase5()
	{
		var nctsGuarantee = GetNewPhase5Guarantee(Factory);
		AssertType<NctsPhase5GuaranteeLookups>(nctsGuarantee.Lookups);
	}

	public void TestLookupType_Phase4()
	{
		var nctsGuarantee = GetNewPhase4Guarantee();
		AssertType<NctsGuaranteeLookups>(nctsGuarantee.Lookups);
	}

	public void TestOfficeDescription()
	{
		CustomsOfficeTestDataHelper.SetupCustomsOfficeData(Factory);
		Factory.Save();

		var nctsGuarantee = GetNewPhase5Guarantee(Factory);
		nctsGuarantee.PW_BondFiledPort = ZString.Empty;
		AssertEquals("[Pre-Condition] Empty Office Description", ZString.Empty, nctsGuarantee.OfficeDescription);

		nctsGuarantee.PW_BondFiledPort = "IT303199";
		AssertEquals("When Office Code is valid", "CAMPOBASSO", nctsGuarantee.OfficeDescription);

		nctsGuarantee.PW_BondFiledPort = "IT212122";
		AssertEquals("When Office Code is valid however with an invalid ROLE value", ZString.Empty, nctsGuarantee.OfficeDescription);

		nctsGuarantee.PW_BondFiledPort = "DE122122";
		AssertEquals("When Office Code is invalid", ZString.Empty, nctsGuarantee.OfficeDescription);
	}

	public void TestOfficeCaptions()
	{
		var nctsGuarantee = GetNewPhase5Guarantee(Factory);
		var officeCaptionResourceStringData = DataBoundResourceStrings.GetDataForProperty(nctsGuarantee.PW_BondFiledPortInfo);
		AssertNotNull(officeCaptionResourceStringData);
		AssertEquals("Caption for PW_BondFiledPort", "Office", officeCaptionResourceStringData.Caption);
		AssertEquals("FullDescription for PW_BondFiledPort", "Customs Office of Guarantee", officeCaptionResourceStringData.FullDescription);

		var officeDescriptionResourceStringData = DataBoundResourceStrings.GetDataForProperty(nctsGuarantee.OfficeDescriptionInfo);
		AssertNotNull(officeDescriptionResourceStringData);
		AssertEquals("Caption for OfficeDescription", "Office Desc.", officeDescriptionResourceStringData.Caption);
	}

	NctsGuarantee GetNewPhase4Guarantee()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		return nctsHeader.Guarantees.AddNew();
	}

	NctsGuarantee GetNewPhase5Guarantee(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.NewDepartureNctsHeaderPhase5();
		return nctsHeader.MovementHeader.Guarantees.AddNew();
	}
}
