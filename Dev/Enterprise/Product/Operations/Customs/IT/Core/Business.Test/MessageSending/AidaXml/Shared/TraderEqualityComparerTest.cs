using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class TraderEqualityComparerTest : TestCase
{
	public void TestEquality()
	{
		var traderMock1 = GetNewTraderMock();
		var traderMock2 = GetNewTraderMock();
		var equalityComparer = new TraderEqualityComparer();
		CombineAssertions(() =>
		{
			AssertEquals("Same Eori, identification and address", true, equalityComparer.Equals(traderMock1.Object, traderMock2.Object));

			traderMock2.Setup(m => m.IdentificationNumber).Returns("2");
			AssertEquals("different identification", false, equalityComparer.Equals(traderMock1.Object, traderMock2.Object));

			traderMock2.Setup(m => m.IdentificationNumber).Returns("1");
			traderMock2.Setup(m => m.EoriNumber).Returns("IT12345");
			AssertEquals("Different Eori", false, equalityComparer.Equals(traderMock1.Object, traderMock2.Object));

			traderMock2.Setup(m => m.EoriNumber).Returns("IT30298u4");
			traderMock2.Setup(m => m.Address.City).Returns("DifferentCity");
			AssertEquals("Different address", false, equalityComparer.Equals(traderMock1.Object, traderMock2.Object));

			traderMock1.Setup(m => m.Address).Returns((IAddress)null);
			traderMock2.Setup(m => m.Address).Returns((IAddress)null);
			AssertEquals("Address is null for both traders", true, equalityComparer.Equals(traderMock1.Object, traderMock2.Object));

			AssertEquals("One object is null", false, equalityComparer.Equals(traderMock1.Object, null));

			AssertEquals("Both objects are null", true, equalityComparer.Equals(null, null));
		});
	}

	Mock<IEoriTrader> GetNewTraderMock()
	{
		var traderMock = new Mock<IEoriTrader>();
		traderMock.Setup(x => x.Address.StreetAndNumber).Returns("StreetAndNumber");
		traderMock.Setup(x => x.Address.City).Returns("City");
		traderMock.Setup(x => x.Address.Country).Returns("Country");
		traderMock.Setup(x => x.IdentificationNumber).Returns("1");
		traderMock.Setup(x => x.Address.Name).Returns("Name");
		traderMock.Setup(x => x.Address.ZipCode).Returns("ZipCode");
		traderMock.Setup(x => x.EoriNumber).Returns("IT30298u4");
		return traderMock;
	}
}
