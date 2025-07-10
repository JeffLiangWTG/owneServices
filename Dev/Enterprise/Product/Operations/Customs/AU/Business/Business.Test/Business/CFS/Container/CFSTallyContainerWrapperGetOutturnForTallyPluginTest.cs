using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CFSTallyContainerWrapperGetOutturnForTallyPluginTest : TestCaseWithFactory
	{
		public void TestContainerWithNoConsol()
		{
			consol.Containers.RemoveAll();
			mock.Setup(m => m.ShowNoConsolError());
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.VerifyAll();
		}

		public void TestTooManyMatches()
		{
			TallyOutturnHeader header = GetCorrectHeader();
			GetCorrectOutturn(header);
			GetCorrectOutturn(header);

			mock.Setup(m => m.ShowMultipleMatchesError());
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.VerifyAll();
		}

		public void TestWithShipments()
		{
			PackUnpackShipment shipment1 = container.PackUnpackShipments.AddNew();
			shipment1.JS_HouseBill = "HARBL1";
			CFSShipmentWrapper shipmentWrapper1 = CFSShipmentWrapper.Load(shipment1);
			PackUnpackShipment shipment2 = container.PackUnpackShipments.AddNew();
			shipment2.JS_HouseBill = "HARBL2";
			CFSShipmentWrapper shipmentWrapper2 = CFSShipmentWrapper.Load(shipment2);

			AssertNull(shipmentWrapper1.Outturn);
			AssertNull(shipmentWrapper2.Outturn);

			mock.Setup(m => m.ShouldCreateAndLinkWhenNoMatchesFound()).Returns(false);
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			AssertNull(shipmentWrapper1.Outturn);
			AssertNull(shipmentWrapper2.Outturn);
			mock.Verify();

			mock.Setup(m => m.ShouldCreateAndLinkWhenNoMatchesFound()).Returns(true);
			AssertNotNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			AssertNotNull(shipmentWrapper1.Outturn);
			AssertNotNull(shipmentWrapper2.Outturn);
			mock.Verify();
		}

		public void TestAlreadyLinked()
		{
			TallyOutturn outturn = GetCorrectOutturn(GetCorrectHeader());
			wrapper.Outturns.Add(outturn);
			AssertEquals(outturn, wrapper.GetOutturnForTallyPlugin(mock.Object));
		}

		public void TestNotLinked()
		{
			TallyOutturn outturn = GetCorrectOutturn(GetCorrectHeader());

			mock.Setup(m => m.ShouldLinkWhenOutturnFound()).Returns(false);
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();

			mock.Setup(m => m.ShouldLinkWhenOutturnFound()).Returns(true);
			AssertEquals(outturn, wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();
		}

		public void TestNotLinkedButOnlyHeaderExists()
		{
			TallyOutturnHeader header = GetCorrectHeader();

			mock.Setup(m => m.ShouldCreateAndLinkWhenOnlyHeaderFound()).Returns(false);
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();

			mock.Setup(m => m.ShouldCreateAndLinkWhenOnlyHeaderFound()).Returns(true);
			TallyOutturn foundOutturn = wrapper.GetOutturnForTallyPlugin(mock.Object);
			AssertNotNull(foundOutturn);
			AssertEquals(header, foundOutturn.Header);
			mock.Verify();
		}

		public void TestDoesntExist()
		{
			mock.Setup(m => m.ShouldCreateAndLinkWhenNoMatchesFound()).Returns(false);
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();

			mock.Setup(m => m.ShouldCreateAndLinkWhenNoMatchesFound()).Returns(true);
			AssertNotNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();
		}

		public void TestAlreadyLinkedToSomethingElse()
		{
			TallyOutturn outturn = GetCorrectOutturn(GetCorrectHeader());
			TallyContainer otherContainer = Factory.New<TallyContainer>();
			CFSTallyContainerWrapper.Load(otherContainer).Outturns.Add(outturn);

			mock.Setup(m => m.ShowOutturnAlreadyLinked());
			AssertNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();
		}

		public void TestAlreadyLinkedToSomethingElseWithShipments()
		{
			PackUnpackShipment shipment1 = container.PackUnpackShipments.AddNew();
			shipment1.JS_HouseBill = "SAMEHOUSEBILL";
			CFSShipmentWrapper shipmentWrapper1 = CFSShipmentWrapper.Load(shipment1);

			PackUnpackShipment shipment2 = container.PackUnpackShipments.AddNew();
			shipment2.JS_HouseBill = "SAMEHOUSEBILL";
			CFSShipmentWrapper shipmentWrapper2 = CFSShipmentWrapper.Load(shipment2);

			PackUnpackShipment shipment3 = container.PackUnpackShipments.AddNew();
			shipment3.JS_HouseBill = "SAMEHOUSEBILL";
			CFSShipmentWrapper shipmentWrapper3 = CFSShipmentWrapper.Load(shipment3);

			mock.Setup(m => m.ShouldCreateAndLinkWhenNoMatchesFound()).Returns(true);
			mock.Setup(m => m.ShowShipmentOutturnsAlreadyLinked(2));
			AssertNotNull(wrapper.GetOutturnForTallyPlugin(mock.Object));
			mock.Verify();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9495C";

			container = Factory.New<TallyContainer>();
			wrapper = CFSTallyContainerWrapper.Load(container);
			consol = Factory.New<CFSLoadListConsol>();

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.MainTransport.JW_Vessel = "ADMIRALENGRACHT";
			consol.MainTransport.JW_VoyageFlight = "54321S";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			consol.Containers.Add(container);
			container.JC_ContainerNum = "BBBB2222227";
		}

		CFSTallyContainerWrapper wrapper;
		TallyContainer container;
		CFSLoadListConsol consol;
		readonly Mock<IFindOrCreateOutturnUI> mock = new Mock<IFindOrCreateOutturnUI>();

		#region Test Data Providers

		TallyOutturnHeader GetCorrectHeader()
		{
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			header.C6_VesselName = "ADMIRALENGRACHT";
			header.C6_OutturningPremiseID = "9495C";
			header.C6_VoyageNum = "54321S";
			return header;
		}

		TallyOutturn GetCorrectOutturn(TallyOutturnHeader header)
		{
			TallyOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "BBBB2222227";
			return outturn;
		}

		#endregion

		#endregion
	}
}
