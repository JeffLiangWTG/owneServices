using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Testing
{
	sealed class MockNotificationRenderContext : INotificationRenderContext
	{
		public void SetShouldAlwaysRenderAllNotifications(bool value)
		{ this.shouldAlwaysRenderAllNotifications = value; }

		public bool ShouldAlwaysRenderAllNotifications()
		{ return shouldAlwaysRenderAllNotifications; }
		bool shouldAlwaysRenderAllNotifications;

		public bool ShouldRenderNotification(Control control, object item, string property)
		{ return focusedProperties.ContainsKey(new FocusedDataProperty(item, property)); }

		public bool ShouldRenderAnyNotificationsFrom(Control control, object item)
		{
			foreach (FocusedDataProperty focusedProperty in focusedProperties.Keys)
			{
				if (object.Equals(focusedProperty.Item, item))
				{
					return true;
				}
			}
			return false;
		}

		public void NotifyDataControlFocused(Control control, object item, string property)
		{ focusedProperties[new FocusedDataProperty(item, property)] = null; }

		public void NotifyDataItemDeleted(Control control, object item)
		{
		}

		#region Focused Properties Dictionary

		readonly Dictionary<FocusedDataProperty, object> focusedProperties = new Dictionary<FocusedDataProperty, object>();

		class FocusedDataProperty
		{
			public FocusedDataProperty(object item, string property)
			{
				this.Item = item;
				this.Property = property;
			}

			public object Item { get; private set; }
			public string Property { get; private set; }

			public override bool Equals(object obj)
			{
				FocusedDataProperty rhs = obj as FocusedDataProperty;
				return rhs != null && object.Equals(Item, rhs.Item) && Property == rhs.Property;
			}

			public override int GetHashCode()
			{ return Item.GetHashCode(); }
		}

		#endregion

		#region INotificationRenderContext Members

		public void NotifyDataControlFocusedForAllProperties(Control control, object item)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
