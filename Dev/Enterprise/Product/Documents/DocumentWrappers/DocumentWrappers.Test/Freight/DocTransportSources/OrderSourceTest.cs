using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class OrderSourceTest : TestCaseWithFactory
	{
		#region TestVessel

		public void TestVessel()
		{
			Order.JD_RV_NKDepartureVessel = "vessel 1";
			Order.JD_RV_NKIntermediateVessel = "vessel 2";
			Order.JD_RV_NKArrivalVessel = "vessel 3";

			AssertEquals("vessel 1", SingleLegDetails.Vessel);
			AssertEquals("vessel 1", DepartureLegDetails.Vessel);
			AssertEquals("vessel 2", IntermediateLegDetails.Vessel);
			AssertEquals("vessel 3", ArrivalLegDetails.Vessel);
		}

		#endregion

		#region TestVoyage

		public void TestVoyage()
		{
			Order.JD_DepartureVoyage = "voyage 1";
			Order.JD_IntermediateVoyage = "voyage 2";
			Order.JD_ArrivalVoyage = "voyage 3";

			AssertEquals("voyage 1", SingleLegDetails.VoyageFlight);
			AssertEquals("voyage 1", DepartureLegDetails.VoyageFlight);
			AssertEquals("voyage 2", IntermediateLegDetails.VoyageFlight);
			AssertEquals("voyage 3", ArrivalLegDetails.VoyageFlight);
		}

		#endregion

		#region TestETD

		public void TestETD()
		{
			ZDateTime now = ZDateTime.SmallDateTimeNow;

			Order.UpdateEventEstimate(Events.Departure, now.AddDays(1).ToOffset());
			Order.JD_E_DEP_2 = now.AddDays(2);
			Order.JD_E_DEP_3 = now.AddDays(3);

			AssertEquals(now.AddDays(1), SingleLegDetails.ETD);
			AssertEquals(now.AddDays(1), DepartureLegDetails.ETD);
			AssertEquals(now.AddDays(2), IntermediateLegDetails.ETD);
			AssertEquals(now.AddDays(3), ArrivalLegDetails.ETD);
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			ZDateTime now = ZDateTime.SmallDateTimeNow;

			Order.JD_E_ARV_1stIntermediate = now.AddDays(1);
			Order.JD_E_ARV_2ndIntermediate = now.AddDays(2);
			Order.UpdateEventEstimate(Events.Arrival, now.AddDays(3).ToOffset());

			AssertEquals(now.AddDays(3), SingleLegDetails.ETA);
			AssertEquals(now.AddDays(1), DepartureLegDetails.ETA);
			AssertEquals(now.AddDays(2), IntermediateLegDetails.ETA);
			AssertEquals(now.AddDays(3), ArrivalLegDetails.ETA);
		}

		#endregion

		#region TestLoad

		public void TestLoad()
		{
			Order.JD_RL_NKPortOfLoading = "LOAD";

			AssertEquals("LOAD", SingleLegDetails.Load);
			AssertEquals("LOAD", DepartureLegDetails.Load);
			AssertEquals("", IntermediateLegDetails.Load);
			AssertEquals("", ArrivalLegDetails.Load);
		}

		#endregion

		#region TestDischarge

		public void TestDischarge()
		{
			Order.JD_RL_NKPortOfDischarge = "DISC";
			AssertEquals("DISC", SingleLegDetails.Discharge);
			AssertEquals("", DepartureLegDetails.Discharge);
			AssertEquals("", IntermediateLegDetails.Discharge);
			AssertEquals("DISC", ArrivalLegDetails.Discharge);
		}

		#endregion

		#region TestLegOrder

		public void TestLegOrder()
		{
			AssertEquals((byte)0, SingleLegDetails.LegOrder);
			AssertEquals((byte)1, DepartureLegDetails.LegOrder);
			AssertEquals((byte)2, IntermediateLegDetails.LegOrder);
			AssertEquals((byte)3, ArrivalLegDetails.LegOrder);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			ZGuid carrier = ZGuid.NewZGuid();
			Order.JD_OH_Carrier = carrier;

			AssertEquals(carrier, SingleLegDetails.Carrier);
			AssertEquals(carrier, DepartureLegDetails.Carrier);
			AssertEquals(carrier, IntermediateLegDetails.Carrier);
			AssertEquals(carrier, ArrivalLegDetails.Carrier);
		}

		#endregion

		#region Implementation

		#region Order

		Order Order
		{
			get
			{
				if (order == null)
				{
					order = Factory.New<Order>();
				}
				return order;
			}
		}

		Order order;

		#endregion

		#region SingleLegDetails

		ITransportDetails SingleLegDetails
		{
			get
			{
				if (singleLegDetails == null)
				{
					singleLegDetails = new OrderSource(Order, OrderSource.Leg.SingleLeg);
				}
				return singleLegDetails;
			}
		}

		ITransportDetails singleLegDetails;

		#endregion

		#region DepartureLegDetails

		ITransportDetails DepartureLegDetails
		{
			get
			{
				if (departureLegDetails == null)
				{
					departureLegDetails = new OrderSource(Order, OrderSource.Leg.Departure);
				}
				return departureLegDetails;
			}
		}

		ITransportDetails departureLegDetails;

		#endregion

		#region IntermediateLegDetails

		ITransportDetails IntermediateLegDetails
		{
			get
			{
				if (intermediateLegDetails == null)
				{
					intermediateLegDetails = new OrderSource(Order, OrderSource.Leg.Intermediate);
				}
				return intermediateLegDetails;
			}
		}

		ITransportDetails intermediateLegDetails;

		#endregion

		#region ArrivalLegDetails

		ITransportDetails ArrivalLegDetails
		{
			get
			{
				if (arrivalLegDetails == null)
				{
					arrivalLegDetails = new OrderSource(Order, OrderSource.Leg.Arrival);
				}
				return arrivalLegDetails;
			}
		}

		ITransportDetails arrivalLegDetails;

		#endregion

		#endregion

	}
}
