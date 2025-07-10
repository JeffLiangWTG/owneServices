using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineNexDocNotificationCollection : ActiveBusinessObjectCollection<QuarantineNexDocNotification>
	{
		public QuarantineNexDocNotificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
