using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Germany)]
	sealed class DEDocSADHTest : DocSADHTest
	{
		public void TestBox17bImporterState_Import()
		{
			var jobDeclaration = GetJobDeclaration();
			jobDeclaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var wrapper = DEDocSADH.New(jobDeclaration.CustomsEntryHeaders[0], Factory);
			AssertEquals("Province of Destination", "BA", wrapper.Box17ImporterState);
		}

		public void TestBox17bImporterState_Export()
		{
			var jobDeclaration = GetJobDeclaration();
			jobDeclaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var wrapper = DEDocSADH.New(jobDeclaration.CustomsEntryHeaders[0], Factory);
			AssertEquals("Province of Destination", "", wrapper.Box17ImporterState);
		}

		public void TestBox18NationalityOfTransportAtDeparture_ExportUcc6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_RN_NKTransportNationalityInland = "IT";
			declaration.JE_RN_NKTrailer1Nationality = "IN";
			declaration.ZG_Box18TransportNationality = "DE";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DEDocSADH.New(entryHeader, Factory);
			using (TemporarilySetUCC6Configuration(declaration, true))
			{
				CombineAssertions("DE should always return ZG_Box18TransportNationality for UCC6", () =>
				{
					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
					declaration.JE_TransportIDInland = "123";
					AssertEquals("AIR, Transport ID not empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportIDInland = string.Empty;
					declaration.JE_AircraftRegistrationInland = "123";
					AssertEquals("AIR, Transport ID empty but aircraft registration not empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_AircraftRegistrationInland = string.Empty;
					AssertEquals("AIR, Transport ID and aircraft registration empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
					declaration.JE_TransportIDInland = "123";
					AssertEquals("Rail, Transport ID not empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportIDInland = string.Empty;
					declaration.JE_Trailer1RegNo = "123";
					AssertEquals("Rail, Transport ID empty but trailer registration number not empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_Trailer1RegNo = string.Empty;
					AssertEquals("Rail, Transport ID and trailer registration number empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
					declaration.JE_TransportIDInland = "123";
					AssertEquals("Sea, Transport ID not empty", "DE", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportIDInland = "123";
					AssertEquals("Sea, Transport ID empty", "DE", wrapper.Box18TransportNationalityAtDeparture);
				});
			}
		}

		protected override ZString CountrySpecificCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		protected override ZString ExpectedEadBarCode => ZString.Empty;

		protected override ZDateTime ExpectedDOE => new ZDateTime(2008, 7, 1);

		JobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "ITBRI";
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}
	}
}
