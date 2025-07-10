using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMWarehouseHeaderProcedureWrapperTest : IMHeaderWrapperTest
{
	public override void TestDeliveryCosts()
	{
		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = new IMWarehouseHeaderProcedureWrapper(entryHeader);
		entryHeader.CH_FreightAdjustment = 0m;
		AssertNull(sadHeaderWrapper.DeliveryCosts);
		entryHeader.CH_FreightAdjustment = 12313.12m;
		AssertNull(sadHeaderWrapper.DeliveryCosts);
	}

	public override void TestCountryOfDestination()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "FIN";
		refUNLOCO.RL_RN_NKCountryCode = "IT";
		jobDeclaration.JE_RL_NKFinalDestination = "";

		CombineAssertions("When it has a warehouse procedure and CPC NOT IN (76,77)", () =>
		{
			entryInstruction.CEI_Procedure = "71";
			sadHeaderWrapper = new IMWarehouseHeaderProcedureWrapper(entryHeader);
			AssertEquals("", sadHeaderWrapper.CountryOfDestination);
		});

		CombineAssertions("When it has a warehouse procedure and CPC IN (76,77)", () =>
		{
			entryInstruction.CEI_Procedure = "76";
			sadHeaderWrapper = new IMWarehouseHeaderProcedureWrapper(entryHeader);
			AssertEquals(jobDeclaration.JE_GoodsDestination, sadHeaderWrapper.CountryOfDestination);

			entryInstruction.CEI_Procedure = "77";
			sadHeaderWrapper = new IMWarehouseHeaderProcedureWrapper(entryHeader);
			AssertEquals(jobDeclaration.JE_GoodsDestination, sadHeaderWrapper.CountryOfDestination);
		});
	}

	public override void TestProvinceOfDestination()
	{
		var wrapper = new IMWarehouseHeaderProcedureWrapper(entryHeader);
		AssertEquals(ZString.Empty, wrapper.ProvinceOfDestination);
	}

	public override void TestMeansOfTransportOnArrival()
	{
		AssertNotNull("MeansOfTransportOnArrival", sadHeaderWrapper.MeansOfTransportOnArrival);
		AssertType<SADMeansOfTransportWrapper>("MeansOfTransportOnArrival type", sadHeaderWrapper.MeansOfTransportOnArrival);

		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		jobDeclaration.ZG_Box18TransportNationality = "KR";
		jobDeclaration.ZG_Box18TransportID = "RX2839A";
		var meansOfTransportOnArrival = sadHeaderWrapper.MeansOfTransportOnArrival;
		AssertEquals(ZString.Empty, meansOfTransportOnArrival.Nationality);
		AssertEquals(ZString.Empty, meansOfTransportOnArrival.Identity);
	}

	public override void TestIsContainerizedTransport()
	{
		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		jobDeclaration.JE_ContainerMode = "FCL";
		AssertNull(sadHeaderWrapper.IsContainerizedTransport);
	}

	public override void TestTermsOfDelivery()
	{
		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertNotNull("TermsOfDelivery", sadHeaderWrapper.TermsOfDelivery);
		AssertType<SADEmptyTermsOfDeliveryWrapper>("TermsOfDelivery type", sadHeaderWrapper.TermsOfDelivery);
	}

	public override void TestMeansOfTransportCrossingBorder()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(sadHeaderWrapper.MeansOfTransportCrossingBorder);
			AssertType<SADMeansOfTransportWrapper>(sadHeaderWrapper.MeansOfTransportCrossingBorder);
		});

		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		jobDeclaration.JE_TransportMode = "AIR";
		jobDeclaration.JE_VesselName = "AE";
		jobDeclaration.JE_VoyageFlightNo = "4343";
		var meansOfTransportCrossingBorder = sadHeaderWrapper.MeansOfTransportCrossingBorder;
		AssertEquals(ZString.Empty, meansOfTransportCrossingBorder.Nationality);
		AssertEquals(ZString.Empty, meansOfTransportCrossingBorder.Identity);
	}

	public override void TestTransactionData()
	{
		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertNotNull(sadHeaderWrapper.TransactionData);
		AssertType<SADEmptyTransactionDataWrapper>(sadHeaderWrapper.TransactionData);
	}

	public override void TestTransportModeAtBorder()
	{
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(ZString.Empty, sadHeaderWrapper.TransportModeAtBorder);
	}

	public override void TestInlandTransportMode()
	{
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		AssertEquals(ZString.Empty, sadHeaderWrapper.InlandTransportMode);
	}

	public override void TestEntryCustomsOffice()
	{
		var officeCode = Factory.New<EuOfficeCode>();
		jobDeclaration.CustomsOffices.Add(officeCode);
		officeCode.CY_Code = "ENT";
		officeCode.CY_Data = "IT303199";

		entryInstruction.CEI_Procedure = "71";
		sadHeaderWrapper = GetHeaderWrapper(entryHeader);
		var entryCustomsOffice = sadHeaderWrapper.EntryCustomsOffice;
		AssertNotNull(entryCustomsOffice);
		AssertType<IMHeaderEmptyEntryCustomsOfficeWrapper>(entryCustomsOffice);
		AssertEquals("", entryCustomsOffice.Name);
		AssertEquals("", entryCustomsOffice.Nationality);
		AssertEquals("", entryCustomsOffice.ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpRefData();
	}

	protected override IMHeaderWrapper GetHeaderWrapper(CusEntryHeader entryHeader)
	{
		return new IMWarehouseHeaderProcedureWrapper(entryHeader);
	}
}
