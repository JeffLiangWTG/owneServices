using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaAdditionalInfoSynchronizer))]
	public class AsycudaAdditionalInfoSynchronizerTest : SynchroniserTestCase
	{
		public void TestAsycudaAdditionalInfoSynchronizer()
		{
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.LTL, "PX", "4");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.LTL, "NE", "5");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.LTL, "PLT", "2");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.LCL, "PX", "4");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.LCL, "NE", "5");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.LCL, "PLT", "2");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Other, "PX", "4");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Other, "NE", "5");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Other, "PLT", "2");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FCL, "PX", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FCL, "NE", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FCL, "PLT", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FTL, "PX", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FTL, "NE", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FTL, "PLT", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Groupage, "PX", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Groupage, "NE", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Groupage, "PLT", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.BuyersConsol, "PX", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.BuyersConsol, "NE", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.BuyersConsol, "PLT", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.ShippersConsol, "PX", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.ShippersConsol, "NE", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.ShippersConsol, "PLT", "3");
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Combination, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.NonContainerised, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Containerised, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Liquid, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Unaccompanied, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.AgentConsol, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.AIR, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.All, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.BreakBulk, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Bulk, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Empty, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FCLMixedShipper, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.FreightAllKind, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Loose, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.Mail, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.OnBoardCourier, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.RollOnRollOff, "PLT", string.Empty);
			AssertPackageFeeTypeSynchronization(Core.Constants.ContainerModes.ULD, "PLT", string.Empty);
		}

		void AssertPackageFeeTypeSynchronization(string consolMode, string packType, string expectedContent)
		{
			var factory = Factory;
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_F3_NKPackType = packType;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_ConsolMode = consolMode;

			factory.Save();
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];

			AssertEquals("There should be 1 additional info", 1, manifestBill.AdditionalInfos.Count(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType));

			var additionalInfo = manifestBill.AdditionalInfos.Cast<AsycudaAdditionalInfo>().First(s => s.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType);
			AssertEquals("Additional info content should be correct", expectedContent, additionalInfo.CSI_Description);
		}
	}
}
