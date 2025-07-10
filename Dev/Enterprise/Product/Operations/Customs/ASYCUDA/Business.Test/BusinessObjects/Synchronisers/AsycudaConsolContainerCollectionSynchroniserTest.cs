using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[TestedType(typeof(AsycudaConsolContainerCollectionSynchroniser))]
	sealed class AsycudaConsolContainerCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestAsycudaConsolContainerCollectionSynchroniser()
		{
			var departurePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var repackingPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, departurePort.RL_RN_NKCountryCode }));
			var destinationPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			string containerNumber1 = "CNHK1234567";
			string containerNumber2 = "CRXU1234569";
			string containerNumber3 = "CNHK1234569";

			var containerType = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR");

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_RL_NKOrigin = departurePort.Code;
			shipment1.JS_RL_NKDestination = destinationPort.Code;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_RL_NKOrigin = departurePort.Code;
			shipment2.JS_RL_NKDestination = destinationPort.Code;

			var consol = shipment1.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = departurePort.Code;
			consol.JK_RL_NKDischargePort = repackingPort.Code;
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Precondition", 0, manifestHeader.Containers.Count);
			var container1 = consol.Containers.AddNew();
			container1.JC_RC = containerType.PK;
			container1.JC_ContainerNum = containerNumber1;
			AssertEquals("No Containers Sychronized", 0, manifestHeader.Containers.Count);

			var packLine1 = shipment1.OuterPackLines.AddNew();
			container1.PackLines.Add(packLine1);
			AssertEquals("1 Container Sychronized", 1, manifestHeader.Containers.Count);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = containerType.PK;
			container2.JC_ContainerNum = containerNumber2;
			var packLine2 = shipment1.OuterPackLines.AddNew();
			container2.PackLines.Add(packLine2);
			AssertEquals("2 Containers Sychronized", 2, manifestHeader.Containers.Count);

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = containerType.PK;
			container3.JC_ContainerNum = containerNumber3;
			var packLine3 = shipment2.OuterPackLines.AddNew();
			container3.PackLines.Add(packLine3);
			AssertEquals("3 Containers Sychronized", 3, manifestHeader.Containers.Count);

			var packLine4 = shipment2.OuterPackLines.AddNew();
			container2.PackLines.Add(packLine4);
			AssertEquals("Duplicate Containers should NOT be Sychronized", 3, manifestHeader.Containers.Count);
		}
	}
}
