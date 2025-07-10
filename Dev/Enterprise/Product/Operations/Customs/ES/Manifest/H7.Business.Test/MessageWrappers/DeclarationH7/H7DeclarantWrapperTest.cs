using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using OrgContact = Enterprise.MasterFiles.Business.OrgContact;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class H7DeclarantWrapperTest : DataProviderTestCase<H7DeclarantWrapper>
{
	public void TestContactInfo()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		AssertNull(wrapper.ContactInfo);

		contactAllocation.PC_Type = OrgConstants.ContactAllocationType.CUS;
		wrapper = new H7DeclarantWrapper(bill);
		AssertEquals("TestContact", wrapper.ContactInfo.Name);
	}

	public void TestAddress()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		AssertEquals("Address1 Address2", wrapper.Address);
	}

	public void TestCity()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		AssertEquals("City", wrapper.City);
	}

	public void TestCountry()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		AssertEquals(Constants.CountryCodes.Australia, wrapper.Country);
	}

	public void TestName()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		AssertEquals("OHFULLNAME", wrapper.Name);
	}

	public void TestIsImporter()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		Assert(!wrapper.IsImporter);
	}

	public void TestPostCode()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		AssertEquals("1234", wrapper.PostCode);
	}

	public void TestId()
	{
		var wrapper = new H7DeclarantWrapper(bill);
		var orgHeader = bill.Header.Declarant.Header;

		CombineAssertions(() =>
		{
			bill.Header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
			AssertNullOrEmpty("When AMA_AgentType is IND, the wrapper id is empty", wrapper.Id);

			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.DCA;
			AssertNullOrEmpty("When AMA_AgentType is DCA, the wrapper id is empty", wrapper.Id);

			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.ICA;
			AssertEquals("Expected empty Id", ZString.Empty, wrapper.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Still Expect empty Id", ZString.Empty, wrapper.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF Id", "NIF22222222", wrapper.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.Id);

			OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI Id with country code", "FR22222222", wrapper.Id);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.Id);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		bill = header.Bills.AddNew();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "OHFULLNAME";
		var address = orgHeader.Addresses.AddNew();
		header.AMA_OA_Declarant = address.PK;
		contact = orgHeader.Contacts.AddNew();
		contact.OC_ContactName = "TestContact";
		contactAllocation = contact.Allocations.AddNew();
		declarant = bill.Header.Declarant;
		declarant.Address1 = "Address1";
		declarant.Address2 = "Address2";
		declarant.City = "City";
		declarant.Postcode = "1234";
		declarant.OA_RN_NKCountryCode = Constants.CountryCodes.Australia;
	}

	protected override H7DeclarantWrapper GetProvider()
	{
		var bill = Factory.New<AsycudaBill>();
		return new H7DeclarantWrapper(bill);
	}

	OrgContact contact;
	OrgContactAllocation contactAllocation;
	AsycudaBill bill;
	OrgAddress declarant;
}
