using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class H7ImporterWrapperTest : DataProviderTestCase<H7ImporterWrapper>
{
	public void TestContactInfo()
	{
		bill1.ABL_ConsigneeName = "ConsigneeName";
		bill1.ABL_ConsigneeEmail = "ConsigneeEmail@email.com";
		bill1.ABL_ConsigneePhone = "123456789";

		var wrapper = new H7ImporterWrapper(bill1);
		CombineAssertions("Should ContactInfo correctly", () =>
		{
			AssertEquals("ConsigneeName", wrapper.ContactInfo.Name);
			AssertEquals("ConsigneeEmail@email.com", wrapper.ContactInfo.Email);
			AssertEquals("123456789", wrapper.ContactInfo.PhoneNumber);
		});

		var cusContact = orgHeader.Contacts.AddNew();
		var allocation = cusContact.Allocations.AddNew();
		allocation.PC_Type = OrgConstants.ContactAllocationType.CUS;
		cusContact.OC_Email = "123cus@email.com";
		cusContact.OC_Mobile = "987654321";

		wrapper = new H7ImporterWrapper(bill1);
		CombineAssertions("Should map to correct fields", () =>
		{
			AssertEquals("ConsigneeName", wrapper.ContactInfo.Name);
			AssertEquals("123cus@email.com", wrapper.ContactInfo.Email);
			AssertEquals("987654321", wrapper.ContactInfo.PhoneNumber);
		});
	}

	public void TestAddress()
	{
		bill1.ABL_ConsigneeStreet1 = "Street1";
		bill1.ABL_ConsigneeStreet2 = "Street2";

		var wrapper = new H7ImporterWrapper(bill1);
		AssertEquals("Street1 Street2", wrapper.Address);
	}

	public void TestCity()
	{
		bill1.ABL_ConsigneeCity = "City";

		var wrapper = new H7ImporterWrapper(bill1);
		AssertEquals("City", wrapper.City);
	}

	public void TestPostCode()
	{
		bill1.ABL_ConsigneePostcode = "PostCode";

		var wrapper = new H7ImporterWrapper(bill1);
		AssertEquals("PostCode", wrapper.PostCode);
	}

	public void TestCountry()
	{
		bill1.ABL_RN_NKConsigneeCountry = Constants.CountryCodes.Australia;

		var wrapper = new H7ImporterWrapper(bill1);
		AssertEquals(Constants.CountryCodes.Australia, wrapper.Country);
	}

	public void TestId()
	{
		bill2.ABL_ConsigneeRegNo = "ConsigneeRegNo1";
		var wrapper1 = new H7ImporterWrapper(bill2);
		AssertEquals("Empty Consignee", "ConsigneeRegNo1", wrapper1.Id);

		bill1.ABL_ConsigneeRegNo = "ConsigneeRegNo2";
		orgCusCode.OK_CustomsRegNo = "CustomsRegNo2";
		var wrapper2 = new H7ImporterWrapper(bill1);
		AssertEquals(ZString.Empty, wrapper2.Id);

		orgCusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
		AssertEquals("NIF code type", "CustomsRegNo2", wrapper2.Id);

		orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		orgCusCode.OK_CustomsRegNo = "CustomsRegNo3";
		AssertEquals("EORI code Type", "ESCUSTOMSREGNO3", wrapper2.Id);
	}

	public void TestName()
	{
		var wrapper1 = new H7ImporterWrapper(bill1);
		AssertEquals(ZString.Empty, wrapper1.Name);

		bill1.ABL_ConsigneeName = "ConsigneeName";
		var wrapper2 = new H7ImporterWrapper(bill1);
		AssertEquals("ConsigneeName", wrapper2.Name);
	}

	public void TestIsParticular()
	{
		var wrapper1 = new H7ImporterWrapper(bill1);
		AssertEquals("IsParticular should be false because Org's OH_Category is not 'NAT'", false, wrapper1.IsParticular);

		orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		AssertEquals("IsParticular should be true because Org's OH_Category is 'NAT'", true, wrapper1.IsParticular);

		var wrapper2 = new H7ImporterWrapper(bill2);
		AssertEquals("IsParticular should be true because Consignee is not an Org", true, wrapper2.IsParticular);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		bill1 = header.Bills.AddNew();
		orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		bill1.ABL_OA_Consignee = orgAddress.PK;
		bill2 = header.Bills.AddNew();

		orgCusCode = orgHeader.CustomsCodes.AddNew();
	}

	protected override H7ImporterWrapper GetProvider()
	{
		var bill = Factory.New<AsycudaBill>();
		return new H7ImporterWrapper(bill);
	}

	OrgCusCode orgCusCode;

	AsycudaBill bill1;
	AsycudaBill bill2;
	OrgHeader orgHeader;
}
