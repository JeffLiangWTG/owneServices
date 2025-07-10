using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillSynchroniser))]
sealed class AsycudaBillSynchroniserTest : Customs.Business.Testing.ManifestBillSynchroniserTest
{
	public void TestCargoType() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusMaps.CargoTypes, "OUT", "Test", true);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusMaps.CargoTypes, "FCL", "9", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusMaps.CargoTypes, "LCL", "9", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusMaps.CargoTypes, "LQD", "13", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil);
		Factory.Save();

		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_OuterPacks = 10;
		shipment.JS_F3_NKPackType = "UT";

		var consol = shipment.Consols.AddNew();
		consol.JK_ConsolMode = "FCL";

		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		manifestHeader.SetParent(consol);
		var manifestBill = manifestHeader.Bills.AddNew();
		manifestHeader.Synchroniser.SetEnabled(true, false);
		manifestHeader.Synchroniser.Synchronise();

		AssertEquals("9", manifestBill.ABL_CargoType);

		consol.JK_ConsolMode = "LQD";
		AssertEquals("13", manifestBill.ABL_CargoType);

		consol.JK_ConsolMode = "LCL";
		AssertEquals("9", manifestBill.ABL_CargoType);
	});

	public void TestABL_OA_ContainerAgent()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_OuterPacks = 10;
		shipment.JS_F3_NKPackType = "UT";

		var consol = shipment.Consols.AddNew();
		consol.JK_ConsolMode = "FCL";

		var carrier1 = Factory.New<OrgAddress>();
		consol.JK_OA_SendingForwarderAddress = carrier1.PK;

		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.SetParent(consol);
		var manifestBill = manifestHeader.Bills.AddNew();
		manifestHeader.Synchroniser.SetEnabled(true, false);
		manifestHeader.Synchroniser.Synchronise();

		AssertEquals("Origin Agent should be defaulted to Sending Forwarder With Contact", manifestBill.ABL_OA_ContainerAgent, consol.JK_OA_SendingForwarderAddress);
	}

	public void TestBillType() => CombineAssertions(() =>
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
		shipment.JS_OuterPacks = 10;
		shipment.JS_F3_NKPackType = "UT";
		var consol = shipment.Consols.AddNew();
		consol.JK_AgentType = "CLD";

		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		manifestHeader.SetParent(consol);
		manifestHeader.Synchroniser.SetEnabled(true, false);
		manifestHeader.Synchroniser.Synchronise();
		AssertEquals("Precondition", 1, manifestHeader.Bills.Count);

		var manifestBill = manifestHeader.Bills[0];
		AssertEquals("CLD", manifestBill.ABL_BolType);
		consol.JK_AgentType = "DRT";
		AssertEquals("STD", manifestBill.ABL_BolType);
		consol.JK_AgentType = "AGT";
		AssertEquals("STD", manifestBill.ABL_BolType);
	});

	public void TestContainerModeSynchronisation()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

		var consol = shipment.Consols.AddNew();
		var header = Factory.New<AsycudaManifestHeader>();
		header.SetParent(consol);
		var bill = header.Bills.AddNew();
		header.Synchroniser.SetEnabled(true, false);
		header.Synchroniser.Synchronise();

		shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
		AssertEquals("3", bill.ABL_SpecialCargoCode);

		shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		AssertEquals("2", bill.ABL_SpecialCargoCode);

		shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		AssertEquals("3", bill.ABL_SpecialCargoCode);

		shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
		AssertEquals(ZString.Empty, bill.ABL_SpecialCargoCode);
	}

	protected override ZString GetLastForeignPort(ForwardingShipment shipment)
	{
		return null;
	}

	protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
	{
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_ParentId = consol.PK;
		header.AMA_ParentTableCode = "JK";
		var bill = header.Bills.AddNew();
		return bill;
	}

	protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
	{
		return new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);
	}

	protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment)
	{
		return shipment.JS_RL_NKOrigin;
	}

	protected override ZString GetPortOfLading(ForwardingShipment shipment)
	{
		return shipment.JS_RL_NKDestination;
	}
}
