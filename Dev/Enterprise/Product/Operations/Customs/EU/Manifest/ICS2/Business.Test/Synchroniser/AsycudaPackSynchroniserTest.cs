using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaPackSynchroniser))]
	sealed class AsycudaPackSynchroniserTest : SynchroniserTestCase
	{
		public void TestSyncMarksAndNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_MarksAndNumbers = "Marks and Numbers Shipment";

			var consol = shipment.Consols.AddNew();

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_MarksAndNumbers = "Marks and Numbers Line";

			var line2 = shipment.OuterPackLines.AddNew();

			var line3 = shipment.OuterPackLines.AddNew();
			line3.JL_MarksAndNumbers = "  Marks  and          Numbers    with  CR/LF \r\n  and leading/multiple spaces";

			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Latvia;

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			try
			{
				manifestHeader.Synchroniser.Synchronise();
			}
			finally
			{
				shipment.ShipmentJobHeader?.Dispose();
			}

			var manifestBill = manifestHeader.Bills[0];

			CombineAssertions(() =>
			{
				AssertEquals("Marks of Line", "Marks and Numbers Line", manifestBill.Packs[0].APA_MarksAndNumbers);
				AssertEquals("Marks of Shipment", "Marks and Numbers Shipment", manifestBill.Packs[1].APA_MarksAndNumbers);
				AssertEquals("Marks with CR/LF and leading/multiple spaces", "Marks and Numbers with CR/LF and leading/multiple spaces", manifestBill.Packs[2].APA_MarksAndNumbers);
			});
		}
	}
}
