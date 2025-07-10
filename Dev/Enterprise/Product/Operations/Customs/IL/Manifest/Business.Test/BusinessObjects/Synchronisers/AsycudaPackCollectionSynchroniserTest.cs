using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollectionSynchroniser))]
	sealed class AsycudaPackCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestOneTimeAsycudaPackCollectionSynchroniser()
		{
			const string containerNumber1 = "CNHK1234567";
			var factory = Factory;
			var undgDG1 = ZGuid.NewZGuid();
			var dgContact1 = factory.NewWithValidTestData<OrgContact>();
			var departurePort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var destinationPort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var containerType = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR");

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = departurePort.Code;
			shipment.JS_RL_NKDestination = destinationPort.Code;

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = departurePort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;
			factory.Save();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_HarmonisedCode = "00001623";
			packLine1.JL_ActualWeight = 100;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_Description = "PackLine1 Description";

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = containerType.PK;
			container1.JC_ContainerNum = containerNumber1;
			container1.PackLines.Add(packLine1);

			var undg1 = packLine1.UNDGs.AddNew();
			undg1.DI_DG = undgDG1;
			undg1.DI_IMOClass = "1";
			undg1.DI_DGFlashPoint = 100m;
			undg1.DI_TechnicalName = "Name1";
			undg1.DI_OC_DGContact = dgContact1.PK;

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			AssertEquals("1 Pack Sychronized", containerNumber1, manifestBill.Packs[0].Container.ACN_ContainerNumber);
			AssertEquals("1 Packed Item Sychronized", 1, manifestBill.PackedItems.Count);
			AssertEquals("1 Pack Sychronized", 1, manifestBill.Packs.Count);
			var asycudaPackedItem = manifestBill.PackedItems[0];
			AssertPackedItem(packLine1, asycudaPackedItem);
			AssertEquals("1 UNDG Sychronized", 1, asycudaPackedItem.UNDGs.Count);
			AssertUNDG("Sync of the First Packed Item UNDG", asycudaPackedItem.UNDGs[0], undgDG1, "1", 100m, "Name1", dgContact1.PK);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine1.JL_HarmonisedCode = "00001624";
			packLine1.JL_ActualWeight = 101;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_Description = "PackLine2 Description";
			var undg2 = packLine1.UNDGs.AddNew();
			undg2.DI_DG = undgDG1;
			undg2.DI_IMOClass = "2";
			undg2.DI_DGFlashPoint = 200m;
			undg2.DI_TechnicalName = "Name2";
			undg2.DI_OC_DGContact = dgContact1.PK;

			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("When PackedItems.Cout>0, Synchronization stoped", 1, manifestBill.PackedItems.Count);
			AssertEquals("When PackedItems.Cout>0, Synchronization stoped", 1, manifestBill.PackedItems.Count);
		}

		static void AssertPackedItem(ForwardingPackLine packLine, AsycudaPackedItem packedItem)
		{
			AssertEquals("Should sync the HarmonisedCode.", packLine.JL_HarmonisedCode, packedItem.API_Tariff);
			AssertEquals("Should sync the ActualWeight.", packLine.JL_ActualWeight, packedItem.API_GrossWeight);
			AssertEquals("Should sync the ActualWeightUQ.", packLine.JL_ActualWeightUQ, packedItem.API_GrossWeightUQ);
			AssertEquals("Should sync the Description.", packLine.JL_Description, packedItem.API_GoodsDescription);

			Assert("API_FormattedTariff should not be readonly", !packedItem.API_FormattedTariffInfo.ReadOnly);
			Assert("API_GrossWeight should not be readonly", !packedItem.API_GrossWeightInfo.ReadOnly);
			Assert("API_GrossWeightUQ should not be readonly", !packedItem.API_GrossWeightUQInfo.ReadOnly);
			Assert("API_GoodsDescription should not be readonly", !packedItem.API_GoodsDescriptionInfo.ReadOnly);

			AssertEquals("Number of linked Packs is 1", 1, packedItem.AsycudaLinkPackages.Count(s => s.IsLinked));
			var linkedPack = packedItem.AsycudaLinkPackages.Single(s => s.IsLinked).Package;
			AssertEquals("The Description of the linked pack is the same.", packLine.JL_Description, linkedPack.APA_GoodsDescription);
		}

		static void AssertUNDG(string caption, UNDGDataItem undg, ZGuid dg, string imoClass, decimal flashPoint, string technicalName, ZGuid contactPK)
		{
			CombineAssertions(caption, () =>
			{
				AssertEquals("DI_DG", dg, undg.DI_DG);
				AssertEquals("DI_IMOClass", imoClass, undg.DI_IMOClass);
				AssertEquals("DI_DGFlashPoint", flashPoint, undg.DI_DGFlashPoint);
				AssertEquals("DI_TechnicalName", technicalName, undg.DI_TechnicalName);
				AssertEquals("DI_OC_DGContact", contactPK, undg.DI_OC_DGContact);

				Assert("DI_DG should not be readonly", !undg.DI_DGInfo.ReadOnly);
				Assert("DI_IMOClass should not be readonly", !undg.DI_IMOClassInfo.ReadOnly);
				Assert("DI_DGFlashPoint should not be readonly", !undg.DI_DGFlashPointInfo.ReadOnly);
				Assert("DI_OC_DGContact should not be readonly", !undg.DI_OC_DGContactInfo.ReadOnly);
			});
		}
	}
}
