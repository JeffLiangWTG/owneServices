#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel.NotificationExtensions;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NotificationsExtensionsTests : TestCase
	{
		#region Mandatory Validation

		public void TestErrorIfEmpty()
		{
			NotificationCollection notifications;
			TestComponent component = new TestComponent("", "");

			component.ObjectProperty = "splaty";
			notifications = new NotificationCollection();
			notifications.ErrorIfEmpty(component, TypeDescriptor.GetProperties(component)["ObjectProperty"]);
			AssertEquals("Shouldn't have an error as the property has a value", false, notifications.HasErrors());

			component.ObjectProperty = null;
			notifications = new NotificationCollection();
			notifications.ErrorIfEmpty(component, TypeDescriptor.GetProperties(component)["ObjectProperty"]);
			AssertEquals("Should have an error as the property is null", true, notifications.HasErrors());

			component.ObjectProperty = "";
			notifications = new NotificationCollection();
			notifications.ErrorIfEmpty(component, TypeDescriptor.GetProperties(component)["ObjectProperty"]);
			AssertEquals("Should have an error as the property is an empty string", true, notifications.HasErrors());

			component.ObjectProperty = DBNull.Value;
			notifications = new NotificationCollection();
			notifications.ErrorIfEmpty(component, TypeDescriptor.GetProperties(component)["ObjectProperty"]);
			AssertEquals("Should have an error as the property is DBNull", true, notifications.HasErrors());

			component.ObjectProperty = new ArrayList();
			notifications = new NotificationCollection();
			notifications.ErrorIfEmpty(component, TypeDescriptor.GetProperties(component)["ObjectProperty"]);
			AssertEquals("Should have an error as the property is an empty list", true, notifications.HasErrors());

			((ArrayList)component.ObjectProperty).Add("splaty");
			notifications = new NotificationCollection();
			notifications.ErrorIfEmpty(component, TypeDescriptor.GetProperties(component)["ObjectProperty"]);
			AssertEquals("Shouldn't have an error as the property is a populated", false, notifications.HasErrors());
		}

		#endregion

		#region List Validation

		public void TestErrorIfNotInList_UsingIEnumerable()
		{ TestErrorIfNotInList(Component.List); }

		public void TestErrorIfNotInList_UsingIBindingList()
		{
			TestBindingList<KeyValuePair<string, string>> list = new TestBindingList<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, string> item in Component.List)
			{
				list.Add(item);
			}
			TestErrorIfNotInList(list);
		}

		void TestErrorIfNotInList(IEnumerable list)
		{
			PropertyDescriptor propertyWithList = TypeDescriptor.GetProperties(Component)["PropertyWithList"];
			PropertyDescriptor valueProperty = TypeDescriptor.GetProperties(typeof(KeyValuePair<string, string>))["Key"];

			Component.PropertyWithList = "value1";
			Notifications.ErrorIfNotInList(Component, propertyWithList, valueProperty, list);
			AssertEquals("No error added if the value exists in the list", false, Notifications.HasErrors());

			Component.PropertyWithList = "not in list";
			Notifications.ErrorIfNotInList(Component, propertyWithList, valueProperty, list);
			AssertEquals("Error when the value doesn't exist in the list", true, Notifications.HasErrors());
			Notifications.Clear();

			Component.PropertyWithList = "";
			Notifications.ErrorIfNotInList(Component, propertyWithList, valueProperty, list);
			AssertEquals("No error added if the value is empty", false, Notifications.HasErrors());

			Component.PropertyWithList = null;
			Notifications.ErrorIfNotInList(Component, propertyWithList, valueProperty, list);
			AssertEquals("No error added if the value is empty", false, Notifications.HasErrors());
		}

		class TestBindingList<T> : KBindingList<T>
		{
			protected override bool SupportsSearchingCore
			{ get { return true; } }

			protected override int FindCore(PropertyDescriptor property, object key)
			{
				for (int i = 0; i < Count; i++)
				{
					T item = this[i];
					if (object.Equals(property.GetValue(item), key))
					{
						return i;
					}
				}
				return -1;
			}
		}

		#endregion

		#region Test Classes

		internal class TestComponent : KComponent
		{
			public TestComponent()
			{
			}

			public TestComponent(string property1, string property2)
			{
				this.property1 = property1;
				this.property2 = property2;
			}

			#region Property1

			public string Property1
			{
				get { return property1; }
				set { property1 = value; }
			}
			string property1;

			#endregion

			#region Property2

			public string Property2
			{
				get { return property2; }
				set { property2 = value; }
			}
			string property2;

			#endregion

			public decimal DecimalProperty
			{
				get { return decimalProperty; }
				set { decimalProperty = value; }
			}
			decimal decimalProperty;

			public object ObjectProperty
			{
				get { return objectProperty; }
				set { objectProperty = value; }
			}
			object objectProperty;

			[List("List", "Value", "Display")]
			public string PropertyWithList
			{
				get { return propertyWithList; }
				set { propertyWithList = value; }
			}
			string propertyWithList;

			public KeyValuePair<string, string>[] List
			{
				get
				{
					return new KeyValuePair<string, string>[]
					{
							new KeyValuePair<string, string>("value1", "display1"),
							new KeyValuePair<string, string>("value2", "display2"),
					};
				}
			}
		}

		#endregion

		#region Implementation

		readonly TestComponent Component = new TestComponent();
		readonly NotificationCollection Notifications = new NotificationCollection();

		#endregion
	}
}
#endif
