#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class MandatoryAttributeTests : TestCase
	{
		#region INotificationProvidingAttribute

		public void TestValidate()
		{
			Component.MandatoryProperty = "value";
			AssertEquals("No errors when the property has a value", false, MetaData.GetNotifications(Component, TestComponent.Properties.MandatoryProperty).HasErrors());

			Component.MandatoryProperty = "";
			AssertEquals("Error when the value doesn't exist", true, MetaData.GetNotifications(Component, TestComponent.Properties.MandatoryProperty).HasErrors());

			Component.MandatoryProperty = null;
			AssertEquals("Error when the value doesn't exist", true, MetaData.GetNotifications(Component, TestComponent.Properties.MandatoryProperty).HasErrors());
		}

		public void TestProvidesNotifications()
		{
			INotificationProvidingAttribute non_mandatory_attr = (INotificationProvidingAttribute)TestComponent.Properties.Property.Attributes[typeof(MandatoryAttribute)];
			AssertEquals("A non-mandatory property doesn't provide notifications", false, non_mandatory_attr.ProvidesNotifications(TestComponent.Properties.Property));

			INotificationProvidingAttribute mandatory_attr = (INotificationProvidingAttribute)TestComponent.Properties.MandatoryProperty.Attributes[typeof(MandatoryAttribute)];
			AssertEquals("A mandatory property provides notifications", true, mandatory_attr.ProvidesNotifications(TestComponent.Properties.MandatoryProperty));
		}

		#endregion

		#region Test Classes

		internal class TestComponent
		{
			public static class Properties
			{
				public static readonly PropertyDescriptor Property = PropertyDescriptorCollectionWithMetaData.FromType(typeof(TestComponent))["Property"];
				public static readonly PropertyDescriptor MandatoryProperty = PropertyDescriptorCollectionWithMetaData.FromType(typeof(TestComponent))["MandatoryProperty"];
			}

			[Mandatory(false)]
			public string Property
			{
				get { return property; }
				set { property = value; }
			}
			string property;

			[Mandatory]
			public string MandatoryProperty
			{
				get { return mandatoryProperty; }
				set { mandatoryProperty = value; }
			}
			string mandatoryProperty;

			public static object Invoke(System.Reflection.MethodBase method, object obj, object[] parameters)
			{ return method.Invoke(obj, parameters); }
		}

		#endregion

		#region Implementation

		readonly TestComponent Component = new TestComponent();

		#endregion
	}
}
#endif
