#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class ListAttributeTests : TestCase
	{
		public void TestAttributeWithListDataSourceOnly()
		{
			object list = MetaData.GetMetaData(Component, TestComponent.Properties.ListProperty, MetaDataTypes.ListDataSource);
			string value = (string)MetaData.GetMetaData(Component, TestComponent.Properties.ListProperty, MetaDataTypes.ListValueMember);
			string display = (string)MetaData.GetMetaData(Component, TestComponent.Properties.ListProperty, MetaDataTypes.ListDisplayMember);

			Assert(list is List<string>);
			AssertEquals("ListValueMember", "", value);
			AssertEquals("ListDisplayMember", "", display);
		}

		public void TestAttributeWhileProvidingDisplayValue()
		{
			object list = MetaData.GetMetaData(Component, TestComponent.Properties.ListPropertyWithValueDisplay, MetaDataTypes.ListDataSource);
			string value = (string)MetaData.GetMetaData(Component, TestComponent.Properties.ListPropertyWithValueDisplay, MetaDataTypes.ListValueMember);
			string display = (string)MetaData.GetMetaData(Component, TestComponent.Properties.ListPropertyWithValueDisplay, MetaDataTypes.ListDisplayMember);

			Assert(list is List<string>);
			AssertEquals("ListValueMember", "value", value);
			AssertEquals("ListDisplayMember", "display", display);
		}

		#region INotificationProvidingAttribute

		public void TestValidate()
		{
			Component.ListProperty = "value2";
			AssertEquals("No errors when the value exists in the list", false, MetaData.GetNotifications(Component, TestComponent.Properties.ListProperty).HasErrors());

			Component.ListProperty = "not in list";
			AssertEquals("Error when the value doesnt exist in the list", true, MetaData.GetNotifications(Component, TestComponent.Properties.ListProperty).HasErrors());
		}

		public void TestProvidesNotifications()
		{
			INotificationProvidingAttribute validated_attr = (INotificationProvidingAttribute)TestComponent.Properties.ListProperty.Attributes[typeof(ListAttribute)];
			INotificationProvidingAttribute non_validated_attr = (INotificationProvidingAttribute)TestComponent.Properties.ListPropertyWithValueDisplay.Attributes[typeof(ListAttribute)];

			AssertEquals("When AllowOnlyTheseValues = true", true, validated_attr.ProvidesNotifications(TestComponent.Properties.ListProperty));
			AssertEquals("When AllowOnlyTheseValues = false", false, non_validated_attr.ProvidesNotifications(TestComponent.Properties.ListPropertyWithValueDisplay));
		}

		#endregion

		#region Test Classes

		internal class TestComponent
		{
			public static class Properties
			{
				public static readonly PropertyDescriptor ListProperty = PropertyDescriptorCollectionWithMetaData.FromType(typeof(TestComponent))["ListProperty"];
				public static readonly PropertyDescriptor ListPropertyWithValueDisplay = PropertyDescriptorCollectionWithMetaData.FromType(typeof(TestComponent))["ListPropertyWithValueDisplay"];
			}

			[List("List", AllowOnlyTheseValues = true)]
			public string ListProperty
			{
				get { return listProperty; }
				set { listProperty = value; }
			}
			string listProperty;

			[List("List", "value", "display")]
			public string ListPropertyWithValueDisplay
			{
				get { return listPropertyWithValueDisplay; }
				set { listPropertyWithValueDisplay = value; }
			}
			string listPropertyWithValueDisplay;

			public IList<string> List
			{
				get
				{
					List<string> result = new List<string>();
					result.Add("value1");
					result.Add("value2");
					return result;
				}
			}
		}

		#endregion

		#region Implementation

		readonly TestComponent Component = new TestComponent();

		#endregion
	}
}
#endif
