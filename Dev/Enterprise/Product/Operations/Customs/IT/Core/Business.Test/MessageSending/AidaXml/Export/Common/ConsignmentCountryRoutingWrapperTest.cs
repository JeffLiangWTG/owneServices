using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class ConsignmentCountryRoutingWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentCountryRoutingWrapper(null));
	}

	public void TestSequenceNumber()
	{
		var consignmentRoutingWrapper = GetNewWrapper();
		AssertEquals(nameof(IConsignmentCountryRouting.SequenceNumber), 1, consignmentRoutingWrapper.SequenceNumber);
	}

	public void TestCountry()
	{
		var consignmentRoutingWrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(IConsignmentCountryRouting.CountryCode), consignmentRoutingWrapper.CountryCode);

		itineraryCountry.CY_Code = "IT";
		consignmentRoutingWrapper = GetNewWrapper();
		AssertEquals(nameof(IConsignmentCountryRouting.CountryCode), "IT", consignmentRoutingWrapper.CountryCode);
	}

	IConsignmentCountryRouting GetNewWrapper() => new ConsignmentCountryRoutingWrapper(itineraryCountry);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		itineraryCountry = declaration.ItineraryCountries.AddNew();
	}
	ItineraryCountry itineraryCountry;
}
