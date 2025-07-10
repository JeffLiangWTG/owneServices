
using CargoWise.EntityFramework;

namespace Enterprise.Client.TNT.AirCargo
{
	public class XXXAirCargoCollection : NonPersistentBusinessObjectCollection<XXXAirCargo>	{
		public XXXAirCargoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public XXXAirCargo this[ConsignmentRecord record]
		{
			get
			{
				XXXAirCargo result = null;

				foreach (XXXAirCargo element in Elements)
				{
					if (element.HouseBill == record.HouseBill && element.Origin == record.Origin && element.Destination == record.Destination)
					{
						result = element;
						break;
					}
				}

				return result;
			}
		}

		protected override BusinessObject AddNewCore()
		{
			return null;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}
	}
}
