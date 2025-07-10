#if DEBUG
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class PropertyDescriptorCollectionWithMetaDataTests : TestCase
	{
		static PropertyDescriptorCollectionWithMetaDataTests()
		{ MockMetaDataType.RegisterTypes(); }

		public void TestWrappedMetadataPropertyDescriptor()
		{
			Assert("Tested in CargoWise.EntityFramework.BusinessObjectPropertyDescriptorCollection.TestTestBindingMetadataOnWrappedProperties()", true);
		}

		#region Factory Method

		public void TestFromType_UsesLRUCache()
		{
			TypeDescriptor.Refresh(typeof(TestConstantObj));

			var propertiesRef = CreateUnrefrencedPropertiesCollection();

			Type[] typeList = typeof(int).Assembly.GetTypes();
			for (int i = 0; i < 100; i++)
			{
				// invalidate the LRU cache
				PropertyDescriptorCollectionWithMetaData.FromType(typeList[i]);
			}
			GC.Collect();
			AssertEquals("The property collection should be collected when out of scope", false, propertiesRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateUnrefrencedPropertiesCollection()
		{
			var properties = PropertyDescriptorCollectionWithMetaData.FromType(typeof(TestConstantObj));
			var propertiesRef = new WeakReference(properties);
			return propertiesRef;
		}

		#endregion

		#region KPropertyDescriptorCollection Overrides

		public void TestFind_UsingMetaDataPropertyNameAndIgnoreCase()
		{
			PropertyDescriptorCollectionWithMetaData properties = new PropertyDescriptorCollectionWithMetaData(typeof(TestConstantObj), false);
			properties.PopulatePropertyDescriptors();
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithConstantMetaData"];

			string metaPropertyName = properties.GetMetaDataPropertyName(property, MetaDataTypes.ReadOnly, false);
			PropertyDescriptor metaProperty = properties.Find(metaPropertyName.ToLower(), true);
			AssertNotNull("Should find the property even though the name is in lowercase", metaProperty);
		}

		[ExpectNoExceptions]
		public void TestPrivatePropertiesNotIncludedInPublicProperties()
		{
			TestMemberObj o = new TestMemberObj();
			PropertyDescriptorCollectionWithMetaData collection = (PropertyDescriptorCollectionWithMetaData)TypeDescriptor.GetProperties(o);
			KPropertyDescriptor property = (KPropertyDescriptor)collection["PropertyWithCalculatedMetaData"];

			string metadataPropertyName = collection.GetMetaDataPropertyName(property, MockMetaDataType.TestType, false);
			PropertyDescriptor metadata_property = collection.GetMetaDataProperty(o.GetType(), property, MockMetaDataType.TestType);

			foreach (KPropertyDescriptor next in collection)
			{
				if (o.GetType().GetProperty(next.Name) == null)
				{
					Fail("Property '" + property.Name + "' is private, but is included in the public property collection. This is not a good idea because someone might try to bind to it, which will fail intermittently.");
				}
			}
		}

		#endregion

		#region GetMetaDataPropertyName / GetMetaDataProperty

		public void TestGetMetaDataPropertyName_OnPropertiesNotInCollection()
		{
			TestConstantObj o = new TestConstantObj();
			PropertyDescriptorCollectionWithMetaData properties = (PropertyDescriptorCollectionWithMetaData)TypeDescriptor.GetProperties(o);
			KPropertyDescriptor wrappedProperty = (KPropertyDescriptor)properties["Related+RelatedPropertyWithConstantMetaData"];

			string metaPropertyName = properties.GetMetaDataPropertyName(wrappedProperty, MockMetaDataType.TestType, false);
			AssertEquals(
				"Should have returned the name of the outer, wrapped property",
				true, metaPropertyName.StartsWith("Related+RelatedPropertyWithConstantMetaData"));
		}

		public void TestGetMetaDataProperty()
		{
			TestConstantObj o = new TestConstantObj();
			PropertyDescriptorCollectionWithMetaData properties = (PropertyDescriptorCollectionWithMetaData)TypeDescriptor.GetProperties(o);

			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithConstantMetaData"];
			KPropertyDescriptor metaDataProperty = properties.GetMetaDataProperty(o.GetType(), property, MockMetaDataType.TestType);
			AssertEquals("Meta-data PropertyDescriptor.Name", properties.GetMetaDataPropertyName(property, MockMetaDataType.TestType, false), metaDataProperty.Name);
			AssertEquals("Meta-data PropertyDescriptor.GetValue()", 6, metaDataProperty.GetValue(o));

			AssertEquals("GetMetaDataPropertyName().Name consistent with GetMetaDataPropertyName()", properties.GetMetaDataPropertyName(property, MockMetaDataType.TestType, false), metaDataProperty.Name);
		}

		public void TestGetMetaDataProperty_WithDifferentMetaDataSpecifiedOnSubclass()
		{
			TestConstantObj baseObj = new TestConstantObj();
			TestConstantSubclassedObj derivedObj = new TestConstantSubclassedObj();
			PropertyDescriptorCollectionWithMetaData baseProperties = (PropertyDescriptorCollectionWithMetaData)TypeDescriptor.GetProperties(baseObj);
			PropertyDescriptorCollectionWithMetaData derivedProperties = (PropertyDescriptorCollectionWithMetaData)TypeDescriptor.GetProperties(derivedObj);

			KPropertyDescriptor baseProperty = (KPropertyDescriptor)baseProperties["PropertyWithConstantMetaData"];
			KPropertyDescriptor derivedProperty = (KPropertyDescriptor)derivedProperties["PropertyWithConstantMetaData"];
			KPropertyDescriptor derivedMetaDataProperty = baseProperties.GetMetaDataProperty(typeof(TestConstantSubclassedObj), baseProperty, MockMetaDataType.TestType);
			AssertEquals("Meta-data PropertyDescriptor.GetValue() for derived class with different meta-data", 500, derivedMetaDataProperty.GetValue(derivedObj));

			AssertEquals("GetMetaDataProperty().Name consistent with GetMetaDataPropertyName()", baseProperties.GetMetaDataPropertyName(derivedProperty, MockMetaDataType.TestType, false), derivedMetaDataProperty.Name);
		}

		public void TestGetMetaDataProperty_WithPrivatePropertyAccessedFromSubclass()
		{
			TestConstantObj baseObj = new TestConstantObj();
			TestConstantSubclassedObj derivedObj = new TestConstantSubclassedObj();
			PropertyDescriptorCollectionWithMetaData basePropertiesIncludingPrivate = (PropertyDescriptorCollectionWithMetaData)((KPropertyDescriptorCollection)TypeDescriptor.GetProperties(baseObj)).AllProperties;

			KPropertyDescriptor baseProperty = (KPropertyDescriptor)basePropertiesIncludingPrivate["PropertyWithConstantMetaDataPrivate"];
			KPropertyDescriptor derivedMetaDataProperty = basePropertiesIncludingPrivate.GetMetaDataProperty(typeof(TestConstantSubclassedObj), baseProperty, MockMetaDataType.TestType);
			AssertEquals(6, derivedMetaDataProperty.GetValue(derivedObj));

			AssertEquals("GetMetaDataProperty().Name consistent with GetMetaDataPropertyName()", basePropertiesIncludingPrivate.GetMetaDataPropertyName(baseProperty, MockMetaDataType.TestType, false), derivedMetaDataProperty.Name);
		}

		#endregion

		#region NewMetaDataProperty

		public void TestConstantMetaData()
		{
			TestConstantObj obj = new TestConstantObj();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithConstantMetaData"];
			KPropertyDescriptor metaDataProperty = MetaData.GetMetaDataProperty(obj.GetType(), property, MockMetaDataType.TestType);

			AssertNotEquals(
				property.Name, metaDataProperty.Name,
				"Meta data property cannot have same name as the property it is describing");
			AssertEquals(6, metaDataProperty.GetValue(new TestConstantObj()));
		}

		public void TestMemberMetaData()
		{
			TestMemberObj obj = new TestMemberObj();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithCalculatedMetaData"];
			KPropertyDescriptor metaDataProperty = MetaData.GetMetaDataProperty(obj.GetType(), property, MockMetaDataType.TestType);

			AssertNotEquals(
				property.Name, metaDataProperty.Name,
				"Meta data property cannot have same name as the property it is describing");
			AssertEquals(68, metaDataProperty.GetValue(new TestMemberObj()));
		}

		public void TestMethodProvidedMetaData()
		{
			TestMethodProvidedMetaPropertyObj obj = new TestMethodProvidedMetaPropertyObj();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithMethodProvidedMetaData"];
			KPropertyDescriptor methodProvidedMetaData = MetaData.GetMetaDataProperty(obj.GetType(), property, MockMetaDataType.TestType);

			AssertNotEquals(
				property.Name, methodProvidedMetaData.Name,
				"Meta data property cannot have same name as the property it is describing");
			AssertEquals(23, methodProvidedMetaData.GetValue(obj));
			methodProvidedMetaData.SetValue(obj, 24);
			AssertEquals(24, methodProvidedMetaData.GetValue(obj));

			KPropertyDescriptor propertyProvidedMetaData = MetaData.GetMetaDataProperty(obj.GetType(), property, MockMetaDataType.TestType, true);
			AssertEquals("When excluding method provider and instead falling back to meta-data property", 5, propertyProvidedMetaData.GetValue(obj));
		}

		public void TestConstantAndMethodProvided_BothInUse()
		{
			TestUsingConstantAttributeAndMethodProvided obj = new TestUsingConstantAttributeAndMethodProvided();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithConstantAndMethodProvidedMetaData"];
			KPropertyDescriptor metaDataProperty = MetaData.GetMetaDataProperty(obj.GetType(), property, MockMetaDataType.TestType);

			AssertNotEquals(
				property.Name, metaDataProperty.Name,
				"Meta data property cannot have same name as the property it is describing");
			AssertEquals(
				"Should use ProvidePropertyMetaDataAttribute over the constant property as it takes precedence",
				1, metaDataProperty.GetValue(obj));
			AssertEquals(
				"Property meta-data used in place of ProvidePropertyMetaDataAttribute when excludeMethodProvider = true",
				2, MetaData.GetMetaData(obj, property, MockMetaDataType.TestType, true));
		}

		[ExpectNoExceptions]
		public void TestGetMetaDataPropertyWithUnexpectedComponentType()
		{
			try
			{
				TestMethodProvidedMetaPropertyObj obj = new TestMethodProvidedMetaPropertyObj();
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
				KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithMethodProvidedMetaData"];
				KPropertyDescriptor methodProvidedMetaData = MetaData.GetMetaDataProperty("".GetType(), property, MockMetaDataType.TestType);
			}
			catch (ArgumentException e)
			{
				//intentionally break this string for readability
				StringBuilder sb = new StringBuilder("componentType must be a sub-class of prop.ComponentType. componentType=System.String, ");
				sb.Append("property.Name=PropertyWithMethodProvidedMetaData, property.ComponentType");
#if NETFRAMEWORK
				sb.Append("=CargoWise.ComponentModel.Testing.PropertyDescriptorCollectionWithMetaDataTests+TestMethodProvidedMetaPropertyObj\r\nParameter name: componentType");
#else
				sb.Append("=CargoWise.ComponentModel.Testing.PropertyDescriptorCollectionWithMetaDataTests+TestMethodProvidedMetaPropertyObj (Parameter 'componentType')");
#endif
				AssertEquals(sb.ToString(), e.Message);
			}
		}

#endregion

		#region Notifications

		public void TestNotifications_WithMandatoryAttribute()
		{
			ComponentWithNotifications component = new ComponentWithNotifications();
			AssertPropertyNotifications(component, "MandatoryProperty", "You must enter a value for MandatoryProperty");
		}

		public void TestNotifications_WithValidationMethod()
		{
			ComponentWithNotifications component = new ComponentWithNotifications();
			AssertPropertyNotifications(component, "ValidatedProperty", "Validated Notification");
		}

		public void TestNotifications_WithMandatoryAttributeAndValidationMethod()
		{
			ComponentWithNotifications component = new ComponentWithNotifications();
			AssertPropertyNotifications(component, "MandatoryAndValidatedProperty", "Validated Notification\r\nYou must enter a value for MandatoryAndValidatedProperty");
		}

		void AssertPropertyNotifications(ComponentWithNotifications component, string propertyName, string expectedNotifications)
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)[propertyName];
			NotificationCollection actualNotifications = NotificationCollection.Cast(MetaData.GetNotifications(component, property));
			AssertEquals(expectedNotifications.Replace("\r\n", "\n").Trim(), actualNotifications.ToMessageListString().Replace("\r\n", "\n").Trim());
		}

		#endregion

		#region IEnumerable

		public void TestGetEnumerator_DoesntIncludeMetaDataProperties_SpecifiedWithConstant()
		{
			TestConstantObj o = new TestConstantObj();
			PropertyDescriptorCollectionWithMetaData properties = new PropertyDescriptorCollectionWithMetaData(o.GetType(), false);
			properties.PopulatePropertyDescriptors();
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithConstantMetaData"];

			string metaPropertyName = properties.GetMetaDataPropertyName(property, MockMetaDataType.TestType, false);
			KPropertyDescriptor metaDataProperty = (KPropertyDescriptor)properties[metaPropertyName];
			AssertNotNull("Meta-data property should not be null for the test", metaDataProperty);

			AssertEnumerableFromPropertyDescriptorCollection("PropertyDescriptorCollection.GetEnumerator should enumerate over all, because this is what data binding will do", properties, metaDataProperty, true);
			AssertEnumerableFromKPropertyDescriptorCollection("KPropertyDescriptorCollection.GetEnumerator should NOT enumerate meta-data props", properties, metaDataProperty, false);
		}

		public void TestGetEnumerator_DoesntIncludeMetaDataProperties_MetaDataSpecifiedWithPrivateMember()
		{
			TestMemberObj o = new TestMemberObj();
			PropertyDescriptorCollectionWithMetaData properties = new PropertyDescriptorCollectionWithMetaData(o.GetType(), false);
			properties.PopulatePropertyDescriptors();
			KPropertyDescriptor property = (KPropertyDescriptor)properties["PropertyWithCalculatedMetaData"];

			string metaPropertyName = properties.GetMetaDataPropertyName(property, MockMetaDataType.TestType, false);
			KPropertyDescriptor metaDataProperty = (KPropertyDescriptor)properties[metaPropertyName];
			AssertNotNull("Meta-data property should not be null for the test", metaDataProperty);

			AssertEnumerableFromPropertyDescriptorCollection("PropertyDescriptorCollection.GetEnumerator should enumerate over all, because this is what data binding will do", properties, metaDataProperty, true);
			AssertEnumerableFromKPropertyDescriptorCollection("KPropertyDescriptorCollection.GetEnumerator should NOT enumerate meta-data props", properties, metaDataProperty, false);
		}

		void AssertEnumerableFromPropertyDescriptorCollection(string message, KPropertyDescriptorCollection properties, PropertyDescriptor property, bool expectEnumerate)
		{
			bool found = false;
			foreach (PropertyDescriptor next in (PropertyDescriptorCollection)properties)
			{
				if (next == property)
				{
					found = true;
				}
			}
			AssertEquals(message, expectEnumerate, found);
		}

		void AssertEnumerableFromKPropertyDescriptorCollection(string message, KPropertyDescriptorCollection properties, PropertyDescriptor property, bool expectEnumerate)
		{
			bool found = false;
			foreach (PropertyDescriptor next in properties)
			{
				if (next == property)
				{
					found = true;
				}
			}
			AssertEquals(message, expectEnumerate, found);
		}

		#endregion

		#region Test Classes

		internal class MockMetaDataType
		{
			public const string TestType = "PropertyDescriptorCollectionWithMetaData.Test";

			public static void RegisterTypes()
			{
				MetaDataType.RegisterMetaDataType(
					new MetaDataType(TestType, typeof(int), 0));
			}
		}

		internal class TestConstantObj : KComponent
		{
			public TestRelatedConstantObj Related
			{ get { return null; } }

			[MetaDataValue(MockMetaDataType.TestType, 6)]
			public virtual int PropertyWithConstantMetaData
			{ get { return 0; } }

			[MetaDataValue(MockMetaDataType.TestType, 6)]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test scenario")]
			int PropertyWithConstantMetaDataPrivate
			{ get { return 0; } }
		}

		internal class TestConstantSubclassedObj : TestConstantObj
		{
			[MetaDataMember(MockMetaDataType.TestType, "PropertyWithConstantMetaData_MetaData")]
			public override int PropertyWithConstantMetaData
			{ get { return 0; } }

			public int PropertyWithConstantMetaData_MetaData
			{ get { return 500; } }
		}

		internal class TestRelatedConstantObj : KComponent
		{
			public TestRelatedConstantObj Related
			{ get { return null; } }

			[MetaDataValue(MockMetaDataType.TestType, 6)]
			public int RelatedPropertyWithConstantMetaData
			{ get { return 0; } }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		class TestMemberObj : KComponent
		{
			[MetaDataMember(MockMetaDataType.TestType, "PrivatePropMetaData")]
			public int PropertyWithCalculatedMetaData
			{ get { return 0; } }

			protected int PrivatePropMetaData
			{ get { return 68; } }

			public static object Invoke(System.Reflection.MethodBase method, object obj, object[] parameters)
			{ return method.Invoke(obj, parameters); }
		}

		[ProvideMetaDataProperty("TestType", MockMetaDataType.TestType)]
		internal abstract class TestMethodProvidedMetaPropertyObjParent : KComponent
		{
		}

		internal class TestMethodProvidedMetaPropertyObj : TestMethodProvidedMetaPropertyObjParent
		{
			[MetaDataMember(MockMetaDataType.TestType, "PropertyWithMethodProvidedMetaData_MetaData")]
			public string PropertyWithMethodProvidedMetaData
			{ get { return ""; } }

			protected int PropertyWithMethodProvidedMetaData_MetaData
			{ get { return 5; } }

			public int GetTestType(PropertyDescriptor property)
			{
				Assertion.AssertEquals("PropertyWithMethodProvidedMetaData", property.Name);
				return this.testMetaData;
			}

			public void SetTestType(PropertyDescriptor property, int value)
			{
				Assertion.AssertEquals("PropertyWithMethodProvidedMetaData", property.Name);
				this.testMetaData = value;
			}

			int testMetaData = 23;
		}

		[ProvideMetaDataProperty("TestType", MockMetaDataType.TestType)]
		internal class TestUsingConstantAttributeAndMethodProvided : KComponent
		{
			[MetaDataValue(MockMetaDataType.TestType, 2)]
			public int PropertyWithConstantAndMethodProvidedMetaData
			{ get { return 0; } }

			public int GetTestType(PropertyDescriptor property)
			{ return 1; }
		}

		internal class ComponentWithNotifications : KComponent
		{
			[Mandatory]
			public string MandatoryProperty
			{
				get { return mandatoryProperty; }
				set { mandatoryProperty = value; }
			}
			string mandatoryProperty;

			[NotificationsMember("ValidateProperty")]
			public string ValidatedProperty
			{
				get { return validatedProperty; }
				set { validatedProperty = value; }
			}
			string validatedProperty;

			[Mandatory, NotificationsMember("ValidateProperty")]
			public string MandatoryAndValidatedProperty
			{
				get { return mandatoryAndValidatedProperty; }
				set { mandatoryAndValidatedProperty = value; }
			}
			string mandatoryAndValidatedProperty;

			protected NotificationCollection ValidateProperty
			{
				get
				{
					NotificationCollection result = new NotificationCollection();
					result.AddError("Validated Notification");
					return result;
				}
			}

			public static object Invoke(System.Reflection.MethodBase method, object obj, object[] parameters)
			{ return method.Invoke(obj, parameters); }
		}

		#endregion
	}
}
#endif
