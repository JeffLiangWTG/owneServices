using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZIsReadOnlyPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestObsoleteZPropertyInfoReadOnlySetter()
		{
			AssertReadOnly("Initially", Dummy.PropertyInfo, false);
			Dummy.Property_ReadOnly = true;
			AssertReadOnly("When ReadOnly set to true", Dummy.PropertyInfo, true);
		}

		public void TestReadOnlyAttribute()
		{
			AssertReadOnly(Dummy.PropertyWithReadOnlyAttributeInfo, true);
		}

		public void TestObsoleteZPropertyInfoReadOnlySetInZPropertyInfoGetter()
		{
			AssertReadOnly(Dummy.PropertyWithReadOnlySetInInfoGetterInfo, true);
		}

		public void TestReadOnlyWrappedProperty_InnerInfoIsReadOnly()
		{
			AssertReadOnly("Initially", Dummy.WrappedPropertyInfo, false);
			Dummy.Property_ReadOnly = true;
			AssertReadOnly("When inner info is read only", Dummy.WrappedPropertyInfo, true);
		}

		public void TestReadOnlyWrappedProperty_WrappingInfoIsReadOnly()
		{
			AssertReadOnly("Initially", Dummy.WrappedPropertyInfo, false);
			Dummy.WrappedProperty_ReadOnly = true;
			AssertReadOnly("When wrapping info is read only", Dummy.WrappedPropertyInfo, true);

			AssertReadOnly(Dummy.ReadOnlyWrappedPropertyInfo, true);
		}

		public void TestReadOnly_WhenWrappedInnerBusinessObjectReadOnly()
		{
			WrappingBusinessObjectWithReadOnlyProperties dummy = Factory.New<WrappingBusinessObjectWithReadOnlyProperties>();
			PropertyDescriptor property = dummy.GetProperties()["RelatedObject+PropertyWithReadOnlyMemberAttribute"];
			AssertEquals(false, MetaData.GetReadOnly(dummy, property));

			dummy.ReadOnly = true;
			dummy.RelatedObject.ReadOnly = false;
			AssertEquals(true, MetaData.GetReadOnly(dummy, property));

			dummy.ReadOnly = false;
			dummy.RelatedObject.ReadOnly = true;
			AssertEquals(true, MetaData.GetReadOnly(dummy, property));
		}

		public void TestReadOnlyOnNullObject()
		{
			DummyBusinessObject dummy = Factory.GetNull<DummyBusinessObject>();
			Assert(dummy.ReadOnly);
			AssertEquals(true, dummy.Z0_DescriptionInfo.ReadOnly);
			AssertEquals(true, MetaData.GetReadOnly(dummy, dummy.Z0_DescriptionInfo.PropertyDescriptor));
			AssertEquals(true, ((IAccessBusinessObject)dummy).IsPropertyReadOnly(DummyBizoSchema.Constants.Z0_Description));
		}

		public void TestReadOnlyWrappedProperty_WithMetadataMethodProvider()
		{
			var parent = new ParentBusinessObjectWithMetadataMethodProvider();
			var property = parent.GetProperties()["Child+Property"];

			parent.PropertyReadOnlyValue = false;
			AssertEquals("Default value for null object", false, MetaData.GetReadOnly(parent, property));
			parent.PropertyReadOnlyValue = true;
			AssertEquals("Default value for null object", false, MetaData.GetReadOnly(parent, property));

			parent.Child = new ChildBusinessObjectWithMetadataMethodProvider();

			parent.PropertyReadOnlyValue = false;
			parent.Child.PropertyReadOnlyValue = false;
			AssertEquals("Should take value from the child", false, MetaData.GetReadOnly(parent, property));

			parent.PropertyReadOnlyValue = false;
			parent.Child.PropertyReadOnlyValue = true;
			AssertEquals("Should take value from the child", true, MetaData.GetReadOnly(parent, property));

			parent.PropertyReadOnlyValue = true;
			parent.Child.PropertyReadOnlyValue = false;
			AssertEquals("Should take value from the child", false, MetaData.GetReadOnly(parent, property));

			parent.PropertyReadOnlyValue = true;
			parent.Child.PropertyReadOnlyValue = true;
			AssertEquals("Should take value from the child", true, MetaData.GetReadOnly(parent, property));
		}

		#region Test Classes

		class WrappingBusinessObjectWithReadOnlyProperties : DummyBusinessObject
		{
			public WrappingBusinessObjectWithReadOnlyProperties(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public BusinessObjectWithReadOnlyProperties RelatedObject
			{
				get { return relatedObject ?? (relatedObject = Factory.New<BusinessObjectWithReadOnlyProperties>()); }
			}
			BusinessObjectWithReadOnlyProperties relatedObject;
		}

		class BusinessObjectWithReadOnlyProperties : DummyBusinessObject
		{
			public BusinessObjectWithReadOnlyProperties(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region Property

			public ZString Property { get; set; }

			public ZPropertyInfo PropertyInfo
			{
				get { return GetZPropertyInfo(nameof(Property)); }
			}

			bool fProperty_ReadOnly;

			public bool Property_ReadOnly
			{
				get => fProperty_ReadOnly;
				set
				{
					fProperty_ReadOnly = value;
					PropertyInfo.RefreshBinding();
					WrappedPropertyInfo.RefreshBinding();
				}
			}

			#endregion

			#region PropertyWithReadOnlyAttribute

			[ReadOnly(true)]
			public ZString PropertyWithReadOnlyAttribute { get; set; }

			public ZPropertyInfo PropertyWithReadOnlyAttributeInfo
			{
				get { return GetZPropertyInfo(nameof(PropertyWithReadOnlyAttribute)); }
			}

			#endregion

			#region PropertyWithReadOnlyMemberAttribute

			[ReadOnlyMember(nameof(PropertyWithReadOnlyMemberAttribute_ReadOnly))]
			public ZString PropertyWithReadOnlyMemberAttribute { get; set; }

			public bool PropertyWithReadOnlyMemberAttribute_ReadOnly { get; set; }

			#endregion

			#region PropertyWithReadOnlySetInInfoGetter

			[ReadOnly(true)]
			public ZString PropertyWithReadOnlySetInInfoGetter { get; set; }

			public ZPropertyInfo PropertyWithReadOnlySetInInfoGetterInfo => GetZPropertyInfo(nameof(PropertyWithReadOnlySetInInfoGetter));

			#endregion

			#region WrappedProperty

			[ReadOnlyMember(nameof(WrappedProperty_ReadOnly))]
			public ZPropertyInfo WrappedProperty { get; set; }

			public ZWrappedPropertyInfo WrappedPropertyInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(WrappedProperty), x => PropertyInfo); }
			}

			bool fWrappedProperty_ReadOnly;

			public bool WrappedProperty_ReadOnly
			{
				get => fWrappedProperty_ReadOnly || fProperty_ReadOnly;
				set { fWrappedProperty_ReadOnly = value; WrappedPropertyInfo.RefreshBinding(); }
			}

			#endregion

			#region ReadOnlyWrappedProperty

			public ZPropertyInfo ReadOnlyWrappedProperty { get; set; }

			protected bool ReadOnlyWrappedProperty_ReadOnly
			{
				get { return true; }
			}

			public ZWrappedPropertyInfo ReadOnlyWrappedPropertyInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(ReadOnlyWrappedProperty), x => PropertyWithReadOnlySetInInfoGetterInfo); }
			}

			#endregion
		}

		[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
		class ParentBusinessObjectWithMetadataMethodProvider : NonPersistentBusinessObject
		{
			public ChildBusinessObjectWithMetadataMethodProvider Child { get; set; }

			public bool GetPropertyReadOnly(PropertyDescriptor property)
			{
				return PropertyReadOnlyValue;
			}

			public bool PropertyReadOnlyValue { get; set; }
		}

		[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
		class ChildBusinessObjectWithMetadataMethodProvider : NonPersistentBusinessObject
		{
			public ZString Property { get; set; }

			public ZPropertyInfo PropertyInfo
			{
				get { return GetZPropertyInfo(nameof(Property)); }
			}

			public bool GetPropertyReadOnly(PropertyDescriptor property)
			{
				return PropertyReadOnlyValue;
			}

			public bool PropertyReadOnlyValue { get; set; }
		}

		#endregion

		#region Implementation

		void AssertReadOnly(ZPropertyInfo property, bool expectedReadOnly)
		{
			AssertReadOnly("", property, expectedReadOnly);
		}

		void AssertReadOnly(string message, ZPropertyInfo property, bool expectedReadOnly)
		{
			AssertEquals(message + "; ReadOnly from MetaData", expectedReadOnly, MetaData.GetReadOnly(property.BizObj, property.PropertyDescriptor));
			AssertEquals(message + "; ReadOnly from ZPropertyInfo must be consistent with meta-data", expectedReadOnly, property.ReadOnly);
		}

		BusinessObjectWithReadOnlyProperties Dummy
		{
			get { return dummy ?? (dummy = Factory.New<BusinessObjectWithReadOnlyProperties>()); }
		}
		BusinessObjectWithReadOnlyProperties dummy;

		#endregion
	}
}
