using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromAgencyShipmentContainer))]
	sealed class FreightWrapperFromAgencyShipmentContainerTest : FreightWrapperTest
	{
		public override void TestContainerLayoutStyle()
		{
			AssertEquals("SingleContainer", Wrapper.ContainerLayoutStyle);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.BookedContainers.AddNew();

			var wrapper = new FreightWrapperFromAgencyShipmentContainer(container, Factory);

			AssertEquals("TrackingBusinessObjectPK", shipment.PK, wrapper.TrackingBusinessObjectPK);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ContainerCount", "1" },
					{ "ContainerLayoutStyle", "SingleContainer" },
					{ "ContainerSummary",  " x 1" },
					{ "HBLContainerMode",  "FCL" },
					{ "JobNumberHeading", "Shipment" },
					{ "MasterBillHeading", "Bill Of Lading" },
					{ "NoCopyBills", "3" },
					{ "NoOriginalBills", "3" },
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
InspectionType : UNK - Unknown - No Security Measures Taken
ServiceLevel : STD - Standard
ShipmentContainerMode : FCL - Full Container Load
ShipmentStatus : BKD - Booking Confirmed
ShipmentTransportMode : SEA - Sea Freight
ShippedOnBoardType : SHP - Shipped";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var shipment = Factory.New<AgencyShipment>();
			return shipment.BookedContainers.AddNew();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var container = (AgencyShipmentContainer)GetNewBusinessObjectToWrap();
			return new FreightWrapperFromAgencyShipmentContainer(container, Factory);
		}
	}
}
