using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class JobDeclarationInlandTransportResDataHelperTest : TestCaseWithFactory
{
	public void TestGetInlandTransactionIDCaption_SEA()
	{
		AssertCaptions(TransportTypeList.Codes.Sea, TransportMeansList.Codes.ImoShipIdentificationNumber, "Lloyds No.", "[19 05 017 000] Lloyds Number");
		AssertCaptions(TransportTypeList.Codes.Sea, TransportMeansList.Codes.NameOfTheSeaGoingVessel, "Vessel Name", "[19 05 017 000] Vessel Name");
	}

	public void TestGetInlandTransactionIDCaption_RAI()
	{
		AssertCaptions(TransportTypeList.Codes.Rail, TransportMeansList.Codes.WagonNumber, "Wagon No.", "[19 05 017 000] Wagon Number");
		AssertCaptions(TransportTypeList.Codes.Rail, TransportMeansList.Codes.TrainNumber, "Train No.", "[19 05 017 000] Train Number");
	}

	public void TestGetInlandTransactionIDCaption_IWT()
	{
		AssertCaptions(TransportTypeList.Codes.InlandWaterwayTransport, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, "Vessel Name", "[19 05 017 000] Vessel Name");
		AssertCaptions(TransportTypeList.Codes.InlandWaterwayTransport, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, "ENI Code", "[19 05 017 000] European Vessel Identification Number");
	}

	public void TestGetInlandTransactionIDCaption_AIR()
	{
		AssertCaptions(TransportTypeList.Codes.Air, TransportMeansList.Codes.IataFlightNumber, "Flight No.", "[19 05 017 000] Flight No.");
		AssertCaptions(TransportTypeList.Codes.Air, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, "Registration No.", "[19 05 017 000] Aircraft Registration Number");
	}

	public void TestGetInlandTransactionIDCaption_ROA()
	{
		AssertCaptions(TransportTypeList.Codes.Road, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, "Registration No.", "[19 05 017 000] Vehicle Registration Number");
		AssertCaptions(TransportTypeList.Codes.Road, TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, "Registration No.", "[19 05 017 000] Vehicle Registration Number");
	}

	public void TestGetInlandTransactionIDCaption_OWN()
	{
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.ImoShipIdentificationNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.NameOfTheSeaGoingVessel, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.WagonNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.TrainNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.IataFlightNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.OwnPropulsion, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, "Transport ID", "[19 06 017 000] Identification number of the transport");
	}

	public void TestGetInlandTransactionIDCaption_MAI()
	{
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.ImoShipIdentificationNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.NameOfTheSeaGoingVessel, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.WagonNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.TrainNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.IataFlightNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.Mail, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, "Transport ID", "[19 06 017 000] Identification number of the transport");
	}

	public void TestGetInlandTransactionIDCaption_FIX()
	{
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.ImoShipIdentificationNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.NameOfTheSeaGoingVessel, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.WagonNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.TrainNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.IataFlightNumber, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, "Transport ID", "[19 06 017 000] Identification number of the transport");
		AssertCaptions(TransportTypeList.Codes.FixedTransportInstallations, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, "Transport ID", "[19 06 017 000] Identification number of the transport");
	}

	public void AssertCaptions(string inlandTransportMode, string inlandTransportType, string expectedCaption, string expectedDescription)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_TransportModeInland = inlandTransportMode;
		declaration.JE_TransportMeans = inlandTransportType;
		var resData = JobDeclarationInlandTransportResDataHelper.GetInlandTransactionIDCaption(declaration);

		CombineAssertions($"Inland Transport mode {inlandTransportMode}, Type of ID {inlandTransportType}", () =>
		{
			AssertEquals("Caption", expectedCaption, resData.Caption);
			AssertEquals("Full Description", expectedDescription, resData.FullDescription);
		});
	}
}
