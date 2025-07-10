using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectAfterSaveNotificationForTest : BusinessObjectAfterSaveNotification
	{
		public BusinessObjectAfterSaveNotificationForTest(EnterpriseBusinessObject bizObj) : base(bizObj, NotificationSubscriberType.Info)
		{
		}

		public new BusinessObject BusinessEntity
		{
			get { return base.BusinessEntity; }
		}

		public ZGuid PK;
	}
}
