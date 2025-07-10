using System;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	internal class ZNotificationsPropertyDescriptor : KPropertyDescriptor
	{
		public ZNotificationsPropertyDescriptor(KPropertyDescriptorCollection collection, string metaDataPropertyName, string propertyName)
			: base(collection, metaDataPropertyName, Array.Empty<Attribute>())
		{
			this.propertyName = propertyName;
		}

		protected override object GetValueCore(object component)
		{
			ZPropertyInfo property = ((BusinessObject)component).ZPropertyInfoHash[propertyName];
			return property.Notifications;
		}

		readonly string propertyName;
	}
}
