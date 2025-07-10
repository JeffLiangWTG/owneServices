using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class TallyOutturn_Test : TestCaseWithFactory
	{
		public void TestParentLoaders()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			PackUnpackShipment shipment = container.PackUnpackShipments.AddNew();
			TallyOutturn outturn = Factory.New<TallyOutturn>();
			{
				CFSTallyContainerWrapper wrapper = (CFSTallyContainerWrapper)outturn.ParentLoadersInternal.LoadBusinessObject(Factory, JobContainerSchema.Constants.Prefix, container.PK);
				AssertNotNull(wrapper.Container);
				AssertEquals(container, wrapper.Container);
			}

			{
				CFSShipmentWrapper wrapper = (CFSShipmentWrapper)outturn.ParentLoadersInternal.LoadBusinessObject(Factory, JobShipmentSchema.Constants.Prefix, shipment.PK);
				AssertNotNull(wrapper);
				AssertEquals(shipment, wrapper.Shipment);
			}
		}

		public void TestContainerDoesNotBlowUpWhenLinkedToShipment()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.Containers.Add(container);
			PackUnpackShipment shipment = container.PackUnpackShipments.AddNew();

			TallyOutturn outturn = Factory.New<TallyOutturn>();
			outturn.Parent = CFSShipmentWrapper.Load(shipment);
			AssertNull(outturn.Container);
			AssertNotNull(outturn.Shipment);
			AssertEquals(shipment.JS_UniqueConsignRef, outturn.ShipmentOrContainerNumber);
		}
	}
}
