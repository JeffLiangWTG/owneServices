using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class DepartureMeansOfTransportWrappersProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryInstructionCustomsMessageWrapper(null));
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryInstructionCustomsMessageWrapper(Factory.New<CusEntryInstruction>()));
	}

	public void TestDepartureMeansOfTransport_WhenTransportModeInlandIsAIR()
	{
		declaration.JE_TransportModeInland = "AIR";

		var wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 0, wrapper.DepartureMeansOfTransports.Count);

		declaration.JE_TransportIDInland = "IATA0";
		declaration.JE_RN_NKTransportNationalityInland = "IT";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "IATA0", 40, "IT");

		declaration.JE_TransportIDInland = "";
		declaration.JE_AircraftRegistrationInland = "PLANE";
		declaration.JE_RN_NKTrailer1Nationality = "US";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "PLANE", 41, "US");

		declaration.JE_TransportIDInland = "IATA1";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 2, wrapper.DepartureMeansOfTransports.Count);
	}

	public void TestDepartureMeansOfTransport_WhenTransportModeInlandRAI()
	{
		declaration.JE_TransportModeInland = "RAI";

		var wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 0, wrapper.DepartureMeansOfTransports.Count);

		declaration.JE_TransportIDInland = "TRAIN N0";
		declaration.JE_RN_NKTransportNationalityInland = "IT";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "TRAIN N0", 21, "IT");

		declaration.JE_TransportIDInland = "";
		declaration.JE_Trailer1RegNo = "WAGON 1";
		declaration.JE_RN_NKTrailer1Nationality = "US";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "WAGON 1", 20, "US");

		declaration.JE_TransportIDInland = "TRAIN";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 2, wrapper.DepartureMeansOfTransports.Count);
	}

	public void TestDepartureMeansOfTransport_WhenTransportModeInlandROA()
	{
		declaration.JE_TransportModeInland = "ROA";

		var wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 0, wrapper.DepartureMeansOfTransports.Count);

		declaration.JE_TransportIDInland = "TRAN ID";
		declaration.JE_RN_NKTransportNationalityInland = "IT";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "TRAN ID", 30, "IT");

		declaration.JE_TransportIDInland = "";
		declaration.JE_Trailer1RegNo = "TRAILER 1";
		declaration.JE_RN_NKTrailer1Nationality = "US";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "TRAILER 1", 31, "US");

		declaration.JE_Trailer1RegNo = "";
		declaration.JE_Trailer2RegNo = "TRAILER 2";
		declaration.JE_RN_NKTrailer2Nationality = "CA";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "TRAILER 2", 31, "CA");

		declaration.JE_TransportIDInland = "TRAIN";
		declaration.JE_Trailer1RegNo = "TRAILER 1";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 3, wrapper.DepartureMeansOfTransports.Count);
	}

	public void TestDepartureMeansOfTransport_WhenTransportModeInlandIsSEAor()
	{
		declaration.JE_TransportModeInland = "SEA";

		var wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 0, wrapper.DepartureMeansOfTransports.Count);

		declaration.JE_TransportIDInland = "VESSEL ID";
		declaration.JE_RN_NKTransportNationalityInland = "IT";
		declaration.JE_TransportMeans = "80";
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", 1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(wrapper.DepartureMeansOfTransports.ElementAt(0), "VESSEL ID", 80, "IT");
	}

	public void TestDepartureMeansOfTransport_AgainstDeclarationTypeAndProcedure()
	{
		CombineAssertions(() =>
		 {
			 declaration.JE_TransportIDInland = "TRAN ID";

			 declaration.JE_EntryStyle = "EX";
			 declaration.JE_TransportModeInland = "FIX";
			 entryInstruction.CEI_Procedure = "21";
			 var wrapper = GetNewWrapper();
			 AssertDepartureMeansOfTransportsCount(wrapper.DepartureMeansOfTransports, 0);

			 declaration.JE_EntryStyle = "EX";
			 declaration.JE_TransportModeInland = "AIR";
			 entryInstruction.CEI_Procedure = "21";
			 wrapper = GetNewWrapper();
			 AssertDepartureMeansOfTransportsCount(wrapper.DepartureMeansOfTransports, 1);

			 declaration.JE_EntryStyle = "EX";
			 declaration.JE_TransportModeInland = "FIX";
			 entryInstruction.CEI_Procedure = "11";
			 wrapper = GetNewWrapper();
			 AssertDepartureMeansOfTransportsCount(wrapper.DepartureMeansOfTransports, 1);

			 officeOfPresentation.CY_Data = "";
			 declaration.JE_CustomsOffice = "CUS9999";

			 declaration.JE_EntryStyle = "CO";
			 declaration.JE_TransportModeInland = "FIX";
			 entryInstruction.CEI_Procedure = "76";
			 wrapper = GetNewWrapper();
			 AssertDepartureMeansOfTransportsCount(wrapper.DepartureMeansOfTransports, 0);

			 declaration.JE_EntryStyle = "CO";
			 declaration.JE_TransportModeInland = "AIR";
			 entryInstruction.CEI_Procedure = "76";
			 wrapper = GetNewWrapper();
			 AssertDepartureMeansOfTransportsCount(wrapper.DepartureMeansOfTransports, 1);

			 declaration.JE_EntryStyle = "CO";
			 declaration.JE_TransportModeInland = "AIR";
			 entryInstruction.CEI_Procedure = "80";
			 wrapper = GetNewWrapper();
			 AssertDepartureMeansOfTransportsCount(wrapper.DepartureMeansOfTransports, 0);
		 });
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		officeOfPresentation = FindOrAddNewOffice("PRE");
		officeOfExit = FindOrAddNewOffice("EXT");

		declaration.JE_EntryStyle = "EX";
		entryInstruction.CEI_SubStyle = "A";
		officeOfPresentation.CY_Data = "PRE9999";
		officeOfExit.CY_Data = "EXT9999";
		entryInstruction.CEI_Procedure = "21";
	}

	EuOfficeCode FindOrAddNewOffice(string code)
	{
		return declaration.CustomsOffices.Find(x => x.CY_Code == code).SingleOrDefault()
				?? declaration.CustomsOffices.AddNew(code);
	}

	ICusEntryInstructionCustomsMessageWrapper GetNewWrapper() => new CusEntryInstructionCustomsMessageWrapper(entryInstruction);

	void AssertDepartureMeansOfTransportsCount(IReadOnlyCollection<IMeansOfTransport> departureMeansOfTransports, int expectedCount)
	{
		var assertionMessage = $@"EntryStyle: [{declaration.JE_EntryStyle}],
TransportModeInland: [{declaration.JE_TransportModeInland}],
Procedure: [{entryInstruction.CEI_Procedure}]";

		AssertEquals($"{assertionMessage}, {nameof(ICusEntryInstructionCustomsMessageWrapper.DepartureMeansOfTransports)} count", expectedCount, departureMeansOfTransports.Count);
	}

	void AssertDepartureMeansOfTransport(IMeansOfTransport departureMeansOfTransport
		, string expectedIdentificationNumber
		, int expectedTypeOfIdentification
		, string expectedNationality)
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(IMeansOfTransport.IdentificationNumber), expectedIdentificationNumber, departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), expectedTypeOfIdentification, departureMeansOfTransport.TypeOfIdentification);
			AssertEquals(nameof(IMeansOfTransport.Nationality), expectedNationality, departureMeansOfTransport.Nationality);
		});
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	EuOfficeCode officeOfPresentation, officeOfExit;
}
