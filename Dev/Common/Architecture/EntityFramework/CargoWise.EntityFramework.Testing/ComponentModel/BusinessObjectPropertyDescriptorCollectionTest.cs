using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectPropertyDescriptorCollectionTest : NUnit.Framework.TestCase
	{
		public void TestTestBindingMetadataOnWrappedProperties()
		{
			var defaultDecimalPlaces = (int)MetaDataType.GetMetaDataType(MetaDataTypes.DecimalPlaces).DefaultValue;

			var dummy = new ParentObject();

			var propertyPathesWithExpectedDecimalPlaces = new Dictionary<string, int>();
			AssertBindingMetadataOnWrappedProperties(dummy, "Child1+Number", defaultDecimalPlaces);
			AssertBindingMetadataOnWrappedProperties(dummy, "Child2+Number", defaultDecimalPlaces);
			AssertBindingMetadataOnWrappedProperties(dummy, "Child2+OtherNumber", defaultDecimalPlaces);

			dummy.Child1 = new ChildObject1();
			dummy.Child2 = new ChildObject2();

			propertyPathesWithExpectedDecimalPlaces.Clear();
			AssertBindingMetadataOnWrappedProperties(dummy, "Child1+Number", 4);
			AssertBindingMetadataOnWrappedProperties(dummy, "Child2+Number", 5);
			AssertBindingMetadataOnWrappedProperties(dummy, "Child2+OtherNumber", 6);
		}

		void AssertBindingMetadataOnWrappedProperties(ParentObject parent, string propertyPath, int expectedDecimals)
		{
			AssertEquals(propertyPath, expectedDecimals, MetaData.GetDecimalPlaces(parent, parent.GetProperties()[propertyPath]));
		}

		#region Test classes

		[ProvideMetaDataProperty("DecimalPlaces", MetaDataTypes.DecimalPlaces)]
		class ParentObject : NonPersistentBusinessObject
		{
			public ChildObject1 Child1 { get; set; }
			public ChildObject2 Child2 { get; set; }

			// Following metadata should be ignored by wrapped properties on child elements

			[DecimalPlaces("NumberDecimalPlaces")]
			public ZDecimal Number { get; set; }

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public int NumberDecimalPlaces
			{
				get { return 1; }
			}

			public int GetDecimalPlaces(PropertyDescriptor property)
			{
				return 2;
			}
		}

		[ProvideMetaDataProperty("DecimalPlaces", MetaDataTypes.DecimalPlaces)]
		class ChildObject1 : NonPersistentBusinessObject
		{
			public ZDecimal Number { get; set; }

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public int GetDecimalPlaces(PropertyDescriptor property)
			{
				return 4;
			}
		}

		class ChildObject2 : NonPersistentBusinessObject
		{
			[DecimalPlaces("NumberDecimalPlaces")]
			public ZDecimal Number { get; set; }

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public int NumberDecimalPlaces
			{
				get { return 5; }
			}

			[DecimalPlaces(6)]
			public ZDecimal OtherNumber { get; set; }

			public ZPropertyInfo OtherNumberInfo
			{
				get { return GetZPropertyInfo(nameof(OtherNumber)); }
			}
		}

		#endregion
	}
}
