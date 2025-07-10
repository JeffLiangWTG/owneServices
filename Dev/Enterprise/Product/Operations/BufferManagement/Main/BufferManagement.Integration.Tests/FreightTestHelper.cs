using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Integration.Tests
{
	static class FreightTestHelper
	{
		internal static TShipment CreateShipment<TShipment>(BusinessObjectFactory factory, string transportMode = Constants.TransportModes.Air, CommonConsol consol = null)
			where TShipment : CommonShipment
		{
			var shipment = factory.NewWithValidTestData<TShipment>();
			shipment.JS_TransportMode = transportMode;

			if (consol != null)
			{
				consol.Shipments.Add(shipment);
			}

			return shipment;
		}

		internal static TConsol CreateConsol<TConsol>(BusinessObjectFactory factory, string transportMode = Constants.TransportModes.Air, CommonShipment shipment = null)
			where TConsol : CommonConsol
		{
			var consol = factory.NewWithValidTestData<TConsol>();
			consol.JK_TransportMode = transportMode;

			if (shipment != null)
			{
				shipment.Consols.Add(consol);
			}

			return consol;
		}

		internal static JobConShipLink GetJobConShipLink(CommonConsol consol, CommonShipment shipment)
		{
			return (JobConShipLink)consol.Shipments.GetRelationshipBusinessObject(shipment);
		}

		internal static TContainer CreateContainer<TContainer>(BusinessObjectFactory factory, CommonConsol consol = null, string transportMode = Constants.TransportModes.Air)
			where TContainer : CommonContainer
		{
			var container = factory.NewWithValidTestData<TContainer>();

			if (consol != null)
			{
				consol.Containers.Add(container);
			}

			return container;
		}

		internal static void SetConsolDischargeETA(CommonShipment shipment, ZDateTime eta)
		{
			shipment.ArrivalConsol.Transports.ArrivalTransport.JW_ETA = eta;
		}

		internal static void SetConsolLoadingETD(CommonShipment shipment, ZDateTime etd)
		{
			shipment.DepartureConsol.Transports.ArrivalTransport.JW_ETD = etd;
		}

		internal static void SetShipmentDischargeETA(CommonShipment shipment, ZDateTime eta)
		{
			shipment.JS_E_ARV = eta;
		}

		internal static void SetShipmentLoadingETD(CommonShipment shipment, ZDateTime etd)
		{
			shipment.JS_E_DEP = etd;
		}

		internal static ForwardingShipment CreateForwardingShipment(BusinessObjectFactory factory, string departurePort, string arrivalPort, string departureConsolLoadingPort = null, string arrivalConsolDischargePort = null)
		{
			var shipment = FreightTestHelper.CreateShipment<ForwardingShipment>(factory);
			shipment.JS_RL_NKOrigin = departurePort;
			shipment.JS_RL_NKDestination = arrivalPort;

			var departureConsol = FreightTestHelper.CreateConsol<ForwardingConsol>(factory);
			departureConsol.JK_RL_NKLoadPort = departureConsolLoadingPort ?? departurePort;
			departureConsol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var arrivalConsol = FreightTestHelper.CreateConsol<ForwardingConsol>(factory);
			arrivalConsol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			arrivalConsol.JK_RL_NKDischargePort = arrivalConsolDischargePort ?? arrivalPort;

			shipment.Consols.Add(departureConsol);
			shipment.Consols.Add(arrivalConsol);

			return shipment;
		}

		internal static ForwardingShipment CreateStandaloneForwardingShipmentWithoutConsols(BusinessObjectFactory factory, string departurePort, string arrivalPort)
		{
			var shipment = FreightTestHelper.CreateShipment<ForwardingShipment>(factory);
			shipment.JS_RL_NKOrigin = departurePort;
			shipment.JS_RL_NKDestination = arrivalPort;

			return shipment;
		}
	}
}
