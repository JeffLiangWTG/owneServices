using CargoWise.Customs.IT.MessageContracts;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class AddressEqualityComparerTest : TestCase
{
	public void TestEquality()
	{
		var addressMock1 = GetNewAddressMock();
		var addressMock2 = GetNewAddressMock();
		var equalityComparer = new AddressEqualityComparer();
		CombineAssertions(() =>
		{
			AssertEquals("Same street, city, country, name and zip code", true, equalityComparer.Equals(addressMock1.Object, addressMock2.Object));

			addressMock2.Setup(m => m.StreetAndNumber).Returns("StreetAndNumber2");
			AssertEquals("different StreetAndNumber", false, equalityComparer.Equals(addressMock1.Object, addressMock2.Object));

			addressMock2.Setup(m => m.StreetAndNumber).Returns("StreetAndNumber");
			addressMock2.Setup(m => m.City).Returns("City2");
			AssertEquals("Different City", false, equalityComparer.Equals(addressMock1.Object, addressMock2.Object));

			addressMock2.Setup(m => m.City).Returns("City");
			addressMock2.Setup(m => m.Country).Returns("Country2");
			AssertEquals("Different Country", false, equalityComparer.Equals(addressMock1.Object, addressMock2.Object));

			addressMock2.Setup(m => m.Country).Returns("Country");
			addressMock2.Setup(m => m.Name).Returns("Name2");
			AssertEquals("Different Name", false, equalityComparer.Equals(addressMock1.Object, null));

			addressMock2.Setup(m => m.Name).Returns("Name");
			addressMock2.Setup(m => m.ZipCode).Returns("ZipCode2");
			AssertEquals("Different ZipCode", false, equalityComparer.Equals(addressMock1.Object, null));

			AssertEquals("One object is null", false, equalityComparer.Equals(addressMock1.Object, null));

			AssertEquals("Both objects are null", true, equalityComparer.Equals(null, null));
		});
	}

	Mock<IAddress> GetNewAddressMock()
	{
		var addressMock = new Mock<IAddress>();
		addressMock.Setup(x => x.StreetAndNumber).Returns("StreetAndNumber");
		addressMock.Setup(x => x.City).Returns("City");
		addressMock.Setup(x => x.Country).Returns("Country");
		addressMock.Setup(x => x.Name).Returns("Name");
		addressMock.Setup(x => x.ZipCode).Returns("ZipCode");
		return addressMock;
	}
}
