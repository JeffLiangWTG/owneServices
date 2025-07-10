using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[TestedType(typeof(AsycudaPackSynchroniser))]
	sealed class AsycudaPackSynchroniserTest : SynchroniserTestCase
	{
		public void TestAsycudaPackSynchroniserSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 3;
			line1.JL_F3_NKPackType = "UT";
			line1.JL_ActualVolume = 15;
			line1.JL_ActualVolumeUQ = "M3";

			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 7;
			line2.JL_F3_NKPackType = "UT";
			line2.JL_ActualVolume = 0;
			line2.JL_ActualVolumeUQ = "M3";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "GMP";
			pack1.RP_CustomsCountry = "AU";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";

			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "AU";

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];

			AssertEquals("2 Pack Sychronized", 2, manifestBill.Packs.Count);

			CombineAssertions(() =>
			{
				AssertEquals("APA_Volume", 15m, manifestBill.Packs[0].APA_Volume);
				AssertEquals("APA_VolumeUQ", "M3", manifestBill.Packs[0].APA_VolumeUQ);

				AssertEquals("APA_PackUQ", "UT", manifestBill.Packs[0].APA_PackUQ);
				AssertEquals("APA_PackQty", 3, manifestBill.Packs[0].APA_PackQty);

				Assert("ReadOnly", manifestBill.Packs[0].APA_PackUQInfo.ReadOnly);

				AssertEquals("APA_Volume", 0m, manifestBill.Packs[1].APA_Volume);
				AssertEquals("APA_VolumeUQ", string.Empty, manifestBill.Packs[1].APA_VolumeUQ);

				AssertEquals("APA_PackUQ", "UT", manifestBill.Packs[1].APA_PackUQ);
				AssertEquals("APA_PackQty", 7, manifestBill.Packs[1].APA_PackQty);

				Assert("ReadOnly", manifestBill.Packs[1].APA_PackUQInfo.ReadOnly);
			});

			line1.JL_F3_NKPackType = "DIZ";
			line2.JL_F3_NKPackType = "DIZ";
			manifestHeader.Synchroniser.Synchronise();

			CombineAssertions(() =>
			{
				AssertEquals("APA_PackUQ", "NO", manifestBill.Packs[0].APA_PackUQ);
				AssertEquals("APA_PackQty", 30, manifestBill.Packs[0].APA_PackQty);
				Assert("ReadOnly", manifestBill.Packs[0].APA_PackUQInfo.ReadOnly);

				AssertEquals("APA_PackUQ", "NO", manifestBill.Packs[1].APA_PackUQ);
				AssertEquals("APA_PackQty", 70, manifestBill.Packs[1].APA_PackQty);
				Assert("ReadOnly", manifestBill.Packs[1].APA_PackUQInfo.ReadOnly);
			});
		}
	}
}
