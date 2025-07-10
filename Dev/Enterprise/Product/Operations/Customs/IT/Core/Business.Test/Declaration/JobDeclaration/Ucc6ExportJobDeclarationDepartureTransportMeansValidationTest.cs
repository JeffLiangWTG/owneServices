using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobDeclarationDepartureTransportMeansValidationTest : TestCaseWithFactory
{
	public void TestCheckJE_TransportIDInlandWithTrailer1RegNo_AESTP_InlandMOT_B1884()
	{
		var expectedMessage = "[B1884] During the transitory period, which is active now, only one means of transport must be filled in";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportModeInland = "RAI";
				declaration.JE_Trailer1RegNo = "XYZ";
				declaration.JE_TransportIDInland = "ABC";
				AssertHasMessageErrorContaining("When JE_TransportModeInland = RAI, JE_TransportIDInland and JE_Trailer1RegNo are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_Trailer1RegNo = ZString.Empty;
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = RAI, JE_TransportIDInland filled and JE_Trailer1RegNo empty", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_Trailer1RegNo = "XYZ";
				declaration.JE_TransportIDInland = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportModeInland = RAI, JE_TransportIDInland empty and JE_Trailer1RegNo filled", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_TransportModeInland = "AAA";
				declaration.JE_TransportIDInland = "ABC";
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AAA, JE_TransportIDInland and JE_Trailer1RegNo are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportModeInland = "RAI";
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = RAI, JE_TransportIDInland and JE_Trailer1RegNo are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_Trailer1RegNoWithTransportIDInland_AESTP_InlandMOT_B1884()
	{
		var expectedMessage = "[B1884] During the transitory period, which is active now, only one means of transport must be filled in";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportModeInland = "RAI";
				declaration.JE_TransportIDInland = "XYZ";
				declaration.JE_Trailer1RegNo = "ABC";
				AssertHasMessageErrorContaining("When JE_TransportModeInland = RAI, JE_Trailer1RegNo and JE_TransportIDInland are filled", declaration.JE_Trailer1RegNoInfo, expectedMessage);

				declaration.JE_TransportIDInland = ZString.Empty;
				declaration.Validation.ValidateJE_Trailer1RegNo();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = RAI, JE_Trailer1RegNo filled and JE_TransportIDInland empty", declaration.JE_Trailer1RegNoInfo, expectedMessage);

				declaration.JE_TransportIDInland = "XYZ";
				declaration.JE_Trailer1RegNo = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportModeInland = RAI, JE_Trailer1RegNo empty and JE_TransportIDInland filled", declaration.JE_Trailer1RegNoInfo, expectedMessage);

				declaration.JE_TransportModeInland = "AAA";
				declaration.JE_Trailer1RegNo = "ABC";
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AAA, JE_Trailer1RegNo and JE_TransportIDInland are filled", declaration.JE_Trailer1RegNoInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportModeInland = "RAI";
				declaration.Validation.ValidateJE_Trailer1RegNo();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = RAI, JE_Trailer1RegNo and JE_TransportIDInland are filled", declaration.JE_Trailer1RegNoInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_TransportIDInlandWithAircraftRegistrationInland_AESTP_InlandMOT_B1884()
	{
		var expectedMessage = "[B1884] During the transitory period, which is active now, only one means of transport must be filled in";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.JE_AircraftRegistrationInland = "XYZ";
				declaration.JE_TransportIDInland = "ABC";
				AssertHasMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland and JE_AircraftRegistrationInland are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_AircraftRegistrationInland = ZString.Empty;
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland filled and JE_AircraftRegistrationInland empty", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_AircraftRegistrationInland = "XYZ";
				declaration.JE_TransportIDInland = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland empty and JE_AircraftRegistrationInland filled", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_TransportModeInland = "AAA";
				declaration.JE_TransportIDInland = "ABC";
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AAA, JE_TransportIDInland and JE_AircraftRegistrationInland are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland and JE_AircraftRegistrationInland are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_AircraftRegistrationInlandWithTransportIDInland_AESTP_InlandMOT_B1884()
	{
		var expectedMessage = "[B1884] During the transitory period, which is active now, only one means of transport must be filled in";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.JE_TransportIDInland = "XYZ";
				declaration.JE_AircraftRegistrationInland = "ABC";
				AssertHasMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland and JE_TransportIDInland are filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);

				declaration.JE_TransportIDInland = ZString.Empty;
				declaration.Validation.ValidateJE_AircraftRegistrationInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland filled and JE_TransportIDInland empty", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);

				declaration.JE_TransportIDInland = "XYZ";
				declaration.JE_AircraftRegistrationInland = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland empty and JE_TransportIDInland filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);

				declaration.JE_TransportModeInland = "AAA";
				declaration.JE_AircraftRegistrationInland = "ABC";
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AAA, JE_AircraftRegistrationInland and JE_TransportIDInland are filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.Validation.ValidateJE_AircraftRegistrationInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland and JE_TransportIDInland are filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_TransportIDInlandWithAircraftRegistrationInland_AESTP_InlandMOT_R0855()
	{
		var expectedMessage = "[R0855] Only one means of transport must be filled in";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.JE_AircraftRegistrationInland = "XYZ";
				declaration.JE_TransportIDInland = "ABC";
				AssertHasMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland and JE_AircraftRegistrationInland are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_AircraftRegistrationInland = ZString.Empty;
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland filled and JE_AircraftRegistrationInland empty", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_AircraftRegistrationInland = "XYZ";
				declaration.JE_TransportIDInland = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland empty and JE_AircraftRegistrationInland filled", declaration.JE_TransportIDInlandInfo, expectedMessage);

				declaration.JE_TransportModeInland = "AAA";
				declaration.JE_TransportIDInland = "ABC";
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AAA, JE_TransportIDInland and JE_AircraftRegistrationInland are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_TransportIDInland and JE_AircraftRegistrationInland are filled", declaration.JE_TransportIDInlandInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_AircraftRegistrationInlandWithTransportIDInland_AESTP_InlandMOT_R0855()
	{
		var expectedMessage = "[R0855] Only one means of transport must be filled in";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.JE_TransportIDInland = "XYZ";
				declaration.JE_AircraftRegistrationInland = "ABC";
				AssertHasMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland and JE_TransportIDInland are filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);

				declaration.JE_TransportIDInland = ZString.Empty;
				declaration.Validation.ValidateJE_AircraftRegistrationInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland filled and JE_TransportIDInland empty", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);

				declaration.JE_TransportIDInland = "XYZ";
				declaration.JE_AircraftRegistrationInland = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland empty and JE_TransportIDInland filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);

				declaration.JE_TransportModeInland = "AAA";
				declaration.JE_AircraftRegistrationInland = "ABC";
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AAA, JE_AircraftRegistrationInland and JE_TransportIDInland are filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportModeInland = "AIR";
				declaration.Validation.ValidateJE_AircraftRegistrationInland();
				AssertNoMessageErrorContaining("When JE_TransportModeInland = AIR, JE_AircraftRegistrationInland and JE_TransportIDInland are filled", declaration.JE_AircraftRegistrationInlandInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_TransportIDInlandWithTrailer1RegNo_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = RAI", () =>
		{
			declaration.JE_TransportModeInland = "RAI";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = "123";
			declaration.JE_Trailer1RegNo = ZString.Empty;
		}
		void ValidateTransportIDInland() => declaration.Validation.ValidateJE_TransportIDInland();
	}

	public void TestCheckJE_Trailer1RegNoWithTransportIDInland_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = RAI", () =>
		{
			declaration.JE_TransportModeInland = "RAI";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = "123";
		}
		void ValidateTrailer1RegNo() => declaration.Validation.ValidateJE_Trailer1RegNo();
	}

	public void TestCheckJE_TransportIDInlandWithAircraftRegistrationInland_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = AIR", () =>
		{
			declaration.JE_TransportModeInland = "AIR";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_AircraftRegistrationInland = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = "123";
			declaration.JE_AircraftRegistrationInland = ZString.Empty;
		}
		void ValidateTransportIDInland() => declaration.Validation.ValidateJE_TransportIDInland();
	}

	public void TestCheckJE_AircraftRegistrationInlandWithTransportIDInland_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = AIR", () =>
		{
			declaration.JE_TransportModeInland = "AIR";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateAircraftRegistrationInland, declaration.JE_AircraftRegistrationInlandInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_AircraftRegistrationInland = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_AircraftRegistrationInland = "123";
		}
		void ValidateAircraftRegistrationInland() => declaration.Validation.ValidateJE_AircraftRegistrationInland();
	}

	public void TestCheckJE_TransportIDInlandWithTrailer1RegNo_Trailer2RegNo_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = ROA", () =>
		{
			declaration.JE_TransportModeInland = "ROA";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTransportIDInland, declaration.JE_TransportIDInlandInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = ZString.Empty;
			declaration.JE_Trailer2RegNo = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = "123";
			declaration.JE_Trailer1RegNo = ZString.Empty;
			declaration.JE_Trailer2RegNo = ZString.Empty;
		}
		void ValidateTransportIDInland() => declaration.Validation.ValidateJE_TransportIDInland();
	}

	public void TestCheckJE_Trailer1RegNoWithTransportIDInland_Trailer2RegNo_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = ROA", () =>
		{
			declaration.JE_TransportModeInland = "ROA";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer1RegNo, declaration.JE_Trailer1RegNoInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = ZString.Empty;
			declaration.JE_Trailer2RegNo = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = "123";
			declaration.JE_Trailer2RegNo = ZString.Empty;
		}
		void ValidateTrailer1RegNo() => declaration.Validation.ValidateJE_Trailer1RegNo();
	}

	public void TestCheckJE_Trailer2RegNoWithTrailer1RegNo_TransportIDInland_EntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		CombineAssertions("When Inland MOT = ROA", () =>
		{
			declaration.JE_TransportModeInland = "ROA";
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);

			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, SetAtLeastOneTransportMeansNotEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
		});

		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);

			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
			AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10", SetAllTransportMeansEmpty, ValidateTrailer2RegNo, declaration.JE_Trailer2RegNoInfo);
		});

		void SetAllTransportMeansEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = ZString.Empty;
			declaration.JE_Trailer2RegNo = ZString.Empty;
		}
		void SetAtLeastOneTransportMeansNotEmpty()
		{
			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = ZString.Empty;
			declaration.JE_Trailer2RegNo = "123";
		}
		void ValidateTrailer2RegNo() => declaration.Validation.ValidateJE_Trailer2RegNo();
	}

	public void TestCheckJE_TransportIDInland_MandatoryWithEntryStyle_ProcedureCode_InlandMOT_C0834()
	{
		var expectedMessage = "[C0834] Means of transport must be filled in";
		CombineAssertions("When Inland MOT = IWT", () =>
		{
			declaration.JE_TransportModeInland = "IWT";
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76");

			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10");
		});

		CombineAssertions("When Inland MOT = OWN", () =>
		{
			declaration.JE_TransportModeInland = "OWN";
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76");

			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10");
		});

		CombineAssertions("When Inland MOT = SEA", () =>
		{
			declaration.JE_TransportModeInland = "SEA";
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76");

			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76");
			AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10");
		});

		CombineAssertions("When Inland MOT = AIR", () =>
		{
			declaration.JE_TransportModeInland = "AIR";
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "10");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "11");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "23");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "31");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "EX", procedureCode: "76");

			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "76");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "77");
			AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(entryStyle: "CO", procedureCode: "10");
		});

		void AssertHasMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(string entryStyle, string procedureCode)
		{
			declaration.JE_EntryStyle = entryStyle;
			instruction.CEI_Procedure = procedureCode;
			declaration.JE_TransportIDInland = ZString.Empty;
			AssertHasMessageErrorContaining($"JE_TransportIDInland empty, EntryStyle = {entryStyle}, ProcedureCode = {procedureCode}", declaration.JE_TransportIDInlandInfo, expectedMessage);

			declaration.JE_TransportIDInland = "ABC";
			AssertNoMessageErrorContaining($"JE_TransportIDInland not empty, EntryStyle = {entryStyle}, ProcedureCode = {procedureCode}", declaration.JE_TransportIDInlandInfo, expectedMessage);
		}

		void AssertNoMandatoryMessageErrorOnTransportIDInlandForEntryStyleAndProcedureCodeC0834(string entryStyle, string procedureCode)
		{
			declaration.JE_EntryStyle = entryStyle;
			instruction.CEI_Procedure = procedureCode;

			declaration.JE_TransportIDInland = ZString.Empty;
			AssertNoMessageErrorContaining($"JE_TransportIDInland empty, EntryStyle = {entryStyle}, ProcedureCode = {procedureCode}", declaration.JE_TransportIDInlandInfo, expectedMessage);
		}
	}

	public void TestCheckJE_RN_NKTransportNationalityInlandWithTransportIDInland_AESTP_B2101()
	{
		var expectedMessage = "[B2101] This field must be filled";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_TransportIDInland = "ABC";
				declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
				AssertHasMessageErrorContaining("When JE_TransportIDInland is filled, JE_RN_NKTransportNationalityInland empty", declaration.JE_RN_NKTransportNationalityInlandInfo, expectedMessage);

				declaration.JE_RN_NKTransportNationalityInland = "IT";
				AssertNoMessageErrorContaining("When JE_TransportIDInland is filled, JE_RN_NKTransportNationalityInland filled", declaration.JE_RN_NKTransportNationalityInlandInfo, expectedMessage);

				declaration.JE_TransportIDInland = ZString.Empty;
				declaration.JE_RN_NKTransportNationalityInland = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_TransportIDInland is empty, JE_RN_NKTransportNationalityInland empty", declaration.JE_RN_NKTransportNationalityInlandInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_TransportIDInland = "ABC";
				declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
				AssertNoMessageErrorContaining("When JE_TransportIDInland is filled, JE_RN_NKTransportNationalityInland empty", declaration.JE_RN_NKTransportNationalityInlandInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_RN_NKTrailer1NationalityWithAircraftRegistrationInland_AESTP_B2101()
	{
		var expectedMessage = "[B2101] This field must be filled";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_AircraftRegistrationInland = "ABC";
				declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
				AssertHasMessageErrorContaining("When JE_AircraftRegistrationInland is filled, JE_RN_NKTrailer1Nationality empty", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);

				declaration.JE_RN_NKTrailer1Nationality = "IT";
				AssertNoMessageErrorContaining("When JE_AircraftRegistrationInland is filled, JE_RN_NKTrailer1Nationality filled", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);

				declaration.JE_AircraftRegistrationInland = ZString.Empty;
				declaration.JE_RN_NKTrailer1Nationality = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_AircraftRegistrationInland is empty, JE_RN_NKTrailer1Nationality empty", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_AircraftRegistrationInland = "ABC";
				declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
				AssertNoMessageErrorContaining("When JE_AircraftRegistrationInland is filled, JE_RN_NKTrailer1Nationality empty", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_RN_NKTrailer1NationalityWithTrailer1RegNo_AESTP_B2101()
	{
		var expectedMessage = "[B2101] This field must be filled";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_Trailer1RegNo = "ABC";
				declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
				AssertHasMessageErrorContaining("When JE_Trailer1RegNo is filled, JE_RN_NKTrailer1Nationality empty", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);

				declaration.JE_RN_NKTrailer1Nationality = "IT";
				AssertNoMessageErrorContaining("When JE_Trailer1RegNo is filled, JE_RN_NKTrailer1Nationality filled", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);

				declaration.JE_Trailer1RegNo = ZString.Empty;
				declaration.JE_RN_NKTrailer1Nationality = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_Trailer1RegNo is empty, JE_RN_NKTrailer1Nationality empty", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_Trailer1RegNo = "ABC";
				declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
				AssertNoMessageErrorContaining("When JE_Trailer1RegNo is filled, JE_RN_NKTrailer1Nationality empty", declaration.JE_RN_NKTrailer1NationalityInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_RN_NKTrailer2NationalityWithTrailer2RegNo_AESTP_B2101()
	{
		var expectedMessage = "[B2101] This field must be filled";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				declaration.JE_Trailer2RegNo = "ABC";
				declaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
				AssertHasMessageErrorContaining("When JE_Trailer2RegNo is filled, JE_RN_NKTrailer2Nationality empty", declaration.JE_RN_NKTrailer2NationalityInfo, expectedMessage);

				declaration.JE_RN_NKTrailer2Nationality = "IT";
				AssertNoMessageErrorContaining("When JE_Trailer2RegNo is filled, JE_RN_NKTrailer2Nationality filled", declaration.JE_RN_NKTrailer2NationalityInfo, expectedMessage);

				declaration.JE_Trailer2RegNo = ZString.Empty;
				declaration.JE_RN_NKTrailer2Nationality = ZString.Empty;
				AssertNoMessageErrorContaining("When JE_Trailer2RegNo is empty, JE_RN_NKTrailer2Nationality empty", declaration.JE_RN_NKTrailer2NationalityInfo, expectedMessage);
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				declaration.JE_Trailer2RegNo = "ABC";
				declaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
				AssertNoMessageErrorContaining("When JE_Trailer2RegNo is filled, JE_RN_NKTrailer2Nationality empty", declaration.JE_RN_NKTrailer2NationalityInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_TransportMeansWithInlandMOT_AESTP_B2101()
	{
		var expectedMessage = "[B2101] This field must be filled";
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				AssertHasMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "SEA");
				AssertHasMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "FIX");
				AssertHasMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "IWT");
				AssertHasMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "OWN");
				AssertHasMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "MAI");

				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "AIR");
				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "XXX");
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "SEA");
				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "FIX");
				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "IWT");
				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "OWN");
				AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(transportModeInland: "MAI");
			});
		}

		void AssertHasMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(string transportModeInland)
		{
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.JE_TransportMeans = ZString.Empty;
			AssertHasMessageErrorContaining($"When JE_TransportModeInland = {transportModeInland}, JE_TransportMeans empty", declaration.JE_TransportMeansInfo, expectedMessage);

			declaration.JE_TransportMeans = "AA";
			AssertNoMessageErrorContaining($"When JE_TransportModeInland = {transportModeInland}, JE_TransportMeans filled", declaration.JE_TransportMeansInfo, expectedMessage);
		}

		void AssertNoMessageErrorWhenJE_TransportMeansEmptyForTransportModeInland(string transportModeInland)
		{
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining($"When JE_TransportModeInland = {transportModeInland}, JE_TransportMeans empty", declaration.JE_TransportMeansInfo, expectedMessage);
		}
	}

	public void TestCheckJE_TransportIDInlandWithInlandMOT_TransportMeans_AESTP_R0473()
	{
		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
		{
			CombineAssertions("TransitionPeriod OFF", () =>
			{
				AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "SEA", transportMeans: "10");
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "SEA", transportMeans: "11");
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "AIR", transportMeans: "10");
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "AIR", transportMeans: "11");

				AssertHasMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "IWT");
				AssertHasMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "FIX");
				AssertHasMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "OWN");
				AssertHasMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "MAI");
				AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "AIR");
			});
		}

		using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
		{
			CombineAssertions("TransitionPeriod ON", () =>
			{
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "SEA", transportMeans: "10");
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "SEA", transportMeans: "11");
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "AIR", transportMeans: "10");
				AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT: "AIR", transportMeans: "11");

				AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "IWT");
				AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "FIX");
				AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "OWN");
				AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "MAI");
				AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(inlandMOT: "AIR");
			});
		}
	}

	public void TestCheckJE_RN_NKTransportNationalityInland_ListValidation()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "Export Nationality");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "IT", "Italy", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_RN_NKTransportNationalityInlandInfo, invalidCode: "XX", validCode: "IT");
	}

	public void TestCheckJE_RN_NKTrailer1Nationality_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_RN_NKTrailer1NationalityInfo, invalidCode: "XX", validCode: "IT");
	}

	public void TestCheckJE_RN_NKTrailer2Nationality_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_RN_NKTrailer2NationalityInfo, invalidCode: "XX", validCode: "IT");
	}

	public void TestCheckJE_TransportMeans_ListValidation()
	{
		declaration.JE_TransportModeInland = "SEA";
		ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_TransportMeansInfo, invalidCode: "99", validCode: "10");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		instruction = declaration.CustomsEntryInstructions.AddNew();
	}

	#region Implementation

	IDisposable TemporarilySetAESTransitionPeriod(bool isInTransitionPeriod)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod
			, RefDataGroupingCodes.EuropeanUnionEUN
			, ZDate.Today
			, isInTransitionPeriod);

	void AssertHasMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(string inlandMOT)
	{
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "10");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "20");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "21");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "30");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "31");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "40");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "41");
		AssertHasMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "80");

		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "11");
	}

	void AssertNoMessageErrorIfTransportIDInlandLowerCaseWithDependentTransportMeans(string inlandMOT)
	{
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "10");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "20");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "21");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "30");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "31");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "40");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "41");
		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "80");

		AssertNoMessageErrorIfTransportIDInlandLowerCase(inlandMOT, transportMeans: "11");
	}

	void AssertHasMessageErrorIfTransportIDInlandLowerCase(string inlandMOT, string transportMeans)
	{
		var expectedMessage = "[R0473] Lowercase letters are not allowed";
		declaration.JE_TransportModeInland = inlandMOT;
		declaration.JE_TransportMeans = transportMeans;

		declaration.JE_TransportIDInland = "abc";
		AssertHasMessageErrorContaining($"When JE_TransportModeInland = {inlandMOT} and JE_TransportMeans = {transportMeans}, JE_TransportIDInland has all lower case letters", declaration.JE_TransportIDInlandInfo, expectedMessage);

		declaration.JE_TransportIDInland = "Abc";
		AssertHasMessageErrorContaining($"When JE_TransportModeInland = {inlandMOT} and JE_TransportMeans = {transportMeans}, JE_TransportIDInland has some lower case letters", declaration.JE_TransportIDInlandInfo, expectedMessage);

		declaration.JE_TransportIDInland = "123";
		AssertNoMessageErrorContaining($"When JE_TransportModeInland = {inlandMOT} and JE_TransportMeans = {transportMeans}, JE_TransportIDInland has all numbers", declaration.JE_TransportIDInlandInfo, expectedMessage);

		declaration.JE_TransportIDInland = "ABC";
		AssertNoMessageErrorContaining($"When JE_TransportModeInland = {inlandMOT} and JE_TransportMeans = {transportMeans}, JE_TransportIDInland has all upper case letters", declaration.JE_TransportIDInlandInfo, expectedMessage);

		declaration.JE_TransportIDInland = ZString.Empty;
		AssertNoMessageErrorContaining($"When JE_TransportModeInland = {inlandMOT} and JE_TransportMeans = {transportMeans}, JE_TransportIDInland is empty", declaration.JE_TransportIDInlandInfo, expectedMessage);
	}

	void AssertNoMessageErrorIfTransportIDInlandLowerCase(string inlandMOT, string transportMeans)
	{
		var expectedMessage = "[R0473] Lowercase letters are not allowed";
		declaration.JE_TransportModeInland = inlandMOT;
		declaration.JE_TransportMeans = transportMeans;

		declaration.JE_TransportIDInland = "abc";
		AssertNoMessageErrorContaining($"When JE_TransportModeInland = {inlandMOT} and JE_TransportMeans = {transportMeans}, JE_TransportIDInland has all lower case letters", declaration.JE_TransportIDInlandInfo, expectedMessage);
	}

	void AssertHasMessageErrorForEntryStyleAndProcedureCodeC0834(string entryStyle
			, string procedureCode
			, Action setAllTransportMeansEmptyAction
			, Action setAtLeastOneTransportMeansNotEmpty
			, Action validateAction
			, ZPropertyInfo targetInfo)
	{
		var expectedMessage = "[C0834] At least one means of transport must be filled in";
		declaration.JE_EntryStyle = entryStyle;
		instruction.CEI_Procedure = procedureCode;
		setAllTransportMeansEmptyAction();
		validateAction();
		AssertHasMessageErrorContaining($"All Transport means are empty, EntryStyle = {entryStyle}, ProcedureCode = {procedureCode}", targetInfo, expectedMessage);

		setAtLeastOneTransportMeansNotEmpty();
		validateAction();
		AssertNoMessageErrorContaining($"All Transport means are not empty, EntryStyle = {entryStyle}, ProcedureCode = {procedureCode}", targetInfo, expectedMessage);
	}

	void AssertNoMessageErrorForEntryStyleAndProcedureCodeC0834(string entryStyle
			, string procedureCode
			, Action setAllTransportMeansEmptyAction
			, Action validateAction
			, ZPropertyInfo targetInfo)
	{
		var expectedMessage = "[C0834] At least one means of transport must be filled in";
		declaration.JE_EntryStyle = entryStyle;
		instruction.CEI_Procedure = procedureCode;
		setAllTransportMeansEmptyAction();
		validateAction();
		AssertNoMessageErrorContaining($"All Transport means are empty, EntryStyle = {entryStyle}, ProcedureCode = {procedureCode}", targetInfo, expectedMessage);
	}

	#endregion

	JobDeclaration declaration;
	CusEntryInstruction instruction;
}
