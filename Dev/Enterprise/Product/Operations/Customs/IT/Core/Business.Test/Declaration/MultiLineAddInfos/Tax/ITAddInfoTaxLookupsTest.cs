using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ITAddInfoTaxLookupsTest : EUAddInfoTaxLookupsTest
{
	public void TestParent()
	{
		AssertNotNull("Add info parent should be not null", addInfoLookups.Parent);
	}

	public void TestPortTaxRateList()
	{
		new ITUniversalReferenceTestDataHelper(Factory).SetupPortTaxRates();
		Factory.Save();

		var portTaxRateList = addInfoLookups.PortTaxRateList;

		AssertEquals("3 Port Tax Rates are available", 3, portTaxRateList.Count);
		AssertEquals("Port Tax Rates lookups codes", "A1, A2, A3", portTaxRateList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();
		org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		var pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
		var addInfoTax = pivot.Taxes.AddNew();
		addInfoLookups = addInfoTax.Data.Lookups;
	}

	OrgHeader org;
	ITAddInfoTaxLookups addInfoLookups;
}
