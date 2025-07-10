#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KPropertyDescriptorCollectionTests : ThreadSafeAccessTestCase
	{
		#region AllProperties / PropertiesWithNoGenericIncompletePropertyTypes

		public void TestAllProperties_ShouldReturnItself()
		{
			KPropertyDescriptorCollection allPropertiesCollection = new KPropertyDescriptorCollection(typeof(KComponent), true);
			AssertEquals("Should return itself", allPropertiesCollection, allPropertiesCollection.AllProperties);
		}

		public void TestPropertiesWithNoGenericIncompletePropertyTypes_ForTypeWithUndefinedGenericParameter()
		{
			TestPropertiesWithNoGenericIncompletePropertyTypes(typeof(TestComponentWithGenericParameter<>), false);
		}

		public void TestPropertiesWithNoGenericIncompletePropertyTypes_ForTypeWithPopulatedGenericParameter()
		{
			TestPropertiesWithNoGenericIncompletePropertyTypes(typeof(TestComponentWithGenericParameter<int>), true);
		}

		void TestPropertiesWithNoGenericIncompletePropertyTypes(Type componentTypeWithGenericParameter, bool genericParameterPopulated)
		{
			KPropertyDescriptorCollection collection = new KPropertyDescriptorCollection(componentTypeWithGenericParameter, false).AllProperties;

			PropertyDescriptor normalProperty = collection["Property"];
			PropertyDescriptor propertyWithGenericPropertyType1 = collection["PropertyWithGenericPropertyType1"];
			PropertyDescriptor propertyWithGenericPropertyType2 = collection["PropertyWithGenericPropertyType2"];

			AssertEquals("Property with non-generic PropertyType", true, ((IList)collection.PropertiesWithNoGenericParametersOnPropertyTypes).Contains(normalProperty));
			AssertEquals("Property with generic PropertyType", genericParameterPopulated, ((IList)collection.PropertiesWithNoGenericParametersOnPropertyTypes).Contains(propertyWithGenericPropertyType1));
			AssertEquals("Property with PropertyType with a generic parameter", genericParameterPopulated, ((IList)collection.PropertiesWithNoGenericParametersOnPropertyTypes).Contains(propertyWithGenericPropertyType2));
		}

		#endregion

		public void TestFindAndRemove()
		{
			AssertNotNull("If KPropertyDescriptorCollection.CachedFoundPropertiesField is null then we should remove the reflection code as .Net has changed.", KPropertyDescriptorCollection.CachedFoundPropertiesField);
			var collection = new PropertyDescriptorCollectionWithMetaData(typeof(TestComponent), false);
			var o = new TestComponent();
			var oCollection = (KPropertyDescriptorCollection)TypeDescriptor.GetProperties(o);
			var property1 = new KPropertyDescriptor(oCollection, oCollection[nameof(TestComponent.Property1)]);
			collection.Add(property1);
			AssertEquals("collection.Count", 1, collection.Count);
			var property2Name = nameof(TestComponent.Property2);
			var property2 = new KPropertyDescriptor(oCollection, oCollection[property2Name]);
			collection.Add(property2);
			var property3Name = nameof(TestComponent.Property3);
			var property3 = new KPropertyDescriptor(oCollection, oCollection[property3Name]);
			collection.Add(property3);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertSame("collection.Find(property2Name, false)", property2, collection.Find(property2Name, false));
			AssertSame("collection[property2Name]", property2, collection[property2Name]);
			AssertSame("collection.Find(property3Name, false)", property3, collection.Find(property3Name, false));
			AssertSame("collection[property3Name]", property3, collection[property3Name]);
			var index3 = collection.IndexOf(property3);
			collection.RemoveAt(index3);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertNull("collection.Find(property3Name, false)", collection.Find(property3Name, false));
			AssertNull("collection[property3Name]", collection[property3Name]);
			collection.Remove(property2);
			AssertNull("collection.Find(property2Name, false)", collection.Find(property2Name, false));
			AssertNull("collection[property2Name]", collection[property2Name]);
		}

		[ExpectNoExceptions]
		public void TestIndexerThreadSafety()
		{
			PropertyDescriptorCollectionWithMetaData collection = new PropertyDescriptorCollectionWithMetaData(typeof(TestComponent), false);
			collection.PopulatePropertyDescriptors();
			Exception lastException = null;
			try
			{
				for (int i = 1; i <= 99; i++)
				{
					string propertyName = "Property" + i;
					KPropertyDescriptor property = (KPropertyDescriptor)collection.AllProperties[propertyName];
					AssertEquals(propertyName, property.Name);
					AssertEquals(null, collection["NotExists" + i]);
					KPropertyDescriptor readOnlyProperty = collection.GetMetaDataProperty(typeof(TestComponent), property, MetaDataTypes.ReadOnly);
					AssertNotNull(readOnlyProperty);
				}

				PropertyDescriptorCollectionWithMetaData newCollection = new PropertyDescriptorCollectionWithMetaData(typeof(TestComponent), false);
				newCollection.PopulatePropertyDescriptors();
				collection = newCollection;
			}
			catch (Exception ex)
			{
				lastException = ex;
			}
			if (lastException != null)
			{
				throw lastException;
			}
		}

		public void TestPopulatePropertyDescriptorsShouldNotPopulateMoreThanOnce()
		{
			KPropertyDescriptorCollection collection = new KPropertyDescriptorCollection(typeof(KComponent), false);
			AssertEquals("Sanity check", 0, collection.Count);

			collection.PopulatePropertyDescriptors();
			AssertEquals("Should populate with public properties", TypeDescriptor.GetProperties(typeof(KComponent)).Count, collection.Count);

			collection.PopulatePropertyDescriptors();
			AssertEquals("Should not populate again", TypeDescriptor.GetProperties(typeof(KComponent)).Count, collection.Count);
		}

		#region Test Classes

		internal class TestComponent : KComponent
		{
			public string Property1 { get; set; }
			public string Property2 { get; set; }
			public string Property3 { get; set; }
			public string Property4 { get; set; }
			public string Property5 { get; set; }
			public string Property6 { get; set; }
			public string Property7 { get; set; }
			public string Property8 { get; set; }
			public string Property9 { get; set; }
			public string Property10 { get; set; }
			public string Property11 { get; set; }
			public string Property12 { get; set; }
			public string Property13 { get; set; }
			public string Property14 { get; set; }
			public string Property15 { get; set; }
			public string Property16 { get; set; }
			public string Property17 { get; set; }
			public string Property18 { get; set; }
			public string Property19 { get; set; }
			public string Property20 { get; set; }
			public string Property21 { get; set; }
			public string Property22 { get; set; }
			public string Property23 { get; set; }
			public string Property24 { get; set; }
			public string Property25 { get; set; }
			public string Property26 { get; set; }
			public string Property27 { get; set; }
			public string Property28 { get; set; }
			public string Property29 { get; set; }
			public string Property30 { get; set; }
			public string Property31 { get; set; }
			public string Property32 { get; set; }
			public string Property33 { get; set; }
			public string Property34 { get; set; }
			public string Property35 { get; set; }
			public string Property36 { get; set; }
			public string Property37 { get; set; }
			public string Property38 { get; set; }
			public string Property39 { get; set; }
			public string Property40 { get; set; }
			public string Property41 { get; set; }
			public string Property42 { get; set; }
			public string Property43 { get; set; }
			public string Property44 { get; set; }
			public string Property45 { get; set; }
			public string Property46 { get; set; }
			public string Property47 { get; set; }
			public string Property48 { get; set; }
			public string Property49 { get; set; }
			public string Property50 { get; set; }
			public string Property51 { get; set; }
			public string Property52 { get; set; }
			public string Property53 { get; set; }
			public string Property54 { get; set; }
			public string Property55 { get; set; }
			public string Property56 { get; set; }
			public string Property57 { get; set; }
			public string Property58 { get; set; }
			public string Property59 { get; set; }
			public string Property60 { get; set; }
			public string Property61 { get; set; }
			public string Property62 { get; set; }
			public string Property63 { get; set; }
			public string Property64 { get; set; }
			public string Property65 { get; set; }
			public string Property66 { get; set; }
			public string Property67 { get; set; }
			public string Property68 { get; set; }
			public string Property69 { get; set; }
			public string Property70 { get; set; }
			public string Property71 { get; set; }
			public string Property72 { get; set; }
			public string Property73 { get; set; }
			public string Property74 { get; set; }
			public string Property75 { get; set; }
			public string Property76 { get; set; }
			public string Property77 { get; set; }
			public string Property78 { get; set; }
			public string Property79 { get; set; }
			public string Property80 { get; set; }
			public string Property81 { get; set; }
			public string Property82 { get; set; }
			public string Property83 { get; set; }
			public string Property84 { get; set; }
			public string Property85 { get; set; }
			public string Property86 { get; set; }
			public string Property87 { get; set; }
			public string Property88 { get; set; }
			public string Property89 { get; set; }
			public string Property90 { get; set; }
			public string Property91 { get; set; }
			public string Property92 { get; set; }
			public string Property93 { get; set; }
			public string Property94 { get; set; }
			public string Property95 { get; set; }
			public string Property96 { get; set; }
			public string Property97 { get; set; }
			public string Property98 { get; set; }
			public string Property99 { get; set; }
		}

		class TestComponentWithGenericParameter<T> : KComponent
		{
			public int Property
			{ get { return 0; } }

			public T PropertyWithGenericPropertyType1
			{ get { return default(T); } }

			public List<T> PropertyWithGenericPropertyType2
			{ get { return null; } }
		}

		#endregion
	}
}
#endif
