using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class ConsolImporter
	{
		public ConsolImporter(FlightDetailsLine line, BusinessObjectFactory factory)
		{
			this.Line = line;
			this.Factory = factory;
		}

		public ForwardingConsol ImportToConsol()
		{
			Line.Process();
			ForwardingConsol consol = (ForwardingConsol)Factory.New(typeof(ForwardingConsol));
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_TransportType = Constants.TransportPlanningType.Flight1;

			transport.JW_RL_NKLoadPort = Line.PortOfLoading;
			transport.JW_RL_NKDiscPort = Line.PortOfDischarge;
			transport.JW_VoyageFlight = Line.ED_FlightNumber;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_IsNeutralMaster = ZBool.True;
			consol.MasterBillAirlinePrefix = Line.ED_AirWayBill.SubstringSafe(0, 3);
			consol.MasterBillMAWB = Line.ED_AirWayBill.SubstringSafe(3, 8);
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			transport.JW_ETD = Line.ED_DepartureDate;
			transport.JW_ETA = Line.ArrivalDate;

			return consol;
		}

		#region Implementation

		internal protected FlightDetailsLine Line;
		internal protected BusinessObjectFactory Factory;

		#endregion
		#region Implementation
		#endregion
	}
}
