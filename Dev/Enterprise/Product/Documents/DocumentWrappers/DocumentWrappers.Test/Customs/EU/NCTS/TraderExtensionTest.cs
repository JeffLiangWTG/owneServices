using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class TraderExtensionTest : TestCase
	{
		public void TestGetWrappedAddress()
		{
			AssertEquals("GetWrappedAddress() when ITrader is null", ZString.Empty, TraderExtension.GetWrappedAddress(null));

			var traderMock = new Mock<ITrader>();
			traderMock.Setup(x => x.CompanyName).Returns("Name");
			traderMock.Setup(x => x.StreetAndNumber).Returns("Street and Number");
			traderMock.Setup(x => x.PostalCode).Returns("Postal Code");
			traderMock.Setup(x => x.City).Returns("City");
			traderMock.Setup(x => x.CountryCode).Returns("XX");

			var trader = traderMock.Object;
			AssertEquals("GetWrappedAddress()", "Name\nStreet and Number\nPostal Code City\nXX", trader.GetWrappedAddress());
		}

		public void TestGetWrappedAddressSummary()
		{
			AssertEquals("GetWrappedAddressSummary() when ITrader is null", ZString.Empty, TraderExtension.GetWrappedAddressSummary(null));

			var traderMock = new Mock<ITrader>();
			traderMock.Setup(x => x.Name).Returns("Name");
			traderMock.Setup(x => x.StreetAndNumber).Returns("Street and Number");
			traderMock.Setup(x => x.PostalCode).Returns("Postal Code");
			traderMock.Setup(x => x.City).Returns("City");
			traderMock.Setup(x => x.CountryCode).Returns("XX");

			var trader = traderMock.Object;
			AssertEquals("GetWrappedAddressSummary()", "Name, Street and Number, Postal Code City, XX", trader.GetWrappedAddressSummary());
			AssertEquals("GetWrappedAddressSummary(streetAndNumber)", "Name, Address1 Address2, Postal Code City, XX", trader.GetWrappedAddressSummary("Address1 Address2"));
		}
	}
}
