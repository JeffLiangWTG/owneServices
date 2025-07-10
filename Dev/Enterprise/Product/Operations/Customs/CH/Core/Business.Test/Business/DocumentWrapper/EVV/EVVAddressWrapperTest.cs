using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

class EVVAddressWrapperTest : TestCase
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>("Null argument", () => EVVAddressWrapper.New(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		var evvAddressMock = new Mock<IEvvAddress>();
		evvAddressMock.Setup(m => m.Name).Returns("name");
		evvAddressMock.Setup(m => m.Street).Returns("street");
		evvAddressMock.Setup(m => m.Country).Returns("ct");
		evvAddressMock.Setup(m => m.PostalCode).Returns("post");
		evvAddressMock.Setup(m => m.City).Returns("city");

		var wrapper = EVVAddressWrapper.New(evvAddressMock.Object);

		AssertEquals("Name", "name", wrapper.Name);
		AssertEquals("Street", "street", wrapper.Street);
		AssertEquals("Country", "ct", wrapper.Country);
		AssertEquals("PostalCode", "post", wrapper.PostalCode);
		AssertEquals("City", "city", wrapper.City);
	});
}
