using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.EDICommunicationAuthInbound
{
	public class ScopeData : NonPersistentBusinessObject
	{
		public ZString Name { get; set; }
	}
}
