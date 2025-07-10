using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DummyIContainerMessaging : NonPersistentBusinessObject, IPRAContainerMessaging, IObsoleteValidation
	{
		public DummyIContainerMessaging(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString CurrentPRAStatus => ZString.Empty;

		public ZPropertyInfo CurrentPRAStatusInfo => GetZPropertyInfo(nameof(CurrentPRAStatus));

		public PRAMessageCollection PRAMessages => praMessages ?? (praMessages = new PRAMessageCollection(this));
		PRAMessageCollection praMessages;
	}
}
