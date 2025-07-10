using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMNonWarehouseHeaderProcedureWrapperTest : IMHeaderWrapperTest
{
	public override void TestDeliveryCosts()
	{
		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(0m, sadHeaderWrapper.DeliveryCosts);
		entryHeader.CH_FreightAdjustment = 12313.12m;
		AssertEquals(12313.12m, sadHeaderWrapper.DeliveryCosts);
		entryHeader.CH_FreightAdjustment = 0m;
		AssertEquals(0m, sadHeaderWrapper.DeliveryCosts);
	}

	public override void TestCountryOfDestination()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "FIN";
		refUNLOCO.RL_RN_NKCountryCode = "IT";
		jobDeclaration.JE_RL_NKFinalDestination = "FIN";

		entryInstruction.CEI_Procedure = "";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(jobDeclaration.JE_GoodsDestination, sadHeaderWrapper.CountryOfDestination);

		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(jobDeclaration.JE_GoodsDestination, sadHeaderWrapper.CountryOfDestination);
	}

	public override void TestProvinceOfDestination()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "FIN";
		refUNLOCO.RL_RN_NKCountryCode = "IT";
		var refCountryStates = Factory.New<RefCountryStates>();
		refCountryStates.RW_Code = "AP";
		refUNLOCO.RL_RW = refCountryStates.PK;
		AssertEquals(ZString.Empty, sadHeaderWrapper.ProvinceOfDestination);

		entryInstruction.CEI_Procedure = "";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		jobDeclaration.JE_RL_NKFinalDestination = "FIN";
		AssertEquals("AP", sadHeaderWrapper.ProvinceOfDestination);

		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals("AP", sadHeaderWrapper.ProvinceOfDestination);
	}

	public override void TestMeansOfTransportOnArrival()
	{
		AssertNotNull("MeansOfTransportOnArrival", sadHeaderWrapper.MeansOfTransportOnArrival);
		AssertType<SADMeansOfTransportWrapper>("MeansOfTransportOnArrival type", sadHeaderWrapper.MeansOfTransportOnArrival);

		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		jobDeclaration.ZG_Box18TransportNationality = "";
		jobDeclaration.ZG_Box18TransportID = "";
		var meansOfTransportOnArrival = sadHeaderWrapper.MeansOfTransportOnArrival;
		AssertEquals(ZString.Empty, meansOfTransportOnArrival.Nationality);
		AssertEquals(ZString.Empty, meansOfTransportOnArrival.Identity);

		jobDeclaration.ZG_Box18TransportNationality = "KR";
		jobDeclaration.ZG_Box18TransportID = "RX2839A";
		meansOfTransportOnArrival = sadHeaderWrapper.MeansOfTransportOnArrival;
		AssertEquals("KR", meansOfTransportOnArrival.Nationality);
		AssertEquals("RX2839A", meansOfTransportOnArrival.Identity);
	}

	public override void TestIsContainerizedTransport()
	{
		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);

		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
		AssertEquals("When JE_ContainerMode is FCL", true, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
		AssertEquals("When JE_ContainerMode is LCL", true, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.ULD;
		AssertEquals("When JE_ContainerMode is ULD", true, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
		AssertEquals("When JE_ContainerMode is CNT", true, sadHeaderWrapper.IsContainerizedTransport);

		jobDeclaration.JE_ContainerMode = ZString.Empty;
		AssertEquals("When JE_ContainerMode is empty", false, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = "XYZ";
		AssertEquals("When JE_ContainerMode is an invalid value", false, sadHeaderWrapper.IsContainerizedTransport);
	}

	public override void TestTermsOfDelivery()
	{
		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertNotNull("TermsOfDelivery", sadHeaderWrapper.TermsOfDelivery);
		AssertType<SADTermsOfDeliveryWrapper>("TermsOfDelivery type", sadHeaderWrapper.TermsOfDelivery);
	}

	public override void TestMeansOfTransportCrossingBorder()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(sadHeaderWrapper.MeansOfTransportCrossingBorder);
			AssertType<SADMeansOfTransportWrapper>(sadHeaderWrapper.MeansOfTransportCrossingBorder);
		});

		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(ZString.Empty, sadHeaderWrapper.MeansOfTransportCrossingBorder.Nationality);

		jobDeclaration.JE_RN_NKTransportNationality = "SG";
		AssertEquals("SG", sadHeaderWrapper.MeansOfTransportCrossingBorder.Nationality);
		AssertEquals(ZString.Empty, sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

		jobDeclaration.JE_TransportMode = "SEA";
		jobDeclaration.JE_VesselName = "SE";
		jobDeclaration.JE_VoyageFlightNo = "3434";
		AssertEquals("SE3434", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

		jobDeclaration.JE_TransportMode = "AIR";
		jobDeclaration.JE_VesselName = "AE";
		jobDeclaration.JE_VoyageFlightNo = "4343";
		AssertEquals("4343", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

		jobDeclaration.JE_TransportMode = "ROA";
		jobDeclaration.JE_VesselName = "SE";
		jobDeclaration.JE_VoyageFlightNo = "3434";
		AssertEquals("SE", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

		jobDeclaration.JE_TransportMode = "";
		jobDeclaration.JE_VesselName = "SE";
		jobDeclaration.JE_VoyageFlightNo = "3434";
		AssertEquals("SE", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);
	}

	public override void TestTransactionData()
	{
		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertNotNull(sadHeaderWrapper.TransactionData);
		AssertType<SADTransactionDataWrapper>(sadHeaderWrapper.TransactionData);
	}

	public override void TestTransportModeAtBorder()
	{
		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(ZString.Empty, sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "SEA";
		AssertEquals("1", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "RAI";
		AssertEquals("2", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "ROA";
		AssertEquals("3", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "AIR";
		AssertEquals("4", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "MAI";
		AssertEquals("5", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "FIX";
		AssertEquals("7", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "IWT";
		AssertEquals("8", sadHeaderWrapper.TransportModeAtBorder);
		jobDeclaration.JE_TransportMode = "OWN";
		AssertEquals("9", sadHeaderWrapper.TransportModeAtBorder);
	}

	public override void TestInlandTransportMode()
	{
		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(ZString.Empty, sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "SEA";
		AssertEquals("1", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "RAI";
		AssertEquals("2", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "ROA";
		AssertEquals("3", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "AIR";
		AssertEquals("4", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "MAI";
		AssertEquals("5", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "FIX";
		AssertEquals("7", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "IWT";
		AssertEquals("8", sadHeaderWrapper.InlandTransportMode);
		jobDeclaration.JE_TransportModeInland = "OWN";
		AssertEquals("9", sadHeaderWrapper.InlandTransportMode);
	}

	public override void TestEntryCustomsOffice()
	{
		var officeCode = jobDeclaration.CustomsOffices.Find(x => x.CY_Code == "ENT").SingleOrDefault();
		if (officeCode == null)
		{
			officeCode = Factory.New<EuOfficeCode>();
			jobDeclaration.CustomsOffices.Add(officeCode);
		}
		officeCode.CY_Code = "ENT";
		officeCode.CY_Data = "IT303199";

		entryInstruction.CEI_Procedure = "40";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		var entryCustomsOffice = sadHeaderWrapper.EntryCustomsOffice;
		AssertNotNull(entryCustomsOffice);
		AssertType<IMHeaderEntryCustomsOfficeWrapper>(entryCustomsOffice);
		AssertEquals("", entryCustomsOffice.Name);
		AssertEquals("IT", entryCustomsOffice.Nationality);
		AssertEquals("303199", entryCustomsOffice.ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpRefData();
	}

	protected override IMHeaderWrapper GetHeaderWrapper(CusEntryHeader entryHeader)
	{
		return new IMNonWarehouseHeaderProcedureWrapper(entryHeader);
	}
}
