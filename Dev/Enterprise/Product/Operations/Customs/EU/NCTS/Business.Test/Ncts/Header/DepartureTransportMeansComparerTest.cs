using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Moq;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class DepartureTransportMeansComparerTest : TestCaseWithFactory
{
	public void TestTransportMeansComparisons()
	{
		var comparer = new DepartureTransportMeansComparer();

		TestCases.ForEach(testCase =>
		{
			var result = comparer.Equals(testCase.Provider1?.Object, testCase.Provider2?.Object);
			AssertEquals(testCase.Description, testCase.ExpectedEqual, result);
		});
	}

	static Mock<IDepartureTransportMeansProvider> CreateMockProvider(
		string inlandTransportMode, string vesselName, string vesselCountry,
		string transportType, string transport, string transportCountry,
		string trailer1ID, string trailer1Nationality, string trailer2ID,
		string trailer2Nationality, string aircraftID,
		List<(string WagonNumber, string WagonNationality)> additionalWagonsInfo)
	{
		var mock = new Mock<IDepartureTransportMeansProvider>();

		mock.SetupGet(m => m.InlandTransportModeAtDeparture).Returns(inlandTransportMode);
		mock.SetupGet(m => m.VesselNameAtDeparture).Returns(vesselName);
		mock.SetupGet(m => m.VesselCountryAtDeparture).Returns(vesselCountry);
		mock.SetupGet(m => m.TransportTypeAtDeparture).Returns(transportType);
		mock.SetupGet(m => m.TransportAtDeparture).Returns(transport);
		mock.SetupGet(m => m.TransportCountryAtDeparture).Returns(transportCountry);
		mock.SetupGet(m => m.Trailer1IDAtDeparture).Returns(trailer1ID);
		mock.SetupGet(m => m.Trailer1NationalityAtDeparture).Returns(trailer1Nationality);
		mock.SetupGet(m => m.Trailer2IDAtDeparture).Returns(trailer2ID);
		mock.SetupGet(m => m.Trailer2NationalityAtDeparture).Returns(trailer2Nationality);
		mock.SetupGet(m => m.AircraftIDAtDeparture).Returns(aircraftID);

		if (additionalWagonsInfo is null)
		{
			mock.SetupGet(m => m.AdditionalWagons).Returns((IBusinessObjectCollection<IAdditionalWagonProvider>)null);
			return mock;
		}

		var additionalWagons = new List<IAdditionalWagonProvider>();
		foreach (var (wagonNumber, wagonNationality) in additionalWagonsInfo)
		{
			var wagonMock = new Mock<IAdditionalWagonProvider>();
			wagonMock.SetupGet(w => w.WagonNumber).Returns(wagonNumber);
			wagonMock.SetupGet(w => w.WagonNationality).Returns(wagonNationality);
			additionalWagons.Add(wagonMock.Object);
		}

		var additionalWagonsMock = new Mock<IBusinessObjectCollection<IAdditionalWagonProvider>>();
		additionalWagonsMock.Setup(m => m.GetEnumerator()).Returns(() => additionalWagons.GetEnumerator());
		additionalWagonsMock.Setup(m => m.Count).Returns(additionalWagons.Count);

		for (int i = 0; i < additionalWagons.Count; i++)
		{
			var index = i;
			additionalWagonsMock.Setup(m => m[index]).Returns(additionalWagons[index]);
		}

		mock.SetupGet(m => m.AdditionalWagons).Returns(additionalWagonsMock.Object);

		return mock;
	}

	List<TransportMeansTestCase> TestCases => [
		new("When all parameters are same", true,
			CreateMockProvider("3", "VesselA", "DE", "10", "123", "IT", "54321", "US", "98765", "FR", "A1B2C3",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("3", "VesselA", "DE", "10", "123", "IT", "54321", "US", "98765", "FR", "A1B2C3",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When InlandTransportModes are different", false,
			CreateMockProvider("3", "VesselA", "DE", "10", "123", "IT", "54321", "US", "98765", "FR", "A1B2C3",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("4", "VesselA", "DE", "10", "123", "IT", "54321", "US", "98765", "FR", "A1B2C3",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When TransportDetails are different", false,
			CreateMockProvider("2", "VesselB", "FR", "21", "4321", "IT", "87654", "GB", "98765", "ES", "B2C3D4",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("2", "VesselB", "FR", "21", "2314", "US", "87654", "GB", "98765", "ES", "B2C3D4",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When VesselNames are different", false,
			CreateMockProvider("3", "VesselX", "DE", "10", "123", "IT", "54321", "US", "98765", "FR", "A1B2C3",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("3", "VesselY", "DE", "10", "123", "IT", "54321", "US", "98765", "FR", "A1B2C3",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When TrailerNationalities are different", false,
			CreateMockProvider("2", "VesselD", "BE", "15", "999", "SE", "66666", "NO", "77777", "DK", "Q1W2E3",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("2", "VesselD", "BE", "15", "999", "SE", "66666", "FI", "77777", "DK", "Q1W2E3",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When TrailerIDs are different", false,
			CreateMockProvider("3", "VesselF", "PL", "25", "654", "CZ", "11111", "HU", "22222", "AT", "G5H6J7",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("3", "VesselF", "PL", "25", "654", "CZ", "99999", "HU", "22222", "AT", "G5H6J7",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When TransportTypes are different", false,
			CreateMockProvider("3", "VesselG", "SE", "35", "222", "NO", "33333", "FI", "44444", "DK", "M1N2B3",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("3", "VesselG", "SE", "40", "222", "NO", "33333", "FI", "44444", "DK", "M1N2B3",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When NumberOfAdditionalWagons are different", false,
			CreateMockProvider("3", "VesselG", "SE", "35", "222", "NO", "33333", "FI", "44444", "DK", "M1N2B3",
				[("WAG001", "DE"), ("WAG002", "FR")]),
			CreateMockProvider("3", "VesselG", "SE", "40", "222", "NO", "33333", "FI", "44444", "DK", "M1N2B3",
				[("WAG005", "US"), ("WAG002", "FR")])
		),

		new("When AdditionalWagon list lengths are different", false,
			CreateMockProvider("3", "VesselG", "SE", "35", "222", "NO", "33333", "FI", "44444", "DK", "M1N2B3",
				[("WAG001", "DE")]),
			CreateMockProvider("3", "VesselG", "SE", "35", "222", "NO", "33333", "FI", "44444", "DK", "M1N2B3",
				[("WAG001", "DE"), ("WAG002", "FR")])
		),

		new("When one AdditionalWagons list is null and the other has data", false,
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3", null),
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3",
				[("WAG001", "NL"), ("WAG002", "BE")])
		),

		new("When both AdditionalWagons list are null", true,
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3", null),
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3", null)
		),

		new("When data inside one TransportMeans is null and the other has data", false,
			CreateMockProvider(null, null, null, null, null, null, null, null, null, null, null, null),
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3",
				[("WAG001", "NL"), ("WAG002", "BE")])
		),

		new("When data inside second TransportMeans is null and the other has data", false,
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3",
				[("WAG001", "NL"), ("WAG002", "BE")]),
			CreateMockProvider(null, null, null, null, null, null, null, null, null, null, null, null)
		),

		new("When data inside both TransportMeans is null", true,
			CreateMockProvider(null, null, null, null, null, null, null, null, null, null, null, null),
			CreateMockProvider(null, null, null, null, null, null, null, null, null, null, null, null)
		),

		new("When first TransportMeans is null and the other has data", false,
			null,
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3",
				[("WAG001", "NL"), ("WAG002", "BE")])
		),

		new("When second TransportMeans is null and the first has data", false,
			CreateMockProvider("3", "VesselZ", "NL", "10", "1234", "BE", "11111", "DE", "22222", "FR", "A1B2C3",
				[("WAG001", "NL"), ("WAG002", "BE")]),
			null
		),

		new("When both TransportMeans are null", true, null, null)
	];
}

sealed class TransportMeansTestCase(
	string description,
	bool expectedEqual,
	Mock<IDepartureTransportMeansProvider> provider1,
	Mock<IDepartureTransportMeansProvider> provider2)
{
	public string Description { get; } = description;
	public bool ExpectedEqual { get; } = expectedEqual;
	public Mock<IDepartureTransportMeansProvider> Provider1 { get; } = provider1;
	public Mock<IDepartureTransportMeansProvider> Provider2 { get; } = provider2;
}
