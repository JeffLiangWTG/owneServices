using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using ExportBorderTransportMeansList = Enterprise.Customs.EU.Business.ExportBorderTransportMeansList;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class ActiveTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveTransportMeansProvider>
	{
		public void TestTypeOfIdentification()
		{
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			AssertEquals("IdentificationType", ExportBorderTransportMeansList.Codes._10, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._10, lloydsIMO, nameof(JobDeclaration.JE_LloydsIMO));
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._11, vesselName, nameof(JobDeclaration.JE_VesselName));
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._30, vesselName, nameof(JobDeclaration.JE_VesselName));
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._40, voyageFlightNo, nameof(JobDeclaration.JE_VoyageFlightNo));
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._41, aircraftRegistration, nameof(JobDeclaration.JE_AircraftRegistration));
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._80, vesselName, nameof(JobDeclaration.JE_VesselName));
			AssertIdentificationNumber(ExportBorderTransportMeansList.Codes._81, vesselName, nameof(JobDeclaration.JE_VesselName));
		}

		void AssertIdentificationNumber(ZString code, string expectedNumber, string nameOfField = "")
		{
			var codeList = Factory.GetCachedValue<ExportBorderTransportMeansList>();
			CombineAssertions(code, () =>
			{
				declaration.ZG_BorderTransportMeans = code;
				declaration.JE_VesselName = vesselName;
				declaration.JE_LloydsIMO = lloydsIMO;
				declaration.JE_IATALoadPort = iATALoadPort;
				declaration.JE_VoyageFlightNo = voyageFlightNo;
				declaration.JE_AircraftRegistration = aircraftRegistration;
				var provider = GetProvider();
				AssertEquals(
					$"{nameof(JobDeclaration.ZG_BorderTransportMeans)}: {codeList.GetDescriptionFromCode(code)}, expecting {nameOfField} for {nameof(ActiveTransportMeansProvider)}.{nameof(ActiveTransportMeansProvider.IdentificationNumber)}",
					expectedNumber,
					provider.IdentificationNumber
				);
			});
		}

		readonly string vesselName = "BOB'S VESSEL";
		readonly string lloydsIMO = "1234567";
		readonly string iATALoadPort = "SYD";
		readonly string voyageFlightNo = "QF123";
		readonly string aircraftRegistration = "AR951";

		public void TestNationality()
		{
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Australia;
			AssertEquals("AESActiveTransportMeansProvider Nationality", Core.Constants.CountryCodes.Australia, Provider.Nationality);
		}

		protected override ActiveTransportMeansProvider GetProvider() => new ActiveTransportMeansProvider(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
