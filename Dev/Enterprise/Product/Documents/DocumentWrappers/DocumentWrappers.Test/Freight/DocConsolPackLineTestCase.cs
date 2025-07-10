using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocConsolPackLineTestCase : BaseFreightTest
	{
		public void TestPackLinesOnConsol_ReturnCorrectContainer()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var thisConsol = shipment.Consols.AddNew();

			var container_1 = thisConsol.Containers.AddNew();
			var container_2 = thisConsol.Containers.AddNew();

			container_1.JC_SealNum = "Cont_1";
			container_2.JC_SealNum = "Cont_2";
			container_1.JC_ContainerNum = "1";
			container_2.JC_ContainerNum = "2";

			DocForwardingConsol thisConsolWrapper = DocForwardingConsol.New(thisConsol, Factory);
			AssertEquals(0, thisConsolWrapper.PackLinesOnConsol(false, false, null, true).Count);
			AssertEquals(0, thisConsolWrapper.PackLinesOnConsol(false, true, null, true).Count);

			var thisLine1 = shipment.OuterPackLines.AddNew();
			var thisLine2 = shipment.OuterPackLines.AddNew();

			thisLine1.SetContainer(thisConsol, container_1);
			thisLine2.SetContainer(thisConsol, null);

			thisConsolWrapper = DocForwardingConsol.New(thisConsol, Factory);
			AssertEquals("Don't Include Packed, Don't Include Empty - Packlines: 0", 0, thisConsolWrapper.PackLinesOnConsol(false, false, null, true).Count);
			AssertEquals("Don't Include Packed, Include Empty - Packlines: 1", 1, thisConsolWrapper.PackLinesOnConsol(false, true, null, true).Count);
			AssertEquals("Include Packed, Don't Include Empty - Packlines: 1", 1, thisConsolWrapper.PackLinesOnConsol(true, false, null, true).Count);
			AssertEquals("Include Packed, Include Empty - Packlines: 2", 2, thisConsolWrapper.PackLinesOnConsol(true, true, null, true).Count);

			AssertEquals("Don't Include Packed, Include Empty - null Container returned", "", thisConsolWrapper.PackLinesOnConsol(false, true, null, true)[0].SealNumber);

			AssertEquals("Include Packed, Don't Include Empty - Cont_1 returned", "Cont_1", thisConsolWrapper.PackLinesOnConsol(true, false, null, true)[0].SealNumber);

			AssertEquals("Include Packed, Include Empty - Cont1 returned", "Cont_1", thisConsolWrapper.PackLinesOnConsol(true, true, null, true)[0].SealNumber);
			AssertEquals("Include Packed, Include Empty - null Container returned", "", thisConsolWrapper.PackLinesOnConsol(true, true, null, true)[1].SealNumber);
		}

		public void TestPackLinesOnConsol_ReturnCorrectConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var thisConsol = shipment.Consols.AddNew();
			var otherConsol = shipment.Consols.AddNew();

			Enterprise.Freight.Business.CommonContainer container_This = thisConsol.Containers.AddNew();
			Enterprise.Freight.Business.CommonContainer container_Other = otherConsol.Containers.AddNew();

			container_This.JC_ContainerNum = "This";
			container_Other.JC_ContainerNum = "Other";

			var thisLine1 = shipment.OuterPackLines.AddNew();
			thisLine1.SetContainer(thisConsol, container_This);
			thisLine1.SetContainer(otherConsol, container_Other);

			DocForwardingConsol thisConsolWrapper = DocForwardingConsol.New(thisConsol, Factory);
			var packlineReturned = thisConsolWrapper.PackLinesOnConsol(true, true, null, true)[0];
			ForwardingConsol consolReturned = (ForwardingConsol)packlineReturned.Container.Consol.WrappedObject;
			AssertEquals("Should return ThisConsol", thisConsol.PK, consolReturned.PK);

			DocForwardingConsol otherConsolWrapper = DocForwardingConsol.New(otherConsol, Factory);
			packlineReturned = otherConsolWrapper.PackLinesOnConsol(true, true, null, true)[0];
			consolReturned = (ForwardingConsol)packlineReturned.Container.Consol.WrappedObject;
			AssertEquals("Should return OtherConsol", otherConsol.PK, consolReturned.PK);
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}
	}
}
