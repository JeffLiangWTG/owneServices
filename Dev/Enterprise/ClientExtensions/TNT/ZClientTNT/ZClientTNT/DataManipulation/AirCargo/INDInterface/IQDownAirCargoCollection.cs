using CargoWise.EntityFramework;

namespace Enterprise.Client.TNT.AirCargo
{
	public class IQDownAirCargoCollection : NonPersistentBusinessObjectCollection<IQDownAirCargo>	{
		public IQDownAirCargoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IQDownAirCargo this[FlightRecord flightRec]
		{
			get
			{
				IQDownAirCargo result = null;

				foreach (IQDownAirCargo element in Elements)
				{
					if (element.HasSameFlightDetail(flightRec))
					{
						result = element;
						break;
					}
				}

				return result;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IQDownAirCargo(Factory, new FlightRecord(""), "");
		}
	}
}
