using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AE;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

class JobDeclarationValidationTest : TestCaseWithFactory
{
	public void TestCheckJE_DateOfArrival()
	{
		declaration.JE_DateOfArrival = ZDateTime.Empty;
		AssertHasMessageErrors(declaration.JE_DateOfArrivalInfo);
		declaration.JE_DateOfArrival = new ZDateTime(2005, 8, 1);
		AssertNoMessageErrors(declaration.JE_DateOfArrivalInfo);
	}

	public void TestMarksAndNumbers() => CombineAssertions(() =>
	{
		declaration.JE_MarksAndNumbersShort = "";
		declaration.Validation.ValidateJE_MarksAndNumbersShort();
		AssertNoMessageErrors("Expected no message errors for JE_MarksAndNumbersShort", declaration.JE_MarksAndNumbersShortInfo);
		declaration.JE_MarksAndNumbers = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque varius, lacus non fermentum dignissim, nunc velit bibendum sapien, ac auctor risus nisi ut lorem. Nulla at magna nec libero dictum consequat. Morbi suscipit, risus eget egestas gravida, lectus lacus volutpat urna, nec varius est justo non augue. Donec blandit tortor a ligula viverra, ut hendrerit sem laoreet.\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
		declaration.JE_MarksAndNumbersShort = declaration.JE_MarksAndNumbers.Substring(0, 35);
		declaration.Validation.ValidateJE_MarksAndNumbersShort();
		AssertHasMessageErrors("Expected message errors when JE_MarksAndNumbers has length more than 350", declaration.JE_MarksAndNumbersShortInfo);
	});

	public void TestCheckJE_RL_NKOrigin()
	{
		declaration.JE_RL_NKOrigin = "";
		AssertHasMessageErrors(declaration.JE_RL_NKOriginInfo);
		declaration.JE_RL_NKOrigin = "AUSYD";
		AssertNoMessageErrors(declaration.JE_RL_NKOriginInfo);
	}

