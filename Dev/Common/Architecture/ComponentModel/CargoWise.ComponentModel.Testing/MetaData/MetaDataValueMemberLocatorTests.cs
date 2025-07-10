#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class MetaDataValueMemberLocatorTests : TestCase
	{
		static MetaDataValueMemberLocatorTests()
		{
			MockMetaDataType.RegisterTypes();
		}

		public void TestWithBothValueAndMemberMetaData()
		{
			PropertyDescriptor propertyWithMetaDataValue = KPropertyDescriptorCollection.FromType(typeof(TestComponentWithBothValueAndMemberMetaData), true)["PropertyWithMetaDataValue"];
			PropertyDescriptor propertyWithMetaDataMember = KPropertyDescriptorCollection.FromType(typeof(TestComponentWithBothValueAndMemberMetaData), true)["PropertyWithMetaDataMember"];
			MetaDataValueMemberLocator locatorWithMetaDataValue =
				MetaDataValueMemberLocator.GetInstance(propertyWithMetaDataValue, MetaDataTypes.MaxLength);
			MetaDataValueMemberLocator locatorWithMetaDataMembers =
				MetaDataValueMemberLocator.GetInstance(propertyWithMetaDataMember, MetaDataTypes.MaxLength);

			AssertEquals("A value was provided in the most concrete subclass", 2, locatorWithMetaDataValue.MetaDataValues.Length);
			AssertEquals("No member was provided in the most concrete subclass", 0, locatorWithMetaDataValue.MetaDataMembers.Length);
			AssertEquals("No value was provided in the most concrete subclass", 0, locatorWithMetaDataMembers.MetaDataValues.Length);
			AssertEquals("A member was provided in the most concrete subclass", 2, locatorWithMetaDataMembers.MetaDataMembers.Length);
		}

		#region GetMetaDataValue

		public void TestGetMetaDataValue()
		{
			TestComponent nullValue = (TestComponent)MetaDataValueMemberLocator.GetMetaDataValue(typeof(TestComponent), MetaDataTypes.Null);
			AssertEquals("Meta-data specified on a component", TestComponent.Null, nullValue);

			AssertEquals("DefaultValue is returned if meta-data not specified on the component", false, MetaDataValueMemberLocator.GetMetaDataValue(typeof(TestComponent), MetaDataTypes.ReadOnly));
		}

		#endregion

		#region MetaDataValues

		public void TestMetaDataValues_FromPropertyTypeFallbackToProperty()
		{
			PropertyDescriptor property = KPropertyDescriptorCollection.FromType(typeof(TestComponent))["Property"];
			MetaDataValueMemberLocator locator = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ListValueMember);
			AssertEquals("Value specified on property", 1, locator.MetaDataValues.Length);
			AssertEquals("Member on property type overridden by value on property", 0, locator.MetaDataMembers.Length);
			AssertEquals("ListValueMember_ValueOnProperty", locator.MetaDataValues[0]);
		}

		public void TestMetaDataValues_OnListDataSourceElementType_WithListAsMember()
		{
			TestComponentWithListMember component = new TestComponentWithListMember();
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)["PropertyWithList"];

			object[] displayMember = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ListDisplayMember).MetaDataValues;
			AssertEquals("element_display", displayMember[0]);

			object[] valueMember = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ListValueMember).MetaDataValues;
			AssertEquals("element_value", valueMember[0]);
		}

		public void TestMetaDataValues_OnListDataSourceElementType_WithListAsConstant()
		{
			PropertyDescriptor property = KPropertyDescriptorCollection.FromType(typeof(TestComponentWithListConstant), true)["PropertyWithList"];

			object[] displayMember = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ListDisplayMember).MetaDataValues;
			AssertEquals("element_display", displayMember[0]);

			object[] valueMember = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ListValueMember).MetaDataValues;
			AssertEquals("element_value", valueMember[0]);
		}

		public void TestMetaDataValues_ForReadOnly()
		{
			PropertyDescriptor property = KPropertyDescriptorCollection.FromType(typeof(TestComponent), true)["MemberWithConstantReadOnly"];
			MetaDataValueMemberLocator locator = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ReadOnly);
			object[] readOnlyValue = locator.MetaDataValues;
			AssertEquals("ReadOnly value specified", 1, readOnlyValue.Length);
			AssertEquals("ReadOnly constant value", true, readOnlyValue[0]);
		}

		#endregion

		#region MetaDataMembers

		public void TestMetaDataMembers_FromPropertyTypeFallbackToProperty()
		{
			PropertyDescriptor property = KPropertyDescriptorCollection.FromType(typeof(TestComponent))["Property"];
			MetaDataValueMemberLocator locator = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ListDisplayMember);
			AssertEquals("Value on property type overridden by member on property", 0, locator.MetaDataValues.Length);
			AssertEquals("Member specified on property", 1, locator.MetaDataMembers.Length);
			AssertEquals("ListDisplayMember_Member", locator.MetaDataMembers[0]);
		}

		public void TestMetaDataMembers_FromDefaultRegisteredMetaDataMembers()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(new TestComponent())["PropertyWithDefaultMetaDataMembers"];
			AssertEquals("From default member", "PropertyWithDefaultMetaDataMembers_ReadOnly", MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ReadOnly).MetaDataMembers[0]);
			AssertEquals("From default member", "PropertyWithDefaultMetaDataMembers_MaxLength", MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.MaxLength).MetaDataMembers[0]);
			AssertEquals("From default member", "PropertyWithDefaultMetaDataMembers_Description", MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.Description).MetaDataMembers[0]);
			AssertEquals("Wrong type", 0, MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.DecimalPlaces).MetaDataMembers.Length);
			AssertEquals("Default member not exist", 0, MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.DateTimeFormat).MetaDataMembers.Length);
		}

		public void TestMetaDataMembers_AttributeShouldOverrideDefaultMetaDataMembers()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestComponent))["AnotherProperty"];
			AssertEquals("Attribute should override the default", "AnotherProperty_UseThisMaxLengthInstead", MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.MaxLength).MetaDataMembers[0]);
		}

		#endregion

		#region IsMetaDataMemberMoreSpecificThanValue

		public void TestIsMetaDataMemberMoreSpecificThanValue()
		{
			TestComponent component = new TestComponent();
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)["ReadOnlyProperty"];
			MetaDataValueMemberLocator locator = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ReadOnly);

			AssertEquals(false, locator.IsMetaDataMemberMoreSpecificThanValue);

			TestSubComponent subComponent = new TestSubComponent();
			property = TypeDescriptor.GetProperties(subComponent)["ReadOnlyProperty"];
			locator = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ReadOnly);

			AssertEquals(true, locator.IsMetaDataMemberMoreSpecificThanValue);
		}

		public void TestIsMetaDataMemberMoreSpecificThanValue_WhenMetaDataSpecifiedInPropertyName()
		{
			TestComponentWithReadOnlyExpressedAsMethod component = new TestComponentWithReadOnlyExpressedAsMethod();
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)["ReadOnlyProperty"];
			MetaDataValueMemberLocator locator = MetaDataValueMemberLocator.GetInstance(property, MetaDataTypes.ReadOnly);
			AssertEquals(true, locator.IsMetaDataMemberMoreSpecificThanValue);
		}

		#endregion

		public void TestUseDescriptorAttribute()
		{
			var propertyCollection = KPropertyDescriptorCollection.FromType(typeof(TestObj));
			var property = propertyCollection[nameof(TestObj.Property)];

			var testProperty = new TestPropertyDescriptor(propertyCollection, property);
			var locator = MetaDataValueMemberLocator.GetInstance(testProperty, MetaDataTypes.ListDataSource);
			AssertEquals("TestList2", locator.MetaDataMembers[0]);
		}

		#region Test Classes

		class TestObj
		{
			public string Property
			{
				get { return property; }
				set { property = value; }
			}
			string property;
		}

		class TestPropertyDescriptor : KPropertyDescriptor
		{
			public TestPropertyDescriptor(KPropertyDescriptorCollection collection, PropertyDescriptor inner)
				: base(collection, inner)
			{ }

			public override AttributeCollection Attributes
			{
				get
				{
					if (attributes == null)
					{
						attributes = new AttributeCollection(new Attribute[] { new ListAttribute("TestList2") }.Union(base.Attributes.Cast<Attribute>()).ToArray());
					}
					return attributes;
				}
			}
			AttributeCollection attributes;
		}

		class TestSubComponent : TestComponent
		{
			#region ReadOnlyMember

			[MetaDataMember(MetaDataTypes.ReadOnly, "ReadOnlyProperty_ReadOnly_New")]
			public override string ReadOnlyProperty
			{
				get { return base.ReadOnlyProperty; }
			}

			protected bool ReadOnlyProperty_ReadOnly_New
			{
				get { return true; }
			}

			#endregion
		}

		class MockMetaDataType
		{
			public const string TestListType = "DMetaDataValueMemberLocator.Test";

			public static void RegisterTypes()
			{
				MetaDataType.RegisterMetaDataType(new MetaDataType(TestListType, typeof(IList), null));
			}
		}

		abstract class TestComponentWithBothValueAndMemberMetaDataBaseBaseBase
		{
			[MetaDataMember(MetaDataTypes.MaxLength, "Property_MaxLength_Decoy")]
			public abstract string PropertyWithMetaDataValue { get; }

			[MetaDataMember(MetaDataTypes.MaxLength, "Property_MaxLength_Decoy")]
			public abstract string PropertyWithMetaDataMember { get; }
		}

		abstract class TestComponentWithBothValueAndMemberMetaDataBaseBase : TestComponentWithBothValueAndMemberMetaDataBaseBaseBase
		{
			[MetaDataMember(MetaDataTypes.MaxLength, "Property_MaxLength")]
			public override abstract string PropertyWithMetaDataValue { get; }

			[MetaDataValue(MetaDataTypes.MaxLength, 10)]
			public override abstract string PropertyWithMetaDataMember { get; }
		}

		abstract class TestComponentWithBothValueAndMemberMetaDataBase : TestComponentWithBothValueAndMemberMetaDataBaseBase
		{
			// these are invalid configurations but MetaDataMembers / MetaDataValues
			// may return multiple members for code analysis purposes

			[MetaDataValue(MetaDataTypes.MaxLength, 1)]
			[MetaDataValue(MetaDataTypes.MaxLength, 2)]
			public override string PropertyWithMetaDataValue
			{ get { return null; } }

			[MetaDataMember(MetaDataTypes.MaxLength, "Property_MaxLength")]
			[MetaDataMember(MetaDataTypes.MaxLength, "Property_MaxLength2")]
			public override string PropertyWithMetaDataMember
			{ get { return null; } }
		}

		class TestComponentWithBothValueAndMemberMetaData : TestComponentWithBothValueAndMemberMetaDataBase
		{
		}

		[MetaDataValue(MetaDataTypes.Null, typeof(TestComponent), "Null")]
		class TestComponent : KComponent
		{
			public static readonly TestComponent Null = new TestComponent();

			[MetaDataValue(MetaDataTypes.ListValueMember, "ListValueMember_ValueOnProperty")]
			[MetaDataMember(MetaDataTypes.ListDisplayMember, "ListDisplayMember_Member")]
			public TestRelatedComponent Property
			{ get { return null; } }

			public string ListDisplayMember_Member
			{ get { return ""; } }

			#region PropertyWithDefaultMetaDataMembers

			public string PropertyWithDefaultMetaDataMembers
			{
				get { return ""; }
			}

			protected bool PropertyWithDefaultMetaDataMembers_ReadOnly
			{
				get { return true; }
			}

			protected int PropertyWithDefaultMetaDataMembers_MaxLength
			{
				get { return 67; }
			}

			protected string PropertyWithDefaultMetaDataMembers_DecimalPlaces
			{
				get { return "TWO WEEKS!"; }
			}

			public IDescription PropertyWithDefaultMetaDataMembers_Description
			{
				get { return null; }
			}

			#endregion

			#region AnotherProperty

			[MetaDataMember(MetaDataTypes.MaxLength, "AnotherProperty_UseThisMaxLengthInstead")]
			public string AnotherProperty
			{
				get { return ""; }
			}

			public int AnotherProperty_MaxLength
			{
				get { return 55; }
			}

			#endregion

			#region ReadOnlyMember

			[ReadOnly(true)]
			public virtual string ReadOnlyProperty
			{
				get { return ""; }
				set { }
			}

			#endregion

			#region MemberWithConstantReadOnly

			[ReadOnly(true)]
			public string MemberWithConstantReadOnly { get; set; }

			#endregion
		}

		[MetaDataValue(MetaDataTypes.ListValueMember, "ListValueMember_ValueOnPropertyType")]
		[MetaDataValue(MetaDataTypes.ListDisplayMember, "ListDisplayMember_ValueOnPropertyType")]
		class TestRelatedComponent
		{
		}

		class TestComponentWithListConstant
		{
			[MetaDataValue(MockMetaDataType.TestListType, typeof(TestComponentWithListConstant), "List")]
			public int PropertyWithList
			{ get { return 0; } }

			public static List<TestCollectionElement> List
			{ get { return new List<TestCollectionElement>(); } }
		}

		class TestComponentWithListMember : KComponent
		{
			public TestComponentWithListMember Self
			{ get { return this; } }

			[MetaDataMember(MockMetaDataType.TestListType, "Self.List")]
			public int PropertyWithList
			{ get { return 0; } }

			public List<TestCollectionElement> List
			{ get { return null; } }
		}

		[ValueDisplayMembers("element_value", "element_display")]
		class TestCollectionElement
		{
		}

		class TestComponentWithReadOnlyExpressedAsMethodBase : KComponent
		{
			[ReadOnly(true)]
			public int ReadOnlyProperty { get; set; }
		}

		class TestComponentWithReadOnlyExpressedAsMethod : TestComponentWithReadOnlyExpressedAsMethodBase
		{
			public bool ReadOnlyProperty_ReadOnly { get; set; }
		}

		#endregion
	}
}
#endif
