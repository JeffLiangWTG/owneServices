using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBForwardingShipmentCustomsStatusProviderTestCase : TestCaseWithFactory
	{
		public void TestStatuses()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Assert("IsAir needs to return true for this to work", shipment.IsAir);
			CusMAWB mAWB3 = Factory.New<CusMAWB>();
			mAWB3.CM_JK = consol.PK;
			mAWB3.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			CusHAWBForTest hAWB3 = Factory.New<CusHAWBForTest>();
			hAWB3.CS_JS = shipment.PK;
			hAWB3.CS_CM = mAWB3.PK;
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_JS = shipment.PK;
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_JK = consol.PK;
			mAWB2.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			CusHAWBForTest hAWB2 = Factory.New<CusHAWBForTest>();
			hAWB2.CS_JS = shipment.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB.CS_MsgStatus = "WTO";
			hAWB.CS_CustomsStatus = "HLD";
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingShipment shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);

			var customsStatusProvider = Forwarding.Business.ForwardingShipmentCustomsStatusProvider.New(shipmentInFactory2);

			AssertEquals("WTO", customsStatusProvider.CustomsMessageStatus());
			AssertEquals("HLD", customsStatusProvider.CustomsCargoStatus());
		}

		public void TestStatusesWhenNotAir()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Mail;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Assert("IsAir needs to return false for this to work", !shipment.IsAir);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_JS = shipment.PK;
			Factory.Save();
			hAWB.CS_MsgStatus = "WTO";
			hAWB.CS_CustomsStatus = "HLD";

			var customsStatusProvider = Forwarding.Business.ForwardingShipmentCustomsStatusProvider.New(shipment);

			Assert("Should be empty", customsStatusProvider.CustomsMessageStatus().IsEmpty);
			Assert("Should be empty", customsStatusProvider.CustomsCargoStatus().IsEmpty);
		}

		sealed class CusHAWBForTest : CusHAWB
		{
			public CusHAWBForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
