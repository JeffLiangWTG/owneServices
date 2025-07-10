using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class TallyCustomsListenerTest : TestCaseWithFactory
	{
		public void TestIsSynchronisedWithTallyContainner()
		{
			var container = Factory.New<TallyContainer>();
			var shipment1 = container.PackUnpackShipments.AddNew();
			var shipment2 = container.PackUnpackShipments.AddNew();

			var header = Factory.New<TallyOutturnHeader>();
			var outturn1 = header.Outturns.AddNew();
			outturn1.C5_ParentID = container.PK;
			var outturn2 = header.Outturns.AddNew();
			outturn2.C5_ParentID = shipment1.PK;
			var outturn3 = header.Outturns.AddNew();
			outturn3.C5_ParentID = shipment2.PK;

			var listener = new TallyCustomsListener(outturn1);
			Assert(outturn1.IsSynchronisedWithTallyContainer);
			listener.GetOutturnFor(shipment1);
			Assert(outturn2.IsSynchronisedWithPackUnpachShipment);
		}

		public void TestGetOutturnForWithMultipleOutturns()
		{
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			TallyOutturn shipmentOutturn1 = header.Outturns.AddNew();
			shipmentOutturn1.C5_OuterPacks = 1;
			TallyOutturn shipmentOutturn2 = header.Outturns.AddNew();
			shipmentOutturn2.C5_OuterPacks = 2;
			shipmentOutturn2.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;

			TallyOutturn containerOutturn = header.Outturns.AddNew();

			PackUnpackShipment shipment = Container.PackUnpackShipments.AddNew();
			CFSShipmentWrapper shipmentWrapper = CFSShipmentWrapper.Load(shipment);
			shipmentWrapper.Outturns.Add(shipmentOutturn1);
			shipmentWrapper.Outturns.Add(shipmentOutturn2);

			CFSTallyContainerWrapper containerWrapper = CFSTallyContainerWrapper.Load(container);
			containerWrapper.Outturns.Add(containerOutturn);

			TallyCustomsListener link = new TallyCustomsListener(containerOutturn);
			IOutturn linkOutturn = link.GetOutturnFor(shipment);
			AssertNotNull("An outturn is returned", linkOutturn);
			AssertEquals("Is 2nd outturn", 2, linkOutturn.NumberOfPackages);
		}

		public void TestLinkedTally()
		{
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			TallyOutturn shipmentOutturn = header.Outturns.AddNew();
			TallyOutturn containerOutturn = header.Outturns.AddNew();

			PackUnpackShipment shipment = Container.PackUnpackShipments.AddNew();
			CFSShipmentWrapper shipmentWrapper = CFSShipmentWrapper.Load(shipment);
			shipmentWrapper.Outturns.Add(shipmentOutturn);

			CFSTallyContainerWrapper containerWrapper = CFSTallyContainerWrapper.Load(container);
			containerWrapper.Outturns.Add(containerOutturn);

			TallyCustomsListener link = new TallyCustomsListener(containerOutturn);
			IOutturn linkOutturn = link.GetOutturnFor(shipment);
			AssertNotNull(linkOutturn);

			shipmentOutturn.C5_OuterPacks = 0;
			AssertEquals(shipmentOutturn.C5_OuterPacks, linkOutturn.NumberOfPackages);
			shipmentOutturn.C5_OuterPacks = 7;
			AssertEquals(shipmentOutturn.C5_OuterPacks, linkOutturn.NumberOfPackages);
		}

		public void TestOutturnsHeaderIsRegisteredEditable()
		{
			CFSTallyContainerWrapper containerWrapper = CFSTallyContainerWrapper.Load(Container);
			TallyOutturn containerOutturn = containerWrapper.Outturns.AddNew();
			PackUnpackShipment shipment = Container.PackUnpackShipments.AddNew();
			CFSShipmentWrapper wrapper = CFSShipmentWrapper.Load(shipment);
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn = header.Outturns.AddNew();
			wrapper.Outturns.Add(outturn);
			AssertNotNull("precondition", outturn.Header);
			TallyCustomsListener link = new TallyCustomsListener(containerOutturn);
			AssertEquals(false, shipment.IsRegisteredEditableChildObject(outturn.Header));
			IOutturn linkOutturn = link.GetOutturnFor(shipment);
			AssertNotNull("precondition", linkOutturn);
			Container.IsRegisteredEditableChildObject(outturn.Header);
		}

		public void TestReceiptDate()
		{
			CFSTallyContainerWrapper wrapper = CFSTallyContainerWrapper.Load(Container);
			TallyOutturn outturn = wrapper.Outturns.AddNew();
			TallyCustomsListener link = new TallyCustomsListener(outturn);

			AssertEquals("precondition", ZDateTime.Empty, outturn.C5_CargoReceiptDate);

			link.ReceiptDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, outturn.C5_CargoReceiptDate);
			link.ReceiptDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, outturn.C5_CargoReceiptDate);
		}

		public void TestSealNumber()
		{
			CFSTallyContainerWrapper wrapper = CFSTallyContainerWrapper.Load(Container);
			TallyOutturn outturn = wrapper.Outturns.AddNew();
			TallyCustomsListener link = new TallyCustomsListener(outturn);

			AssertEquals("precondition", ZString.Empty, outturn.C5_ContainerSeal);

			link.SetSealNumber("foo");
			AssertEquals("foo", outturn.C5_ContainerSeal);
			link.SetSealNumber("bar");
			AssertEquals("bar", outturn.C5_ContainerSeal);
			link.SetSealNumber(ZString.Empty);
			AssertEquals(ZString.Empty, outturn.C5_ContainerSeal);
		}

		public void TestSealIntact()
		{
			CFSTallyContainerWrapper wrapper = CFSTallyContainerWrapper.Load(Container);
			TallyOutturn outturn = wrapper.Outturns.AddNew();
			TallyCustomsListener link = new TallyCustomsListener(outturn);

			AssertEquals("precondition", false, outturn.C5_SealIntactIndicator);

			link.SetSealIntact(true);
			AssertEquals(true, outturn.C5_SealIntactIndicator);
			link.SetSealIntact(false);
			AssertEquals(false, outturn.C5_SealIntactIndicator);
		}

		#region Implementation

		TallyContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Factory.New<TallyContainer>();
				}
				return container;
			}
		}
		TallyContainer container;

		#endregion
	}
}
