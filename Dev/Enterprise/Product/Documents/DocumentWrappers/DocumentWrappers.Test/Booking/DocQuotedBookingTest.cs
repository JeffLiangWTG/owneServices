using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocQuotedBooking))]
	sealed class DocQuotedBookingTest : DocumentWrapperTestCase
	{
		public void TestContainerCount()
		{
			ForwardingShipment b = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking shipment = QuotedBooking.New(ZGuid.Empty, b.PK, Factory);
			shipment.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;

			CommonContainer container1 = shipment.QuotedBookingContainers.AddNew();
			CommonContainer container2 = shipment.QuotedBookingContainers.AddNew();
			CommonContainer container3 = shipment.QuotedBookingContainers.AddNew();

			RefContainer container20PL = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL"));
			RefContainer container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			container1.JC_RC = container20PL.PK;
			container2.JC_RC = container20PL.PK;
			container3.JC_RC = container40GP.PK;
			container1.JC_ContainerCount = 3;
			container2.JC_ContainerCount = 4;
			container3.JC_ContainerCount = 5;

			DocQuotedBooking wrapper = DocQuotedBooking.New(shipment, Factory);
			Assert("Should contain 7 20PL container but was " + wrapper.ContainerCount, wrapper.ContainerCount.Contains("7 X 20PL"));
			Assert("Should contain 5 40GP container but was " + wrapper.ContainerCount, wrapper.ContainerCount.Contains("5 X 40GP"));
		}

		public void TestDischargeETAString()
		{
			SetUpSailing();

			ZDateTime now = ZDateTime.Now;
			Sailing.Destination.JB_E_ARV = now.AddDays(10);

			ForwardingShipment b = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking shipment = QuotedBooking.New(ZGuid.Empty, b.PK, Factory);
			shipment.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			((IQuotedBooking)shipment).SailingJX = Sailing.PK;

			DocQuotedBooking wrapper = DocQuotedBooking.New(shipment, Factory);
			AssertContains("date set on sailing", Sailing.Destination.JB_E_ARV.ToShortDateString(), wrapper.DischargeETAString);

			((IQuotedBooking)shipment).SailingJX = ZGuid.Empty;
			AssertEquals("no sailing or consol", "", wrapper.DischargeETAString);

			ForwardingConsol consol = shipment.Booking.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;
			AssertEquals("if no sailing then try for a consol", now.AddDays(10).ToShortDateString(), wrapper.DischargeETAString);
		}

		public void TestContainerCommodities()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var shipment = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			shipment.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var containerType1 = shipment.QuotedBookingContainers.AddNew();
			containerType1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			containerType1.JC_RH_NKContainerCommodityCode = "IRON";

			var containerType2 = shipment.QuotedBookingContainers.AddNew();
			containerType2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			containerType2.JC_RH_NKContainerCommodityCode = "IRON";

			var containerType3 = shipment.QuotedBookingContainers.AddNew();
			containerType3.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL")).PK;
			containerType3.JC_RH_NKContainerCommodityCode = "";

			var containerType4 = shipment.QuotedBookingContainers.AddNew();
			containerType4.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			containerType4.JC_RH_NKContainerCommodityCode = "OCHM";

			var wrapper = DocQuotedBooking.New(shipment, Factory);
			AssertEquals("Expected to only include each code once", "IRON, OCHM", wrapper.ContainerCommodities);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocQuotedBooking.New(QuotedBooking, Factory) };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocQuotedBooking.New(QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory), Factory);
		}

		#region Implementation

		void SetUpSailing()
		{
			Sailing = Factory.New<JobSailing>();
			Voyage = Factory.New<JobVoyage>();
			Origin = Factory.New<VoyageOrigin>();
			Destination = Factory.New<VoyageDestination>();

			Origin.JA_JV = Voyage.PK;
			Destination.JB_JV = Voyage.PK;
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;
			((IQuotedBooking)QuotedBooking).SailingJX = Sailing.PK;
		}

		protected override void SetUp()
		{
			DocQuotedBooking = DocQuotedBooking.New(QuotedBooking, Factory);
			AssertNotNull(DocQuotedBooking);
			base.SetUp();
		}

		VoyageOrigin Origin;
		VoyageDestination Destination;
		JobVoyage Voyage;
		JobSailing Sailing;
		QuotedBooking fQuotedBooking;
		DocQuotedBooking DocQuotedBooking;

		QuotedBooking QuotedBooking
		{
			get
			{
				if (fQuotedBooking == null)
				{
					ForwardingShipment b = QuotedBooking.CreateNewBooking(Factory);
					fQuotedBooking = QuotedBooking.New(ZGuid.Empty, b.PK, Factory);
				}
				return fQuotedBooking;
			}
		}

		#endregion
	}
}
