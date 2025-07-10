using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SEK.Testing
{
	[TestedType(typeof(SEKDocAWB))]
	public class SEKDocAWBTest : DocumentWrapperTestCase
	{
		SEKDocAWB Doc { get; set; }

		ForwardingConsol Consol { get; set; }

		public void TestTotalHAWBWeightKG()
		{
			Assert(Consol.Shipments.Count == 0);
			AssertEquals("Weight is not set", new ZDecimal(0), Doc.TotalHAWBWeightKG);
			CommonShipment shipment = Consol.Shipments.AddNew();
			PackLine line = shipment.OuterPackLines.AddNew();
			line.JL_ActualWeight = 111;
			AssertEquals("Weight is set", new ZDecimal(111), Doc.TotalHAWBWeightKG);
		}

		public void TestTotalConsolWeight()
		{
			var roadConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			roadConsol.JK_TransportMode = Core.Constants.TransportModes.Road;
			roadConsol.JK_RL_NKDischargePort = "AUBNE";
			roadConsol.JK_RL_NKLoadPort = "AUSYD";
			var shipmentAttachedToRoadConsol = roadConsol.Shipments.AddNew();
			shipmentAttachedToRoadConsol.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipmentAttachedToRoadConsol.JS_ActualWeight = 2m;
			var shipmentAttachedToAWBConsol = roadConsol.Shipments.AddNew();
			shipmentAttachedToAWBConsol.JS_ActualWeight = 0.345m;
			shipmentAttachedToAWBConsol.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipmentAttachedToAWBConsol.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentAttachedToAWBConsol.Consols.Add(Consol);
			shipmentAttachedToAWBConsol = Consol.Shipments.AddNew();
			shipmentAttachedToAWBConsol.JS_ActualWeight = 1m;
			shipmentAttachedToAWBConsol.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AssertEquals("TotalConsolWeight", "1.345 KG", Doc.TotalConsolWeight);
		}

		public void TestAWBsCount()
		{
			AssertEquals("There should be 1 AWB", 1, Doc.AWBsCount);
		}

		#region Implementation
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			ConsolExportAWBHeader header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			Consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_RL_NKDischargePort = "HKHKG";
			Consol.JK_RL_NKLoadPort = "AUSYD";
			header.EH_ParentID = Consol.PK;
			Doc = (SEKDocAWB)SEKDocAWB.New(header, Factory);
			return new DocumentWrapper[] { Doc };
		}
		#endregion
	}
}
