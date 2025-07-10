using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestManifestBillTypeHWB()
		{
			var factory = Factory;
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			factory.Save();
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			AssertEquals(AsycudaBillKindList.Codes.HWB, manifestBill.ABL_BolType);
			factory.Save();

			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("save button should not be active", false, manifestBill.HasChanges);
		}

		public void TestSynchroniseConditionWithHBLContainerPackModeOverride()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			shipment.JS_HBLContainerPackModeOverride = "CFS/CFS";
			AssertEquals("CFS/CFS", "30", manifestBill.ABL_Condition);

			manifestBill.ABL_Condition = "";
			shipment.JS_HBLContainerPackModeOverride = "CY/CY";
			AssertEquals("CY/CY", "30", manifestBill.ABL_Condition);

			manifestBill.ABL_Condition = "";
			shipment.JS_HBLContainerPackModeOverride = "CFS/CY";
			AssertEquals("CFS/CFS", "30", manifestBill.ABL_Condition);

			manifestBill.ABL_Condition = "";
			shipment.JS_HBLContainerPackModeOverride = "CY/CFS";
			AssertEquals("CY/CY", "30", manifestBill.ABL_Condition);

			shipment.JS_HBLContainerPackModeOverride = "CFS/DOOR";
			AssertEquals("CFS/DOOR", "29", manifestBill.ABL_Condition);

			manifestBill.ABL_Condition = "";
			shipment.JS_HBLContainerPackModeOverride = "CY/DOOR";
			AssertEquals("CY/DOOR", "29", manifestBill.ABL_Condition);

			shipment.JS_HBLContainerPackModeOverride = "DOOR/CFS";
			AssertEquals("DOOR/CFS", "28", manifestBill.ABL_Condition);

			manifestBill.ABL_Condition = "";
			shipment.JS_HBLContainerPackModeOverride = "DOOR/CY";
			AssertEquals("DOOR/CY", "28", manifestBill.ABL_Condition);

			shipment.JS_HBLContainerPackModeOverride = "DOOR/DOOR";
			AssertEquals("DOOR/DOOR", "27", manifestBill.ABL_Condition);
		}

		public void TestGetAsycudaPackCollectionSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var bill = (AsycudaBill)GetManifestBill(consol);

			AssertType<AsycudaPackCollectionSynchroniser>("AsycudaBill should use expected synchronizer", new AsycudaBillSynchroniserForTest(bill, shipment).GetAsycudaPackCollectionSynchroniserExposed());
		}

		public void TestSynchronizePackageFeeType()
		{
			var factory = Factory;
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			factory.Save();
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];

			AssertEquals("There should be 1 additional info", 1, manifestBill.AdditionalInfos.Count(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType));
			var additionalInfo = manifestBill.AdditionalInfos.First(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType);
			AssertEquals("CSI_Description should have NoPacks value", "2", additionalInfo.CSI_Description);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("There should be 0 additional infos", 0, manifestBill.AdditionalInfos.Count(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType));

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("There should be back 1 additional info", 1, manifestBill.AdditionalInfos.Count(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType));
			additionalInfo = manifestBill.AdditionalInfos.First(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType);
			AssertEquals("CSI_Description should have NoPacks value", "2", additionalInfo.CSI_Description);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("CSI_Description should have Containerized value", "3", additionalInfo.CSI_Description);
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = Consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}

	public class AsycudaBillSynchroniserForTest : AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniserForTest(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment shipmentSource) : base(destination, shipmentSource)
		{
		}

		public ASYCUDA.Business.AsycudaPackCollectionSynchroniser GetAsycudaPackCollectionSynchroniserExposed()
		{
			return base.GetAsycudaPackCollectionSynchroniser();
		}
	}
}
