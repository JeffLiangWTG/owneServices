using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DestinationDeletableBusinessObjectSynchroniserTest : TestCaseWithFactory
	{
		public void TestDestinationDeletedEvent()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 20;
			Assert("PRE-Condition Shipment should have at least one packline", shipment.OuterPackLines.Count > 0);

			ISynchroniserDeletableBusinessObject destination = Factory.New<CusSCAHouse>();
			DestinationDeletableBusinessObjectSynchroniser testSynchroniser = new DestinationDeletableBusinessObjectSynchroniser(destination, shipment);
			testSynchroniser.DestinationDeleted += new EventHandler(TestSynchroniser_DestinationDeleted);
			testSynchroniser.SetEnabled(true, false);
			((CusSCAHouse)destination).Delete();
			AssertEquals("Failed to call destination Deleted Event", true, destinationDeletedCalled);
		}

		#region Implementation

		bool destinationDeletedCalled;
		void TestSynchroniser_DestinationDeleted(object sender, EventArgs e)
		{
			destinationDeletedCalled = true;
		}

		#endregion
	}
}
