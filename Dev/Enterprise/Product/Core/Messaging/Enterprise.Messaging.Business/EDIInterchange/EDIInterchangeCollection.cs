using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	public class EDIInterchangeCollection : BusinessObjectCollection<EDIInterchange>
	{
		public EDIInterchangeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
			this.Load();
		}

		public EDIInterchangeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
