using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business
{
	public class EDICommunicationsModeCollection : BusinessObjectCollection<EDICommunicationsMode>
	{
		public EDICommunicationsModeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public EDICommunicationsModeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
