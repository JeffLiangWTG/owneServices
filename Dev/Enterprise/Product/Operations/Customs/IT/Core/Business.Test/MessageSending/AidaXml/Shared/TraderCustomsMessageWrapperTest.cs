using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class TraderCustomsMessageWrapperTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When traderWrapper is empty", () => new TraderCustomsMessageWrapper(null));
	}

	public void TestFields_WhenIdentificationNumberIsFilled()
	{
		var trader = GetNewTrader("IdentificationNumber");
		var exporterWrapper = (IEoriTrader)new TraderCustomsMessageWrapper(trader);

		CombineAssertions(() =>
		{
			AssertNull(nameof(ITrader.Address), exporterWrapper.Address);
			AssertEquals(nameof(ITrader.IdentificationNumber), "IdentificationNumber", exporterWrapper.IdentificationNumber);
			AssertEquals(nameof(IEoriTrader.EoriNumber), "IT30298u4", exporterWrapper.EoriNumber);
		});
	}

	public void TestFields_WhenIdentificationNumberIsEmpty()
	{
		var trader = GetNewTrader(null);
		var exporterWrapper = (IEoriTrader)new TraderCustomsMessageWrapper(trader);
		CombineAssertions(() => TraderWrapperAssertionHelper.AssertTrader(exporterWrapper, "StreetAndNumber", "City", "Country", expectedIdentificationNumber: null, "Name", "ZipCode"));
	}

	IEoriTrader GetNewTrader(string identificationNumber)
	{
		var traderMock = new Mock<IEoriTrader>();
		traderMock.Setup(x => x.Address.StreetAndNumber).Returns("StreetAndNumber");
		traderMock.Setup(x => x.Address.City).Returns("City");
		traderMock.Setup(x => x.Address.Country).Returns("Country");
		traderMock.Setup(x => x.IdentificationNumber).Returns(identificationNumber);
		traderMock.Setup(x => x.Address.Name).Returns("Name");
		traderMock.Setup(x => x.Address.ZipCode).Returns("ZipCode");
		traderMock.Setup(x => x.EoriNumber).Returns("IT30298u4");
		return traderMock.Object;
	}
}
