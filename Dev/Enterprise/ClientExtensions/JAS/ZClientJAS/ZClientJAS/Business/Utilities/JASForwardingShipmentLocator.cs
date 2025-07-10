
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Utilities
{
	public class JASForwardingShipmentLocator
	{
		public JASForwardingShipment Find(BusinessObjectFactory factory, ZString houseBillNumber, ZString transportMode, ZString origin, ZString destination)
		{
			return (factory != null) ? FindShipment(factory, houseBillNumber, transportMode, origin, destination) : null;
		}

		public JASForwardingShipment Find(JASForwardingConsol consol, ZString houseBillNumber, ZString transportMode, ZString origin, ZString destination)
		{
			JASForwardingShipment result = null;

			if (consol != null)
			{
				result = GetShipmentFromConsol(consol, houseBillNumber);
				if (result == null)
				{
					result = FindShipment(consol.Factory, houseBillNumber, transportMode, origin, destination);
				}
			}

			return result;
		}

		JASForwardingShipment GetShipmentFromConsol(JASForwardingConsol consol, ZString houseBillNumber)
		{
			JASForwardingShipment result = null;

			if (consol != null && !houseBillNumber.IsEmpty)
			{
				ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, houseBillNumber);
				filter.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_HouseBill, SQLComparisonOperator.Equal, houseBillNumber.KeepNumericCharacters());
				JASForwardingShipment[] shipments = (JASForwardingShipment[])consol.Shipments.Find(filter);
				if (shipments.Length > 0)
				{
					result = shipments[0];
				}
			}

			return result;
		}

		JASForwardingShipment FindShipment(BusinessObjectFactory factory, ZString houseBillNumber, ZString transportMode, ZString origin, ZString destination)
		{
			JASForwardingShipment result = null;

			if (!houseBillNumber.IsEmpty)
			{
				ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, houseBillNumber);
				if (!transportMode.IsEmpty)
				{
					filter.AddToFilter(JobShipmentSchema.JS_TransportMode, transportMode);
				}

				if (!origin.IsEmpty)
				{
					filter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.StartsWith, origin.Left(2));
				}

				if (!destination.IsEmpty)
				{
					filter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, SQLComparisonOperator.StartsWith, destination.Left(2));
				}

				result = FindUniqueShipment(factory, filter);
			}

			return result;
		}

		JASForwardingShipment FindUniqueShipment(BusinessObjectFactory factory, ZQuery filter)
		{
			JASForwardingShipment[] shipments = (JASForwardingShipment[])factory.Load(typeof(JASForwardingShipment), filter);
			return (shipments.Length == 1) ? shipments[0] : null;
		}
	}
}
