using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyNotificationCollector : ZNotificationCollector
	{
		public DummyNotificationCollector(BusinessObject bizObj)
			: base(bizObj, true, true, PropertyDescriptionType.ColumnName)
		{
		}

		protected override bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
		{
			return info.Name != IgnorePropertyName && base.ShouldIncludeNotificationsFromInfo(info);
		}

		public ZGuid IgnorePK;
		public ZString IgnorePropertyName;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject business)
		{
			return business.PK != IgnorePK && base.ShouldIncludeNotificationsFromObject(business);
		}
	}
}
