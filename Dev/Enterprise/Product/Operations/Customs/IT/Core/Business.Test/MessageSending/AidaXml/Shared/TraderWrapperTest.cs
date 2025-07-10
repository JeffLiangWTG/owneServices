using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class TraderWrapperTest : TestCaseWithFactory
{
	public void TestAddress()
	{
		var traderWrapper = GetNewTraderWrapperWithValidAddress();
		AssertType<AddressWrapper>(nameof(ITrader.Address), traderWrapper.Address);
	}

	public void TestAddressWhenEmpty()
	{
		var traderWrapper = GetNewTraderWrapperWithEmptyAddress();
		AssertNull(nameof(ITrader.Address), traderWrapper.Address);
	}

	public void TestIdentificationNumber()
	{
		var traderWrapper = GetNewTraderWrapperWithValidAddress();
		AssertEquals(nameof(ITrader.IdentificationNumber), "", traderWrapper.IdentificationNumber);

		orgAddress.CustomsCodes.AddNew("IVA", "123456789", "DE");
		traderWrapper = GetNewTraderWrapperWithValidAddress();
		AssertEquals(nameof(ITrader.IdentificationNumber), "DE123456789", traderWrapper.IdentificationNumber);

		orgAddress.CustomsCodes.AddNew("EOR", "385040449", "IT");
		traderWrapper = GetNewTraderWrapperWithValidAddress();
		AssertEquals(nameof(ITrader.IdentificationNumber), "IT385040449", traderWrapper.IdentificationNumber);
	}

	public void TestEoriNumber()
	{
		var traderWrapper = GetNewTraderWrapperWithValidAddress();
		AssertNullOrEmpty(nameof(IEoriTrader.EoriNumber), traderWrapper.EoriNumber);

		orgAddress.CustomsCodes.AddNew("EOR", "385040449", "IT");
		traderWrapper = GetNewTraderWrapperWithValidAddress();
		AssertEquals(nameof(IEoriTrader.EoriNumber), "IT385040449", traderWrapper.EoriNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgAddress = Factory.New<OrgHeader>().MainAddress;
		jobDocAddress = Factory.New<JobDocAddress>();
	}

	OrgAddress orgAddress;
	JobDocAddress jobDocAddress;

	IEoriTrader GetNewTraderWrapperWithValidAddress()
	{
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		return new TraderWrapper(jobDocAddress);
	}

	IEoriTrader GetNewTraderWrapperWithEmptyAddress()
	{
		return new TraderWrapper(jobDocAddress: null);
	}

	public static Mock<IEoriTrader> SetupTrader(string identificationNumber)
	{
		var addressMock = new Mock<IAddress>();
		addressMock.Setup(s => s.Name).Returns($"{identificationNumber}_ADDRESS");
		var traderMock = new Mock<IEoriTrader>();
		traderMock.Setup(t => t.IdentificationNumber).Returns(identificationNumber);
		traderMock.Setup(t => t.Address).Returns(addressMock.Object);
		return traderMock;
	}
}
