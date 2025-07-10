using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITBox18IdentityOfTransportAtDepartureBuilderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("when declaration is null", () => new ITBox18IdentityOfTransportAtDepartureBuilder(null));

		AssertNoExceptionThrown("when declaration is valid", () => new ITBox18IdentityOfTransportAtDepartureBuilder(Factory.New<JobDeclaration>()));
	}

	public void TestGetBox18IdentityOfTransportAtDepartureFormattedForIMPUCC6_COM_EXPNonUCC6Declarations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var builder = new ITBox18IdentityOfTransportAtDepartureBuilder(declaration);

		CombineAssertions("Box18 value", () =>
		{
			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"IMP",
				"AB 123CD",
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"COM",
				"AB 123CD",
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"EXP",
				"AB 123CD",
				ZString.Empty);
		});
	}

	public void TestGetBox18IdentityOfTransportAtDepartureFormattedForEXPUCC6Declarations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var builder = new ITBox18IdentityOfTransportAtDepartureBuilder(declaration);

		CombineAssertions("Box18 value for EXP", () =>
		{
			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"AIR",
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"AIR",
				"XYZ-10001000101",
				ZString.Empty,
				ZString.Empty,
				"XYZ-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"RAI",
				"RAI-10001000101",
				ZString.Empty,
				ZString.Empty,
				"RAI-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"FIX",
				"XYZ-10001000101",
				ZString.Empty,
				ZString.Empty,
				"XYZ-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"AIR",
				"XYZ-10001000101",
				"KA29",
				ZString.Empty,
				"KA29");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"AIR",
				ZString.Empty,
				"KA29",
				ZString.Empty,
				"KA29");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"RAI",
				"RAI-10001000101",
				"KA29",
				"RAI29",
				"RAI-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				builder.GetBox18IdentityOfTransportAtDepartureFormatted,
				"RAI",
				ZString.Empty,
				"KA29",
				"RAI29",
				"RAI29");
		});
	}

	void AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
		JobDeclaration declaration,
		Func<ZString> getBox18IdentityOfTransportAtDepartureFormatted,
		string messageType,
		string box18TransportID,
		ZString expectedValueWhenNoID)
	{
		declaration.JE_MessageType = messageType;
		declaration.ZG_Box18TransportID = box18TransportID;

		AssertEquals(
			$"For {declaration.JE_MessageType} when Box18TransportID available",
			box18TransportID,
			getBox18IdentityOfTransportAtDepartureFormatted());

		declaration.ZG_Box18TransportID = "";
		AssertEquals(
			$"For {declaration.JE_MessageType} when Box18TransportID not available",
			expectedValueWhenNoID,
			getBox18IdentityOfTransportAtDepartureFormatted());
	}

	void AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
		JobDeclaration declaration,
		Func<ZString> getBox18IdentityOfTransportAtDepartureFormatted,
		ZString transportModeInland,
		ZString transportIDInland,
		ZString aircraftRegistrationInland,
		ZString trailer1RegNo,
		ZString expectedValue)
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		declaration.ZG_Box18TransportID = "AB 123CD";
		declaration.JE_TransportModeInland = transportModeInland;
		declaration.JE_TransportIDInland = transportIDInland;
		declaration.JE_AircraftRegistrationInland = aircraftRegistrationInland;
		declaration.JE_Trailer1RegNo = trailer1RegNo;

		AssertEquals(
			$@"When JE_TransportModeInland is {transportModeInland},JE_TransportIDInland is {transportIDInland},
				JE_AircraftRegistrationInland is {aircraftRegistrationInland} and JE_Trailer1RegNo is {trailer1RegNo}",
			expectedValue, getBox18IdentityOfTransportAtDepartureFormatted());
	}
}
