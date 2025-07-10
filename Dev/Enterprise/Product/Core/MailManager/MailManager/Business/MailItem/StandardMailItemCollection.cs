using CargoWise.EntityFramework;

namespace Enterprise.MailManager.Business
{
	public class StandardMailItemCollection : MailItemCollection
	{
		public StandardMailItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public StandardMailItemCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
