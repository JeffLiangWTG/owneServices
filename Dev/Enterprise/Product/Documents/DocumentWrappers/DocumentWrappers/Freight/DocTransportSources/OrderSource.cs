using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Freight
{
	public class OrderSource : ITransportDetails
	{
		public enum Leg
		{
			SingleLeg = 0,
			Departure = 1,
			Intermediate = 2,
			Arrival = 3
		}

		public OrderSource(Order order, Leg leg)
		{
			this.order = order;
			this.leg = leg;
		}

		#region ITransportDetails Members

		public ZString ParentDescription
		{
			get { return ""; }
		}

		public ZString TransportMode
		{
			get { return order.JD_TransportMode; }
		}

		public ZString TransportType
		{
			get { return ""; }
		}

		public ZString TransportTypeDescription
		{
			get { return ""; }
		}

		public ZString Vessel
		{
			get
			{
				switch (leg)
				{
					case Leg.SingleLeg:
						return order.JD_RV_NKDepartureVessel;
					case Leg.Departure:
						return order.JD_RV_NKDepartureVessel;
					case Leg.Intermediate:
						return order.JD_RV_NKIntermediateVessel;
					case Leg.Arrival:
						return order.JD_RV_NKArrivalVessel;
					default:
						return "";
				}
			}
		}

		public ZString VoyageFlight
		{
			get
			{
				switch (leg)
				{
					case Leg.SingleLeg:
						return order.JD_DepartureVoyage;
					case Leg.Departure:
						return order.JD_DepartureVoyage;
					case Leg.Intermediate:
						return order.JD_IntermediateVoyage;
					case Leg.Arrival:
						return order.JD_ArrivalVoyage;
					default:
						return "";
				}
			}
		}

		public ZString Load
		{
			get
			{
				switch (leg)
				{
					case Leg.SingleLeg:
						return order.JD_RL_NKPortOfLoading;
					case Leg.Departure:
						return order.JD_RL_NKPortOfLoading;
					default:
						return "";
				}
			}
		}

		public ZString Discharge
		{
			get
			{
				switch (leg)
				{
					case Leg.SingleLeg:
						return order.JD_RL_NKPortOfDischarge;
					case Leg.Arrival:
						return order.JD_RL_NKPortOfDischarge;
					default:
						return "";
				}
			}
		}

		public ZByte LegOrder
		{
			get { return (byte)leg; }
		}

		public ZDateTime ETD
		{
			get
			{
				switch (leg)
				{
					case Leg.SingleLeg:
						return order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime();
					case Leg.Departure:
						return order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime();
					case Leg.Intermediate:
						return order.JD_E_DEP_2;
					case Leg.Arrival:
						return order.JD_E_DEP_3;
					default:
						return ZDateTime.Empty;
				}
			}
		}

		public ZDateTime ETA
		{
			get
			{
				switch (leg)
				{
					case Leg.SingleLeg:
						return order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime();
					case Leg.Departure:
						return order.JD_E_ARV_1stIntermediate;
					case Leg.Intermediate:
						return order.JD_E_ARV_2ndIntermediate;
					case Leg.Arrival:
						return order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime();
					default:
						return ZDateTime.Empty;
				}
			}
		}

		public ZDateTime ATD
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime ATA
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLCutOff
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLCutOff
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLAvailabilityDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLStorageDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZGuid Carrier
		{
			get { return order.JD_OH_Carrier; }
		}

		IFlightDetailsSuppression ITransportDetails.SuppressingBizO
		{
			get { return order; }
		}

		#endregion

		readonly Order order;
		readonly Leg leg;
	}
}
