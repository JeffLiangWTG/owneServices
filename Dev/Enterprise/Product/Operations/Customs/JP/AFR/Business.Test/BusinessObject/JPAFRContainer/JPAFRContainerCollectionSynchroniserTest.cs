using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRContainerCollectionSynchroniserTest : Customs.Business.Testing.SynchroniserTestCase
	{
		public void TestSynchronisation()
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "10.01 01.10 A";
			packLine1.SetContainer(consol, container1);
			packLine1.SetContainer(consol2, container2);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "20.02 02.20 B";
			packLine2.SetContainer(consol, container1);
			packLine2.SetContainer(consol2, null);

			var bill = header.Bills.AddNew();
			AssertEquals(0, bill.Containers.Count);

			var synchroniser = new JPAFRContainerCollectionSynchroniser(shipment, bill, consol);
			synchroniser.Synchronise();
			AssertEquals(1, bill.Containers.Count);
			var billContainer1 = bill.Containers["CONT1"];
			AssertNotNull(billContainer1);

			Factory.Save();
			// changes to packing details should be done on a different factory as packing details is not exposed on the Consol Screen.

			var newFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var consolInOtherFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var consol2InOtherFactory = newFactory.Load<ForwardingConsol>(consol2.PK);
			var packLine2InOtherFactory = newFactory.Load<PackLine>(packLine2.PK);
			packLine2InOtherFactory.SetContainer(consolInOtherFactory, null);

			newFactory.Save();
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(billContainer1, bill.Containers["CONT1"]);

			var packLine3InOtherFactory = shipmentInOtherFactory.OuterPackLines.AddNew();
			packLine3InOtherFactory.JL_HarmonisedCode = "60.605.30";
			packLine3InOtherFactory.SetContainer(consolInOtherFactory, null);
			packLine3InOtherFactory.SetContainer(consol2InOtherFactory, null);

			newFactory.Save();
			AssertEquals(3, shipment.OuterPackLines.Count);
			var packLine3 = (PackLine)shipment.OuterPackLines.FindByPK(packLine3InOtherFactory.PK);
			packLine3.SetContainer(consol, null);
			packLine3.SetContainer(consol2, null);
			Factory.Save();

			AssertEquals(ZGuid.Empty, packLine3.JL_JC);
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(billContainer1, bill.Containers["CONT1"]);

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONT3";
			Factory.Save();

			AssertEquals(1, bill.Containers.Count);
			AssertEquals(billContainer1, bill.Containers["CONT1"]);

			var container3InOtherFactory = newFactory.Load<ForwardingContainer>(container3.PK);
			packLine3InOtherFactory.SetContainer(consolInOtherFactory, container3InOtherFactory);
			newFactory.Save();

			AssertEquals(2, consol.Containers.Count);
			AssertEquals(2, bill.Containers.Count);
			AssertEquals(billContainer1, bill.Containers["CONT1"]);
			var billContainer2 = bill.Containers["CONT3"];
			AssertNotNull(billContainer2);

			container1.Delete();
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(true, billContainer1.IsDeleted);
			AssertEquals(billContainer2, bill.Containers["CONT3"]);
		}

		public void TestSynchronisation_DuplicatedContainers()
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "1001";
			packLine1.SetContainer(consol, container1);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "1002";
			packLine2.SetContainer(consol, container2);

			var bill = header.Bills.AddNew();
			AssertEquals(0, bill.Containers.Count);

			var synchroniser = new JPAFRContainerCollectionSynchroniser(shipment, bill, consol);
			synchroniser.Synchronise();
			AssertEquals(2, bill.Containers.Count);
			var billContainer1 = bill.Containers[0];
			var billContainer2 = bill.Containers[1];
			AssertEquals("CONT1", billContainer1.JPC_ContainerNum);
			AssertEquals("CONT1", billContainer2.JPC_ContainerNum);

			container2.JC_ContainerNum = "CONT2";
			AssertEquals(2, bill.Containers.Count);
			AssertEquals("CONT1", billContainer1.JPC_ContainerNum);
			AssertEquals("CONT2", billContainer2.JPC_ContainerNum);

			container2.JC_ContainerNum = "CONT1";
			AssertEquals(2, bill.Containers.Count);
			AssertEquals("CONT1", billContainer1.JPC_ContainerNum);
			AssertEquals("CONT1", billContainer2.JPC_ContainerNum);

			synchroniser.SetEnabled(false, false);
			container1.Delete();

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONT1";
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "1003";
			packLine3.SetContainer(consol, container3);

			synchroniser.SetEnabled(true, false);
			AssertEquals(2, bill.Containers.Count);
			AssertEquals("CONT1", billContainer1.JPC_ContainerNum);
			AssertEquals("CONT1", billContainer2.JPC_ContainerNum);

			synchroniser.Synchronise(true);
			AssertEquals("Is deleted as container1 is deleted", true, billContainer1.IsDeleted);
			var list = new List<JPAFRContainer>(bill.Containers);
			AssertEquals(2, list.Count);
			AssertEquals(billContainer2, list.First(x => x.PK == billContainer2.PK));
			var billContainer3 = list.First(x => x.PK != billContainer2.PK);
			AssertEquals("CONT1", billContainer2.JPC_ContainerNum);
			AssertEquals("CONT1", billContainer3.JPC_ContainerNum);

			synchroniser.SetEnabled(false, false);
			packLine1.Delete();

			synchroniser.SetEnabled(true, false);
			AssertEquals(2, bill.Containers.Count);
			AssertEquals("CONT1", billContainer2.JPC_ContainerNum);
			AssertEquals("CONT1", billContainer3.JPC_ContainerNum);

			synchroniser.Synchronise(true);
			list = new List<JPAFRContainer>(bill.Containers);
			AssertEquals(2, list.Count);
			AssertEquals(billContainer2, list.First(x => x.PK == billContainer2.PK));
			AssertEquals(billContainer3, list.First(x => x.PK == billContainer3.PK));
			AssertEquals("CONT1", billContainer2.JPC_ContainerNum);
			AssertEquals("CONT1", billContainer3.JPC_ContainerNum);
		}

		public void TestSynchronisationViaDataRefreshBus()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "ABCDHB1";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_HarmonisedCode = "1010.10.10";
			var bill = header.Bills.AddNew("ABCDHB1");
			var synchroniser = new JPAFRContainerCollectionSynchroniser(shipment, bill, consol);
			synchroniser.Synchronise(true);
#pragma warning disable
			((IBusinessObjectState)consol).UpdatedByDataRefreshIncludingChildren += new EventHandler((x, y) => synchroniser.Synchronise());
#pragma warning restore
			AssertEquals(1, bill.Containers.Count);
			var billContainer = bill.Containers[0];
			AssertEquals("CONT1", billContainer.JPC_ContainerNum);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var containerInOtherFactory = newFactory.Load<ForwardingContainer>(container.PK);
			AssertEquals("CONT1", containerInOtherFactory.JC_ContainerNum);
			containerInOtherFactory.JC_ContainerNum = "CONT2";
			newFactory.Save();
			AssertEquals("CONT2", container.JC_ContainerNum);
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(false, billContainer.IsDeleted);
			AssertEquals(billContainer, bill.Containers["CONT2"]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
		}
		ForwardingConsol consol;
		JPAFRHeader header;
	}
}
