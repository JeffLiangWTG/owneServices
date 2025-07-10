#if DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class KPropertyDescriptorTests : TestCase
	{
		#region ToString / GetHashCode / Equals

		public void TestEquals()
		{
			KPropertyDescriptorCollection reflectedPropertyCollection = KPropertyDescriptorCollection.FromType(typeof(TestObj));
			PropertyDescriptor reflectedProperty = reflectedPropertyCollection["Property"];

			PropertyDescriptor1 property1 = new PropertyDescriptor1(
				reflectedPropertyCollection,
				reflectedProperty);
			PropertyDescriptor2 property2 = new PropertyDescriptor2(
				reflectedPropertyCollection,
				reflectedProperty);

			AssertEquals(false, property1.Equals(reflectedProperty));
			AssertEquals(false, property1.Equals(property2));
			AssertEquals(true, property1.Equals(property1));
		}

		#endregion

		#region GetExtenderProvider / IsReflectPropertyDescriptor

		public void TestGetExtenderProvider()
		{
			TestExtendee component = new TestExtendee();
			TestExtenderProvider provider = new TestExtenderProvider();
			component.Site = new TestSite(provider, component);

			PropertyDescriptor extenderProperty = TypeDescriptor.GetProperties(component)["ExtenderValue"];
			AssertNotNull("Should find the extender property for the test", extenderProperty);
			KPropertyDescriptor wrappedExtenderProperty = new KPropertyDescriptor(null, extenderProperty);
			IExtenderProvider actualProvider = wrappedExtenderProperty.GetExtenderProvider();
			AssertEquals("Should return the IExtenderProvider", provider, actualProvider);
		}

		[ProvideProperty("ExtenderValue", typeof(Component))]
		class TestExtenderProvider : Component, IExtenderProvider
		{
			public string GetExtenderValue(Component component)
			{ return "value"; }

			public void SetExtenderValue(Component component, string value)
			{
			}

			#region IExtenderProvider Members

			bool IExtenderProvider.CanExtend(object extendee)
			{ return true; }

			#endregion
		}

		class TestExtendee : Component
		{
		}

		class TestSite : ISite
		{
			readonly TestExtenderProvider provider;
			readonly TestExtendee extendee;

			public TestSite(TestExtenderProvider provider, TestExtendee extendee)
			{
				this.provider = provider;
				this.extendee = extendee;
			}

			object IServiceProvider.GetService(Type serviceType)
			{
				if (serviceType == typeof(IExtenderListService))
				{
					return new TestExtenderListService(provider);
				}
				return null;
			}

			#region ISite Members

			IComponent ISite.Component
			{ get { return extendee; } }

			IContainer ISite.Container
			{ get { return new Container(); } }

			bool ISite.DesignMode
			{ get { return false; } }

			string ISite.Name
			{
				get { return "x"; }
				set { }
			}

			#endregion
		}

		class TestExtenderListService : IExtenderListService
		{
			public TestExtenderListService(TestExtenderProvider provider)
			{ this.provider = provider; }

			public IExtenderProvider[] GetExtenderProviders()
			{ return new IExtenderProvider[] { provider }; }

			readonly TestExtenderProvider provider;
		}

		#endregion

		#region GetValue / SetValue / GetValueForMetaData

		public void TestGetValue()
		{
			TestComponent o = new TestComponent();
			KPropertyDescriptor property = new KPropertyDescriptor((KPropertyDescriptorCollection)TypeDescriptor.GetProperties(o), TypeDescriptor.GetProperties(o)["Property"]);
			o.Property = 2;
			AssertEquals(2, property.GetValue(o));
		}

		public void TestGetValueForMetaData()
		{
			TestComponent o = new TestComponent();
			KPropertyDescriptor property = new KPropertyDescriptor((KPropertyDescriptorCollection)TypeDescriptor.GetProperties(o), TypeDescriptor.GetProperties(o)["Property"]);
			o.Property = 2;
			AssertEquals(2, property.GetValueForMetaData(o));
		}

		[ExpectNoExceptions]
		public void TestTargetExceptionRaiseAppropriatelyOnGetValue()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["Property"];
			try
			{
				property.GetValue("notacomponent");
				Fail("Expected the correct exception type to be thrown");
			}
			catch (TargetException)
			{
			}
		}

		[ExpectNoExceptions]
		public void TestTargetExceptionRaiseAppropriatelyOnGetValueForMetaData()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["Property"];
			try
			{
				property.GetValueForMetaData("notacomponent");
				Fail("Expected the correct exception type to be thrown");
			}
			catch (TargetException)
			{
			}
		}

		[ExpectNoExceptions]
		public void TestTargetExceptionRaiseAppropriatelyOnSetValue()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["Property"];
			try
			{
				property.SetValue("notacomponent", null);
				Fail("Expected the correct exception type to be thrown");
			}
			catch (TargetException)
			{
			}
		}

		#endregion

		#region AddValueChanged / RemoveValueChanged

		#region TestCalculatedFromProperty

		public void TestCalculatedFromProperty()
		{
			TestObjWithCalculatedFromProperties o = new TestObjWithCalculatedFromProperties();
			PropertyDescriptor property2 = TypeDescriptor.GetProperties(o)["Property2"];

			bool calculatedPropChangedFired = false;
			EventHandler handler = delegate
			{ calculatedPropChangedFired = true; };
			property2.AddValueChanged(o, handler);
			calculatedPropChangedFired = false;
			o.Property2 = 2;
			AssertEquals("Change event should have fired", true, calculatedPropChangedFired);

			property2.RemoveValueChanged(o, handler);
			calculatedPropChangedFired = false;
			o.Property2 = 3;
			AssertEquals("Change event should NOT have fired", false, calculatedPropChangedFired);
		}

		internal class TestObjWithCalculatedFromProperties : KComponent
		{
			int property1;
			public int Property1
			{
				get { return property1; }
				set
				{
					property1 = value;
					if (Property1Changed != null)
					{
						Property1Changed(this, EventArgs.Empty);
					}
				}
			}
			public event EventHandler Property1Changed;

			[CalculatedFrom("Property1")]
			public int Property2
			{
				get { return Property1; }
				set { Property1 = value; }
			}
		}

		#endregion

		#endregion

		#region GetCalculatedFromProperties

		public void TestCalculatedFromProperties()
		{
			ComponentWithCalculatedFromProperties o = new ComponentWithCalculatedFromProperties();
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(o)["CalculatedFromOtherProperties"];
			PropertyDescriptor[] calculatedFromProperties = (PropertyDescriptor[])property.CalculatedFromProperties.Clone();
			Array.Sort(calculatedFromProperties, delegate(PropertyDescriptor x, PropertyDescriptor y)
			{
				return x.Name.CompareTo(y.Name);
			});

			AssertEquals(3, calculatedFromProperties.Length);
			AssertEquals("Property1", calculatedFromProperties[0].Name);
			AssertEquals("Property2", calculatedFromProperties[1].Name);
			AssertEquals("Property3", calculatedFromProperties[2].Name);
		}

		class ComponentWithCalculatedFromProperties : KComponent
		{
			[CalculatedFrom("Property1", "Property2")]
			[CalculatedFrom("Property3")]
			public string CalculatedFromOtherProperties
			{ get { return Property1; } }

			public string Property1
			{ get { return ""; } }

			public string Property2
			{ get { return ""; } }

			public string Property3
			{ get { return ""; } }
		}

		#endregion

		#region GetAttributeFromMostSpecificComponentType

		public void TestGetAttributeFromMostSpecificComponentType()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestGetAttributeFromMostSpecificComponentType_DerivedType))["Property"];
			DescriptionAttribute attr = (DescriptionAttribute)property.GetAttributeFromMostSpecificComponentType(typeof(DescriptionAttribute));
			AssertEquals("Should return most specific attribute type", "derived", attr.Description);

			PropertyDescriptor property2 = TypeDescriptor.GetProperties(typeof(TestGetAttributeFromMostSpecificComponentType_DerivedType2))["Property"];
			DescriptionAttribute attr2 = (DescriptionAttribute)property.GetAttributeFromMostSpecificComponentType(typeof(DescriptionAttribute));
			AssertEquals("Should return most specific attribute type", "derived", attr2.Description);
		}

		class TestGetAttributeFromMostSpecificComponentType_BaseType
		{
			[Description("base")]
			public int Property
			{ get { return 0; } }
		}

		class TestGetAttributeFromMostSpecificComponentType_DerivedType : TestGetAttributeFromMostSpecificComponentType_BaseType
		{
			[Description("derived")]
			public new int Property
			{ get { return 0; } }
		}

		class TestGetAttributeFromMostSpecificComponentType_DerivedType2 : TestGetAttributeFromMostSpecificComponentType_BaseType2
		{
			[Description("derived")]
			public new int Property
			{ get { return 0; } }
		}

		class TestGetAttributeFromMostSpecificComponentType_BaseType2
		{
			[Description("base")]
			public int Property
			{ get { return 0; } }
		}

		#endregion

		#region GetAttributesAllowMultiple

		public void TestGetAttributesAllowMultiple()
		{
			Attribute[] attrs;
			KPropertyDescriptor prop;
			KPropertyDescriptorCollection collection = (KPropertyDescriptorCollection)TypeDescriptor.GetProperties(new TestComponent());

			prop = (KPropertyDescriptor)collection["Property"];
			attrs = new List<Attribute>(prop.GetAttributesAllowMultiple(typeof(TestAttribute))).ToArray();
			AssertEquals("Should return both attributes", 2, attrs.Length);

			prop = new PropertyDescriptor1(collection, prop);
			attrs = new List<Attribute>(prop.GetAttributesAllowMultiple(typeof(TestAttribute))).ToArray();
			AssertEquals("Should return both attributes even when wrapping", 2, attrs.Length);
		}

		#endregion

		#region PropertyDescriptor Overrides

		public void TestGetChildProperties()
		{
			PropertyDescriptor prop = KPropertyDescriptorCollection.FromType(typeof(TestComponent))["Property"];
			PropertyDescriptorCollection child_collection = prop.GetChildProperties();
			Assert(child_collection is KPropertyDescriptorCollection);
		}

		#endregion

		#region Test Classes

		internal class TestComponent : KComponent
		{
			[Test("1")]
			[Test("2")]
			public int Property
			{
				get
				{
					AssertNotNull(this);
					return property;
				}
				set
				{
					property = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, EventArgs.Empty);
					}
				}
			}
			int property;
			public event EventHandler PropertyChanged;

			public int TestExceptionThrowingProperty
			{
				get { throw new ApplicationException(); }
				set { throw new ApplicationException(); }
			}
		}

		[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
		sealed class TestAttribute : Attribute
		{
			public TestAttribute(string value)
			{ this.Value = value; }

			public readonly string Value;
		}

		protected class PropertyDescriptor1 : KPropertyDescriptor
		{
			public PropertyDescriptor1(KPropertyDescriptorCollection collection, PropertyDescriptor inner)
				: base(collection, inner)
			{ }
		}

		protected class PropertyDescriptor2 : KPropertyDescriptor
		{
			public PropertyDescriptor2(KPropertyDescriptorCollection collection, PropertyDescriptor inner)
				: base(collection, inner)
			{ }
		}

		protected class TestObj
		{
			public int Property
			{
				get { return property; }
				set { property = value; }
			}
			int property;
		}

		#endregion
	}
}
#endif
