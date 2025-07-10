using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class ConsignmentCountryRoutingWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new ConsignmentCountryRoutingWrapper(null));
	}

	public void TestSequenceNumber()
	{
		var wrapper = GetWrapper();
		AssertEquals(nameof(IConsignmentCountryRouting.SequenceNumber), 1, wrapper.SequenceNumber);
	}

	public void TestCountryCode()
	{
		var wrapper = GetWrapper();
		AssertNullOrEmpty(nameof(IConsignmentCountryRouting.CountryCode), wrapper.CountryCode);

		countryOfRouting.CY_Data = "IT";
		wrapper = GetWrapper();
		AssertEquals(nameof(IConsignmentCountryRouting.CountryCode), "IT", wrapper.CountryCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.NewDepartureNctsHeader();
		countryOfRouting = header.CountriesOfRouting.AddNew();
	}

	IConsignmentCountryRouting GetWrapper() => new ConsignmentCountryRoutingWrapper(countryOfRouting);

	CountryOfRouting countryOfRouting;
}
