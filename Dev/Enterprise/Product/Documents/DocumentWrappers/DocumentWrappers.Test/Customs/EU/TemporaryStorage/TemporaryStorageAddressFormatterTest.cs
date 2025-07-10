using System;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

sealed class TemporaryStorageAddressFormatterTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStorageAddressFormatter(addressDetails: null));
	}

	public void TestFormatAddressAsString()
	{
		const string expectedFormattedAddress =
			"Company Name\r\n" +
			"Company Address1\r\n" +
			"Company Address2\r\n" +
			"12345\r\n" +
			"Company City\r\n" +
			"IT";

		var addressDetailsMock = new Mock<IAddressDetails>();
		addressDetailsMock.Setup(x => x.CompanyName).Returns("Company Name");
		addressDetailsMock.Setup(x => x.AddressLine1).Returns("Company Address1");
		addressDetailsMock.Setup(x => x.AddressLine2).Returns("Company Address2");
		addressDetailsMock.Setup(x => x.PostCode).Returns("12345");
		addressDetailsMock.Setup(x => x.City).Returns("Company City");
		addressDetailsMock.Setup(x => x.Country).Returns("IT");

		var formatter = new TemporaryStorageAddressFormatter(addressDetailsMock.Object);
		AssertEquals("Formatted Address", expectedFormattedAddress, formatter.AsString());
	}
}
