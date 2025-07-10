using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.DeclarationH7;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class H7RepresentativeWrapperTest : DataProviderTestCase<H7RepresentativeWrapper>
{
	public void TestContactInfo()
	{
		var wrapper = new H7RepresentativeWrapper(bill);

		CombineAssertions(() =>
		{
			AssertNull(wrapper.ContactInfo);

			bill.Header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
			AssertNull("When AMA_AgentType is IND, the ContactInfo is null", wrapper.ContactInfo);

			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.DCA;
			AssertNull("When AMA_AgentType is DCA, the ContactInfo is null", wrapper.ContactInfo);

			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.ICA;
			contactAllocation.PC_Type = OrgConstants.ContactAllocationType.CUS;
			wrapper = new H7RepresentativeWrapper(bill);
			AssertEquals("TestContact", wrapper.ContactInfo.Name);
		});
	}

	public void TestStatus()
	{
		var wrapper = new H7RepresentativeWrapper(bill);

		CombineAssertions(() =>
		{
			bill.Header.AMA_AgentType = EUH7AgentTypes.Codes.DIR;
			AssertEquals(2, wrapper.Status);

			bill.Header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
			AssertEquals(3, wrapper.Status);

			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.DCA;
			AssertEquals(4, wrapper.Status);

			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.ICA;
			AssertEquals(5, wrapper.Status);
		});
	}

	public void TestId()
	{
		var wrapper = new H7RepresentativeWrapper(bill);

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
		orgHeader = Factory.New<OrgHeader>();
		var address = orgHeader.Addresses.AddNew();
		header.AMA_OA_Representative = address.PK;
		contact = orgHeader.Contacts.AddNew();
		contact.OC_ContactName = "TestContact";
		contactAllocation = contact.Allocations.AddNew();
	}

	protected override H7RepresentativeWrapper GetProvider()
	{
		var bill = Factory.New<AsycudaBill>();
		return new H7RepresentativeWrapper(bill);
	}

	OrgContact contact;
	OrgContactAllocation contactAllocation;
	AsycudaBill bill;
	OrgHeader orgHeader;
}
