using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class H7ExporterNonOrganizationWrapperTest : DataProviderTestCase<H7ExporterNonOrganizationWrapper>
{
	public void TestId()
	{
		var exporterWrapper = new H7ExporterNonOrganizationWrapper(bill);
		AssertEquals(ZString.Empty, exporterWrapper.Id);
	}

	public void TestName()
	{
		bill.ABL_ShipperName = "ShipperName";
		var exporterWrapper = new H7ExporterNonOrganizationWrapper(bill);
		AssertEquals("ShipperName", exporterWrapper.Name);
	}

	public void TestAddress()
	{
		bill.ABL_ShipperStreet1 = "ShipperStreet1";
		bill.ABL_ShipperStreet2 = "ShipperStreet2";
		var exporterWrapper = new H7ExporterNonOrganizationWrapper(bill);
		AssertEquals("ShipperStreet1 ShipperStreet2", exporterWrapper.Address);
	}

	public void TestCity()
	{
		bill.ABL_ShipperCity = "ShipperCity";
		var exporterWrapper = new H7ExporterNonOrganizationWrapper(bill);
		AssertEquals("ShipperCity", exporterWrapper.City);
	}

	public void TestPostCode()
	{
		bill.ABL_ShipperPostcode = "123456";
		var exporterWrapper = new H7ExporterNonOrganizationWrapper(bill);
		AssertEquals("123456", exporterWrapper.PostCode);
	}

	public void TestCountry()
	{
		bill.ABL_RN_NKShipperCountry = Constants.CountryCodes.Spain;
		var exporterWrapper = new H7ExporterNonOrganizationWrapper(bill);
		AssertEquals(Constants.CountryCodes.Spain, exporterWrapper.Country);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		bill = header.Bills.AddNew();
	}

	protected override H7ExporterNonOrganizationWrapper GetProvider()
	{
		return new H7ExporterNonOrganizationWrapper(bill);
	}

	AsycudaBill bill;
}
