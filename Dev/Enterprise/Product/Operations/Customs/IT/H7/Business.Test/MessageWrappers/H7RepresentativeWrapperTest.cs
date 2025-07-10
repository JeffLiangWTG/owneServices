using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7RepresentativeWrapper))]
public sealed class H7RepresentativeWrapperTest : DataProviderTestCase<H7RepresentativeWrapper>
{
	public void TestAddress()
	{
		AssertNull(nameof(Provider.Address), Provider.Address);
	}

	public void TestRepresentativeType()
	{
		SetUpTestData();
		var wrapper = new H7RepresentativeWrapper(header);
		CombineAssertions(() =>
		{
			header.AMA_AgentType = "aaa";
			AssertNull(wrapper.RepresentativeType);

			header.AMA_AgentType = EUH7AgentTypes.Codes.DIR;
			AssertEquals(2, wrapper.RepresentativeType);

			header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
			AssertEquals(3, wrapper.RepresentativeType);
		});
	}

	public void TestEoriNumber()
	{
		AssertNull(nameof(Provider.EoriNumber), Provider.EoriNumber);
	}

	public void TestIdentificationNumber()
	{
		AssertEquals(nameof(Provider.IdentificationNumber), "RepRegNo123", Provider.IdentificationNumber);
	}

	public void TestIdentificationNumber_NoExceptionWhenOrgHasNoEORI()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_OA_Representative = orgHeader.MainAddress.PK;

		var provider = new H7RepresentativeWrapper(header);
		AssertNoExceptionThrown("No exception when org has no EORI", () => AssertNullOrEmpty(provider.IdentificationNumber));
	}

	void SetUpTestData()
	{
		header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
	}

	protected override H7RepresentativeWrapper GetProvider()
	{
		var header = Factory.New<AsycudaManifestHeader>();

		var orgHeader = Factory.New<OrgHeader>();
		var address = orgHeader.Addresses.AddNew();
		var customCode = address.CustomsCodes.AddNew("EOR", "RepRegNo123", "IT");

		header.AMA_OA_Representative = address.PK;

		return new H7RepresentativeWrapper(header);
	}

	AsycudaManifestHeader header;
}
