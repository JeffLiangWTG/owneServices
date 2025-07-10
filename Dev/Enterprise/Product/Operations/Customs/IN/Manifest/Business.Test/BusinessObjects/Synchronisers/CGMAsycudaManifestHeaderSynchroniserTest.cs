using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaManifestHeaderSynchroniser))]
sealed class CGMAsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
{
	public void TestPortOfOrigin()
	{
		CombineAssertions(() =>
		{
			var (leg1, leg2) = SetupTransports();
			AssertEquals("BA4CA", ManifestHeader.AMA_RL_NKOrigin);
			leg2.JW_RL_NKLoadPort = "AEDXB";
			AssertEquals("AEDXB", ManifestHeader.AMA_RL_NKOrigin);
		});
	}

	public void TestPortOfDestination()
	{
		CombineAssertions(() =>
		{
			var (leg1, leg2) = SetupTransports();
			AssertEquals("INBLR", ManifestHeader.AMA_RL_NKFinalDestination);
			leg2.JW_RL_NKDiscPort = "INBOM";
			AssertEquals("INBOM", ManifestHeader.AMA_RL_NKFinalDestination);
		});
	}

	public void TestManifestQty()
	{
		CombineAssertions(() =>
		{
			var (shipment1, shipment2) = SetupShipments();
			AssertEquals(15, ManifestHeader.ManifestQty);
			shipment1.JS_OuterPacks = 3;
			AssertEquals(8, ManifestHeader.ManifestQty);
		});
	}

	public void TestGrossWeight()
	{
		CombineAssertions(() =>
		{
			var (shipment1, shipment2) = SetupShipments();
			AssertEquals(58m, ManifestHeader.GrossWeight);
			shipment1.JS_ActualWeight = 18;
			AssertEquals(44m, ManifestHeader.GrossWeight);
		});
	}

	public void TestGrossWeightUQ()
	{
		CombineAssertions(() =>
		{
			var (shipment1, shipment2) = SetupShipments();
			AssertEquals("KG", ManifestHeader.GrossWeightUQ);
			shipment1.JS_UnitOfWeight = "LB";
			AssertEquals("KG", ManifestHeader.GrossWeightUQ);
			shipment2.JS_UnitOfWeight = "OZ";
			AssertEquals("LB", ManifestHeader.GrossWeightUQ);
		});
	}

	public void TestGoodsDescription()
	{
		AssertEquals("Unknow transport mode", ZString.Empty, ManifestHeader.AMA_GoodsDescription);
		CombineAssertions("Air transport mode", () =>
		{
			SourceConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("No Dimensions Available", ManifestHeader.AMA_GoodsDescription);

			var line1 = SourceConsol.AWBHeader.AWBRateLines.AddNew();
			line1.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			line1.NatureAndQtyOfGoodsText.Text = "Books";

			var line2 = SourceConsol.AWBHeader.AWBRateLines.AddNew();
			line2.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			line2.NatureAndQtyOfGoodsText.Text = "Total";

			var line3 = SourceConsol.AWBHeader.AWBRateLines.AddNew();
			line3.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			line3.NatureAndQtyOfGoodsText.Text = "A123";

			AssertEquals("No Dimensions Available Books", ManifestHeader.AMA_GoodsDescription);
		});
	}

	(Transport, Transport) SetupTransports()
	{
		var leg1 = SourceConsol.Transports.AddNew("USAPG", "BA4CA");
		var leg2 = SourceConsol.Transports.AddNew("BA4CA", "INBLR");
		return (leg1, leg2);
	}

	(ForwardingShipment, ForwardingShipment) SetupShipments()
	{
		var shipment1 = SourceConsol.Shipments.AddNew();
		shipment1.JS_OuterPacks = 10;
		shipment1.JS_ActualWeight = 32;
		shipment1.JS_UnitOfWeight = "KG";
		var shipment2 = SourceConsol.Shipments.AddNew();
		shipment2.JS_OuterPacks = 5;
		shipment2.JS_ActualWeight = 26;
		shipment1.JS_UnitOfWeight = "KG";
		return (shipment1, shipment2);
	}

	CGMAsycudaManifestHeader GetManifestHeader()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.SetParent(SourceConsol);
		header.Synchroniser.SetEnabled(true, false);
		header.Synchroniser.Synchronise();
		return header;
	}

	CGMAsycudaManifestHeader ManifestHeader => manifestHeader ??= GetManifestHeader();
	CGMAsycudaManifestHeader manifestHeader;

	ForwardingConsol SourceConsol => sourceConsol ??= Factory.New<ForwardingConsol>();
	ForwardingConsol sourceConsol;
}