	public void TestJE_RL_NKPortOfArrival()
	{
		declaration.JE_RL_NKPortOfArrival = "";
		Assert(declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		declaration.JE_RL_NKPortOfArrival = "?????";
		Assert(declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		declaration.JE_RL_NKPortOfArrival = "AEQIW";
		Assert(!declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
	}

	public void TestJE_VoyageFlightNo()
	{
		declaration.JE_VoyageFlightNo = "";
		Assert(declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
		declaration.JE_VoyageFlightNo = "TEST123";
		Assert(!declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
	}

	public void TestJE_RL_NKPortOfLoading()
	{
		declaration.JE_RL_NKPortOfLoading = "";
		AssertHasMessageError(declaration.JE_RL_NKPortOfLoadingInfo, "You have not entered a Port Of Loading.");
		declaration.JE_RL_NKPortOfLoading = "?????";
		AssertHasMessageError(declaration.JE_RL_NKPortOfLoadingInfo, "This port code is invalid. Please check against the transport mode and shipment type.");
		declaration.JE_RL_NKPortOfLoading = "AUSYD";
		Assert("No message errors", !declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
		var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
		port.RL_IATA = ZString.Empty;
		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		declaration.JE_RL_NKPortOfLoading = port.RL_Code;
		AssertHasMessageError(declaration.JE_RL_NKPortOfLoadingInfo, "Port of Loading has no IATA Code");
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		declaration.Validation.ValidateJE_RL_NKPortOfLoading();
		AssertNoMessageError(declaration.JE_RL_NKPortOfLoadingInfo, "Port of Loading has no IATA Code");
	}

	public void TestJE_TotalNoOfPacks()
	{
		declaration.JE_TotalNoOfPacks = 0;
		Assert(declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
		declaration.JE_TotalNoOfPacks = 12;
		Assert(!declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
	}

	public void TestJE_TotalWeight()
	{
		declaration.JE_TotalWeight = 0;
		Assert(declaration.JE_TotalWeightInfo.HasMessageErrors());
		declaration.JE_TotalWeight = 100;
		Assert(!declaration.JE_TotalWeightInfo.HasMessageErrors());
	}

	public void TestJE_TotalWeightUnit()
	{
		declaration.JE_TotalWeightUnit = "";
		Assert(declaration.JE_TotalWeightUnitInfo.HasMessageErrors());
		declaration.JE_TotalWeightUnit = "HJ";
		Assert(declaration.JE_TotalWeightUnitInfo.HasMessageErrors());
		declaration.JE_TotalWeightUnit = "KG";
		Assert(!declaration.JE_TotalWeightUnitInfo.HasMessageErrors());
	}

	public void TestJE_TransportMode()
	{
		declaration.JE_TransportMode = "";
		Assert(declaration.JE_TransportModeInfo.HasMessageErrors());
		declaration.JE_TransportMode = "HED";
		Assert(declaration.JE_TransportModeInfo.HasMessageErrors());
		declaration.JE_TransportMode = Constants.TransportModes.Sea;
		Assert(!declaration.JE_TransportModeInfo.HasMessageErrors());
	}

	public void TestFinalDestination()
	{
		CheckDestination(AEJobMessageTypeList.Codes.Import, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.TemporaryAdmission, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.Drawback, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.CargoTransfer, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.Export, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.ExWarehouse, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.MiscellaneousCustoms, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.Refund, true, false, false);
		CheckDestination(AEJobMessageTypeList.Codes.Transit, false, true, true);
		CheckDestination(AEJobMessageTypeList.Codes.Transfer, false, true, true);
	}

	void CheckDestination(string messageType, bool canBeAE, bool canBeGCC, bool canBeNonAEGCC)
	{
		declaration.JE_MessageType = messageType;
		declaration.JE_RL_NKFinalDestination = "AEDXB";
		CheckDestination("AEDXB", canBeAE);
		CheckDestination("QADOH", canBeGCC);
		CheckDestination("OMFAH", canBeGCC);
		CheckDestination("SAAHB", canBeGCC);
		CheckDestination("KWJAH", canBeGCC);
		CheckDestination("BHBAH", canBeGCC);
		CheckDestination("USLAX", canBeNonAEGCC);
	}

	void CheckDestination(string portCode, bool shouldHaveNoMessageError)
	{
		declaration.JE_RL_NKFinalDestination = portCode;
		if (shouldHaveNoMessageError)
		{
			AssertNoMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);
		}
		else
		{
			AssertHasMessageErrors(declaration.JE_RL_NKFinalDestinationInfo);
		}
	}

	public void TestInTransitDestinationAllowsGCCCountry()
	{
		declaration.JE_MessageType = AEJobMessageTypeList.Codes.Transfer;
		declaration.JE_RL_NKFinalDestination = "YEBYD";
		Assert(!declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
	}

	public void TestJE_ImporterCode()
	{
		Assert(!declaration.JE_OH_ImporterInfo.HasNotifications());
		declaration.JE_MessageType = AEJobMessageTypeList.Codes.Transfer;
		declaration.JE_TypeOfGoods = TypeOfGoodsList.Codes.HighValueAboveDeminimis;
		declaration.JE_OH_Importer = ZGuid.Empty;
		Assert(!declaration.JE_OH_ImporterInfo.HasNotifications());
		declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
		declaration.JE_MessageType = AEJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = ZGuid.Empty;
		Assert(declaration.JE_OH_ImporterInfo.HasMessageErrors());
		declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
		Assert(!declaration.JE_OH_ImporterInfo.HasMessageErrors());
	}

	public void TestJE_OH_Supplier()
	{
		Assert(!declaration.JE_OH_SupplierInfo.HasNotifications());
		declaration.JE_MessageType = AEJobMessageTypeList.Codes.Transfer;
		declaration.JE_TypeOfGoods = TypeOfGoodsList.Codes.HighValueAboveDeminimis;
		declaration.JE_OH_Supplier = ZGuid.Empty;
		Assert(!declaration.JE_OH_SupplierInfo.HasNotifications());
		declaration.JE_OH_Supplier = OrgHeader.New(Factory).PK;
		declaration.JE_MessageType = AEJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Supplier = ZGuid.Empty;
		Assert(declaration.JE_OH_SupplierInfo.HasMessageErrors());
		declaration.JE_OH_Supplier = OrgHeader.New(Factory).PK;
		Assert(!declaration.JE_OH_SupplierInfo.HasMessageErrors());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		qantas = Factory.New<OrgHeader>();
	}

	protected OrgHeader qantas;
	protected JobDeclaration declaration;
}
