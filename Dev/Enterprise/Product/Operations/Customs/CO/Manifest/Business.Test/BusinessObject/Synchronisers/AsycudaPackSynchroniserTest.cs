using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	sealed class AsycudaPackSynchroniserTest : SynchroniserTestCase
	{
		public void TestPackUQIsNotConverted()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "BT";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "CO";
			pack1.RP_ConversionFactor = 2;
			pack1.RP_CommercialPack = "PCS";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "PCS";

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var bill = manifestHeader.Bills[0];
			var pack = bill.Packs[0];

			CombineAssertions(() =>
			{
				AssertEquals("APA_PackUQ is not converted", 10, pack.APA_PackQty);
				AssertEquals("APA_PackUQ is not converted", "PCS", pack.APA_PackUQ);
			});
		}
	}
}
