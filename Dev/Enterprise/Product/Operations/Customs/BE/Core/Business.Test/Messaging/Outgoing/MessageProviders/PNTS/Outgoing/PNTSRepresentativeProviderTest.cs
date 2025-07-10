using CargoWise.Types;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSRepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSRepresentativeProvider>
{
	public void TestIdentificationNumber()
	{
		AssertEquals("BE12345", provider.IdentificationNumber);
	}

	public void TestName()
	{
		AssertEquals("SBCompany", provider.Name);
	}

	public void TestCommunication()
	{
		var cusContact = orgHeader.Contacts.AddNew();
		cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		cusContact.OC_Email = "ShaGou@163.com";
		CombineAssertions(() =>
		{
			AssertEquals("ShaGou@163.com", provider.Communication.Identifier);
			AssertEquals("EM", provider.Communication.Type);
		});
	}

	public void TestStatus()
	{
		temporaryStorageHeader.AMA_OA_Declarant = orgAddress.PK;
		AssertEquals(3, provider.Status);

		temporaryStorageHeader.AMA_OA_Declarant = Factory.New<OrgAddress>().PK;
		provider = new PNTSRepresentativeProvider(temporaryStorageHeader);
		AssertEquals(2, provider.Status);

		temporaryStorageHeader.AMA_OA_Declarant = ZGuid.Empty;
		provider = new PNTSRepresentativeProvider(temporaryStorageHeader);
		AssertNull(provider.Status);
	}

	protected override PNTSRepresentativeProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		orgHeader.OH_FullName = "SBCompany";
		orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		temporaryStorageHeader.AMA_OA_Representative = orgAddress.PK;
		provider = new PNTSRepresentativeProvider(temporaryStorageHeader);
	}
	OrgAddress orgAddress;
	OrgHeader orgHeader;
	PNTSRepresentativeProvider provider;
	TemporaryStorageHeader temporaryStorageHeader;
}
