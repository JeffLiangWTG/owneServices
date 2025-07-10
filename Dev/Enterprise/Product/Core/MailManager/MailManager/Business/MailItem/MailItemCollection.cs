
using CargoWise.EntityFramework;

namespace Enterprise.MailManager.Business
{
	public abstract class MailItemCollection : BusinessObjectCollection<MailItem>
	{
		public MailItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MailItemCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}