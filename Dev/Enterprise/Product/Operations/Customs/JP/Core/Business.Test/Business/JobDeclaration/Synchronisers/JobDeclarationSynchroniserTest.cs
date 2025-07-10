using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business.Testing;

sealed class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
{
	public void TestSynchroniseJS_HBLContainerPackModeOverride()
	{
		var jpDeclaration = declaration as JobDeclaration;
		var synchroniser = jpDeclaration.ShipmentSynchroniser;
		CombineAssertions(() =>
		{
			TestReceiptMode(ZString.Empty, ZString.Empty, ZString.Empty);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT, ZString.Empty, ZString.Empty);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR, ZString.Empty, ReceiptModeList.Codes.Mode53);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS, ZString.Empty, ReceiptModeList.Codes.Mode52);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT, ReceiptModeList.Codes.Mode52, ZString.Empty);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, ReceiptModeList.Codes.Mode52, ReceiptModeList.Codes.Mode52);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CFS_CY, ReceiptModeList.Codes.Mode52, ReceiptModeList.Codes.Mode51);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, ReceiptModeList.Codes.Mode52, ReceiptModeList.Codes.Mode53);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CY_CY, ReceiptModeList.Codes.Mode51, ReceiptModeList.Codes.Mode51);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CY_CFS, ReceiptModeList.Codes.Mode51, ReceiptModeList.Codes.Mode52);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR, ReceiptModeList.Codes.Mode51, ReceiptModeList.Codes.Mode53);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT, ReceiptModeList.Codes.Mode53, ZString.Empty);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, ReceiptModeList.Codes.Mode53, ReceiptModeList.Codes.Mode53);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, ReceiptModeList.Codes.Mode53, ReceiptModeList.Codes.Mode52);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY, ReceiptModeList.Codes.Mode53, ReceiptModeList.Codes.Mode51);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT, ReceiptModeList.Codes.Mode53, ReceiptModeList.Codes.Mode54);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR, ReceiptModeList.Codes.Mode54, ReceiptModeList.Codes.Mode53);
			TestReceiptMode(Core.Constants.HBLDeliveryModes.Codes.PORT_PORT, ReceiptModeList.Codes.Mode54, ReceiptModeList.Codes.Mode54);
		});

		void TestReceiptMode(string sourceValue, string expectedReceiptModeValue, string expectedDeliveryModeValue)
		{
			Shipment.JS_HBLContainerPackModeOverride = sourceValue;
			synchroniser.Synchronise(true);
			AssertEquals(expectedReceiptModeValue, jpDeclaration.JE_ReceiptMode);
		}

		CombineAssertions(() =>
		{
			TestDeliveryMode(ZString.Empty, ZString.Empty, ZString.Empty);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT, ZString.Empty, ZString.Empty);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR, ZString.Empty, DeliveryModeList.Codes.Mode53);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS, ZString.Empty, DeliveryModeList.Codes.Mode52);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT, DeliveryModeList.Codes.Mode52, ZString.Empty);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, DeliveryModeList.Codes.Mode52, DeliveryModeList.Codes.Mode52);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CFS_CY, DeliveryModeList.Codes.Mode52, DeliveryModeList.Codes.Mode51);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, DeliveryModeList.Codes.Mode52, DeliveryModeList.Codes.Mode53);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CY_CY, DeliveryModeList.Codes.Mode51, DeliveryModeList.Codes.Mode51);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CY_CFS, DeliveryModeList.Codes.Mode51, DeliveryModeList.Codes.Mode52);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR, DeliveryModeList.Codes.Mode51, DeliveryModeList.Codes.Mode53);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT, DeliveryModeList.Codes.Mode53, ZString.Empty);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, DeliveryModeList.Codes.Mode53, DeliveryModeList.Codes.Mode53);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, DeliveryModeList.Codes.Mode53, DeliveryModeList.Codes.Mode52);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY, DeliveryModeList.Codes.Mode53, DeliveryModeList.Codes.Mode51);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT, DeliveryModeList.Codes.Mode53, DeliveryModeList.Codes.Mode54);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR, DeliveryModeList.Codes.Mode54, DeliveryModeList.Codes.Mode53);
			TestDeliveryMode(Core.Constants.HBLDeliveryModes.Codes.PORT_PORT, DeliveryModeList.Codes.Mode54, DeliveryModeList.Codes.Mode54);
		});

		void TestDeliveryMode(string sourceValue, string expectedReceiptModeValue, string expectedDeliveryModeValue)
		{
			Shipment.JS_HBLContainerPackModeOverride = sourceValue;
			synchroniser.Synchronise(true);
			AssertEquals(expectedDeliveryModeValue, jpDeclaration.JE_DeliveryMode);
		}
	}

	public new void TestRoadShipmentWithSeveralTransportLegs()
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_MasterBillNum = "CNRU901600152785";
		consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consol.JK_RL_NKLoadPort = "KRSEL";
		consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

		var leg1 = consol.Transports[0];
		leg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
		leg1.JW_RL_NKLoadPort = "KRSEL";
		leg1.JW_RL_NKDiscPort = "AUSYD";
		leg1.JW_LegOrder = (ZByte)1;

		var leg2 = consol.Transports.AddNew();
		leg2.JW_TransportMode = Core.Constants.TransportModes.Rail;
		leg2.JW_RL_NKLoadPort = "AUSYD";
		leg2.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		leg2.JW_LegOrder = (ZByte)2;

		var shipment = Factory.New<ForwardingShipment>();
		shipment.Consols.Add(consol);
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_HouseBill = ZString.Empty;
		shipment.JS_RL_NKOrigin = "KRSEL";
		shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

		var declaration = Factory.New<BaseJobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_JS = shipment.PK;
		declaration.ShipmentSynchroniser.Synchronise(true);

		AssertEquals("should not create an empty bill with no house bill number", 1, declaration.Bills.Count);
		AssertEquals(ZString.Empty, declaration.JE_HouseBill);
		AssertEquals(Core.Constants.TransportModes.Rail, declaration.JE_TransportMode);
	}

	public override void TestLoadAndDischargeSyncroniseToCorrectTransport_Export()
	{
		var declaration = GetJobDeclaration() as JobDeclaration;
		var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode)).Code;

		var consol = Factory.New<ForwardingConsol>();
		consol.JK_RL_NKLoadPort = localPort;
		consol.JK_RL_NKDischargePort = "KRANY";

		var firstInternationalLegTransport = consol.Transports[0];
		firstInternationalLegTransport.JW_RL_NKLoadPort = localPort;
		firstInternationalLegTransport.JW_RL_NKDiscPort = "AUSYD";
		firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
		firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
		firstInternationalLegTransport.JW_LegOrder = 1;

		var secondInternationalLegTransport = consol.Transports.AddNew();
		secondInternationalLegTransport.JW_RL_NKLoadPort = "AUSYD";
		secondInternationalLegTransport.JW_RL_NKDiscPort = "KRANY";
		secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
		secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
		secondInternationalLegTransport.JW_LegOrder = 2;

		var shipment = consol.Shipments.AddNew();
		declaration.JE_JS = shipment.PK;
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

		AssertEquals("Precondition: Declaration.JE_RL_NKPortOfLoading", "", declaration.JE_RL_NKPortOfLoading);
		AssertEquals("Precondition: Declaration.JE_RL_NKPortOfArrival", "", declaration.JE_RL_NKPortOfArrival);

		decSynchroniser = new JobDeclarationSynchroniser(declaration);
		decSynchroniser.Synchronise(true);

		AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", localPort, declaration.JE_RL_NKPortOfLoading);
		AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", "AUSYD", declaration.JE_RL_NKPortOfArrival);
		AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);
		AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(1), declaration.JE_DateOfArrival);
	}

	public override void TestLoadAndDischargeSyncroniseToCorrectTransport_Import()
	{
		var declaration = GetJobDeclaration() as JobDeclaration;
		var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode)).Code;

		var consol = Factory.New<ForwardingConsol>();
		consol.JK_RL_NKLoadPort = "KRANY";
		consol.JK_RL_NKDischargePort = localPort;

		var firstInternationalLegTransport = consol.Transports[0];
		firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
		firstInternationalLegTransport.JW_RL_NKDiscPort = "AUSYD";
		firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
		firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
		firstInternationalLegTransport.JW_LegOrder = 1;

		var secondInternationalLegTransport = consol.Transports.AddNew();
		secondInternationalLegTransport.JW_RL_NKLoadPort = "AUSYD";
		secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
		secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
		secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
		secondInternationalLegTransport.JW_LegOrder = 2;

		var shipment = consol.Shipments.AddNew();
		declaration.JE_JS = shipment.PK;
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

		declaration.JE_RL_NKPortOfLoading = "";
		declaration.JE_RL_NKPortOfArrival = "";

		decSynchroniser = new JobDeclarationSynchroniser(declaration);
		decSynchroniser.Synchronise(true);

		AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "AUSYD", declaration.JE_RL_NKPortOfLoading);
		AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
		AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday.AddDays(7), declaration.JE_ExportDate);
		AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(8), declaration.JE_DateOfArrival);
	}

	protected override void SetupForDetection(ForwardingShipment shipment, ZString origin, ZString destination)
	{
		base.SetupForDetection(shipment, origin, destination);
		shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
	}
}
