using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class DepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DepartureTransportMeansProvider(null, ZString.Empty, ZString.Empty, 0));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestTypeOfIdentification() => CombineAssertions(() =>
	{
		AssertEquals(11, Provider.TypeOfIdentification);

		declaration.JE_TransportMeans = string.Empty;
		var provider = new DepartureTransportMeansProvider(declaration, declaration.JE_TransportIDInland, declaration.JE_RN_NKTransportNationalityInland, 1);
		AssertNull(provider.TypeOfIdentification);

		declaration.JE_TransportMeans = "A";
		provider = new DepartureTransportMeansProvider(declaration, declaration.JE_TransportIDInland, declaration.JE_RN_NKTransportNationalityInland, 1);
		AssertNull(provider.TypeOfIdentification);
	});

	public void TestIdentificationNumber()
	{
		AssertEquals("MSC ANITA", Provider.IdentificationNumber);
	}

	public void TestNationality()
	{
		AssertEquals("BE", Provider.Nationality);
	}

	protected override DepartureTransportMeansProvider GetProvider()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMeans = "11";
		declaration.JE_TransportModeInland = "SEA";
		declaration.JE_TransportIDInland = "MSC ANITA";
		declaration.JE_RN_NKTransportNationalityInland = "BE";

		return new DepartureTransportMeansProvider(declaration, declaration.JE_TransportIDInland, declaration.JE_RN_NKTransportNationalityInland, 1);
	}
	JobDeclaration declaration;
}
