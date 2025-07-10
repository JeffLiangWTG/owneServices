using Enterprise.Customs.BE.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BE.Business.Testing;

class TransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportMeansProvider>
{
	public void TestTypeOfIdentification()
	{
		declaration.ZG_BorderTransportMeans = "10";
		AssertEquals("Type of Identification", 10, provider.TypeOfIdentification);
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			declaration.JE_VesselName = "MSC Cargowise";
			declaration.JE_VoyageFlightNo = "Flight123";
			declaration.ZG_BorderTransportMeans = "10";
			provider = new TransportMeansProvider(declaration);
			AssertNull("Identification Number", provider.IdentificationNumber);

			declaration.ZG_BorderTransportMeans = "11";
			provider = new TransportMeansProvider(declaration);
			AssertEquals("Identification Number", "MSC Cargowise", provider.IdentificationNumber);

			declaration.ZG_BorderTransportMeans = "41";
			provider = new TransportMeansProvider(declaration);
			AssertEquals("Identification Number", "Flight123", provider.IdentificationNumber);
		});
	}

	public void TestNationality()
	{
		declaration.JE_RN_NKTransportNationality = CountryCodes.Belgium;
		AssertEquals("Nationality", "BE", provider.Nationality);
	}

	protected override TransportMeansProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		provider = new TransportMeansProvider(declaration);
	}
	JobDeclaration declaration;
	TransportMeansProvider provider;
}
