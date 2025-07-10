#if DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class WrappingPropertyDescriptorTests : TestCase
	{
		static WrappingPropertyDescriptorTests()
		{ MockMetaDataType.RegisterTypes(); }

		public void TestComponentType()
		{ AssertEquals(typeof(TestComponent), property.ComponentType); }

		public void TestName()
		{ AssertEquals("Related+Property", property.Name); }

		public void TestAddRemoveValueChanged()
		{
			property.AddValueChanged(component, new EventHandler(OnValueChanged));
			AssertEquals("Event shouldnt be fired initially", false, valueChangedCalled);

			valueChangedCalled = false;
			component.Related.Property = "splaty";
			AssertEquals("Event should be fired", true, valueChangedCalled);

			valueChangedCalled = false;
			component.Related = new TestRelatedComponent("aha");
			AssertEquals("Event should be fired", true, valueChangedCalled);

			valueChangedCalled = false;
			component.Related.Property = "bah";
			AssertEquals("Event should be fired", true, valueChangedCalled);

			valueChangedCalled = false;
			TestRelatedComponent old_related = component.Related;
			component.Related = new TestRelatedComponent("splaty");
			AssertEquals("Event should be fired", true, valueChangedCalled);
			valueChangedCalled = false;
			old_related.Property = "shouldntfirechanged";
			AssertEquals("Object no longer attached shouldnt fire event", false, valueChangedCalled);

			property.RemoveValueChanged(component, new EventHandler(OnValueChanged));
			valueChangedCalled = false;
			component.Related.Property = "aha!";
			AssertEquals("Event handler removed, shouldnt fire anymore", false, valueChangedCalled);

			component.Related = new TestRelatedComponent("blah");
			AssertEquals("Event handler removed, shouldnt fire anymore", false, valueChangedCalled);
			component.Related.Property = "splaty";
			AssertEquals("Event handler removed, shouldnt fire anymore", false, valueChangedCalled);
		}

		public void TestGetSetValue()
		{
			property.SetValue(component, "newvalue");
			AssertEquals("newvalue", property.GetValue(component));
		}

		public void TestAddValueChanged_DoesntCauseMemoryLeak()
		{
			var wref = AddEventToUnreferencedComponent();
			GC.Collect();
			AssertEquals("Component should be collected", false, wref.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference AddEventToUnreferencedComponent()
		{
			property.AddValueChanged(component, new EventHandler(new DummyHandlerClass().Handler));
			AssertEquals("Event shouldnt be fired initially", false, valueChangedCalled);

			var wref = new WeakReference(component);
			component = null;
			return wref;
		}

		public void TestAttributes_IFilteredAttributeForWrappingPropertyDescriptor()
		{
			WrappingPropertyDescriptor wrappedProperty = (WrappingPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["Related+Property"];

			TestFilteredAttribute filteredAttr = (TestFilteredAttribute)wrappedProperty.Attributes[typeof(TestFilteredAttribute)];
			AssertEquals("Related+TestPropertyOnFilteredAttribute", filteredAttr.TestPropertyOnFilteredAttribute);

			IEnumerator<Attribute> filteredAttrs = wrappedProperty.GetAttributesAllowMultiple(typeof(TestFilteredAttribute)).GetEnumerator();
			filteredAttrs.MoveNext();
			filteredAttr = (TestFilteredAttribute)filteredAttrs.Current;
			AssertEquals("Related+TestPropertyOnFilteredAttribute", filteredAttr.TestPropertyOnFilteredAttribute);
		}

		public void TestAttributes_WrappingPropertyNeverBrowsable()
		{
			PropertyDescriptor property = KPropertyDescriptorCollection.FromType(typeof(TestRelatedComponent))["Property"];
			WrappingPropertyDescriptor wrappedProperty = (WrappingPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["Related+Property"];
			AssertEquals("Inner property is browsable for the test", true, property.IsBrowsable);
			AssertEquals("Wrapping properties should never be browsable", true, wrappedProperty.IsBrowsable);
		}

		class DummyHandlerClass
		{
			public void Handler(object sender, EventArgs e) { }
		}

		public void TestFireChangeEvent()
		{
			property.AddValueChanged(component, new EventHandler(OnValueChanged));
			AssertEquals("Event shouldnt be fired initially", false, valueChangedCalled);
			property.FireChangeEvent(component);
			AssertEquals("Event should be fired due to FireChangeEvent called", true, valueChangedCalled);
		}

		public void TestHasSetter()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)["Related+Property"];
			AssertEquals("With setter", true, property.HasSetter());
			AssertEquals("With setter", true, ((KPropertyDescriptor)property).HasSetter());

			PropertyDescriptor propertyNoSetter = TypeDescriptor.GetProperties(component)["Related+PropertyNoSetter"];
			AssertEquals("No setter", false, propertyNoSetter.HasSetter());
			AssertEquals("No setter", false, ((KPropertyDescriptor)propertyNoSetter).HasSetter());
		}

		#region Test Classes

		internal class TestComponent : KComponent
		{
			TestRelatedComponent related = new TestRelatedComponent("");
			public TestRelatedComponent Related
			{
				get { return related; }
				set
				{
					related = value;
					if (RelatedChanged != null)
					{
						RelatedChanged(this, EventArgs.Empty);
					}
				}
			}
			public event EventHandler RelatedChanged;
		}

		internal class TestRelatedComponent : KComponent
		{
			public TestRelatedComponent(string propertyValue)
			{ this.propertyValue = propertyValue; }

			[MetaDataMember(MockMetaDataType.MetaDataOnMember, "PropertyMetaDataMember")]
			[MetaDataValue(MockMetaDataType.ConstantMetaData, 7)]
			[TestFiltered("TestPropertyOnFilteredAttribute")]
			public string Property
			{
				get { return propertyValue; }
				set
				{
					propertyValue = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, EventArgs.Empty);
					}
				}
			}
			string propertyValue;
			public event EventHandler PropertyChanged;

			public string PropertyNoSetter
			{
				get { return Property; }
			}

			public int PropertyMetaDataMember
			{ get { return 0; } }
		}

		class MockMetaDataType
		{
			public const string ConstantMetaData = "DWrappingPropertyDescriptor.Test.ConstantMetaData";
			public const string MetaDataOnMember = "DWrappingPropertyDescriptor.Test.MetaDataOnMember";

			public static void RegisterTypes()
			{
				MetaDataType.RegisterMetaDataType(
					new MetaDataType(ConstantMetaData, typeof(int), 0));
				MetaDataType.RegisterMetaDataType(
					new MetaDataType(MetaDataOnMember, typeof(int), 0));
			}
		}

		[AttributeUsage(AttributeTargets.Property)]
		sealed class TestFilteredAttribute : Attribute, IFilteredAttributeForWrappingPropertyDescriptor
		{
			public TestFilteredAttribute(string testPropertyOnFilteredAttribute)
			{ this.TestPropertyOnFilteredAttribute = testPropertyOnFilteredAttribute; }

			public string TestPropertyOnFilteredAttribute { get; private set; }

			public Attribute GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
			{ return new TestFilteredAttribute(wrappingProperty.Outer.Name + "+" + TestPropertyOnFilteredAttribute); }
		}

		#endregion

		#region Implementation

		WrappingPropertyDescriptor property;
		TestComponent component = new TestComponent();

		protected override void SetUp()
		{
			base.SetUp();
			property = (WrappingPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestComponent))["Related+Property"];
		}

		bool valueChangedCalled;
		void OnValueChanged(object sender, EventArgs e)
		{
			AssertEquals(
				"Sender should be the outer-most component, not an inner property",
				component, sender);
			valueChangedCalled = true;
		}

		void OnValueChanged_ThrowException(object sender, EventArgs e)
		{ throw new InvalidCastException("Didnt expect this event here"); }

		#endregion
	}
}
#endif
