using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class BorderMeansOfTransportWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull_DeclarationCannotBeNull()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => BorderMeansOfTransportWrapper.NewOrNull(declaration: null));
	}

	public void TestNewOrNull_ReturnsNullIfAllInvolvedFieldsAreEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.ZG_BorderTransportMeans = "";
		declaration.JE_VesselName = "";
		declaration.JE_VoyageFlightNo = "";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNull("When all involved fields are empty, BorderMeansOfTransportWrapper", BorderMeansOfTransportWrapper.NewOrNull(declaration));
	}

	public void TestTypeOfIdentification()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.ZG_BorderTransportMeans = "10";
		var wrapper = BorderMeansOfTransportWrapper.NewOrNull(declaration);
		AssertEquals("TypeOfIdentification", 10, wrapper.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_ReturnsNegativeValueIfBorderTransportMeansIsInvalid()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.ZG_BorderTransportMeans = "XX";
		var wrapper = BorderMeansOfTransportWrapper.NewOrNull(declaration);
		AssertEquals("TypeOfIdentification", -1, wrapper.TypeOfIdentification);
	}

	public void TestIdentificationNumber_IfTransportModeIsAir()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = "AIR";

		declaration.JE_VoyageFlightNo = "FLIGHTNO";
		var wrapper = BorderMeansOfTransportWrapper.NewOrNull(declaration);
		AssertEquals("IdentificationNumber", "FLIGHTNO", wrapper.IdentificationNumber);
	}

	public void TestIdentificationNumber_IfTransportModeIsNotAir()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = "XYZ";

		declaration.JE_VesselName = "VESSEL NAME";
		var wrapper = BorderMeansOfTransportWrapper.NewOrNull(declaration);
		AssertEquals("IdentificationNumber", "VESSEL NAME", wrapper.IdentificationNumber);
	}

	public void TestNationality()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RN_NKTransportNationality = "IT";

		var wrapper = BorderMeansOfTransportWrapper.NewOrNull(declaration);
		AssertEquals("Nationality", "IT", wrapper.Nationality);
	}
}
