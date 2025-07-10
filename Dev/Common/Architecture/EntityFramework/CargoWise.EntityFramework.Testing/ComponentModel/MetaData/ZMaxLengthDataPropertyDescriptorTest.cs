using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZMaxLengthDataPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestMaxLengthAttribute()
		{
			AssertMaxLength(Dummy.PropertyWithMaxLengthAttributeInfo, 10);
		}

		public void TestMaxLengthWrappedProperty_FromWrappingInfo()
		{
			AssertMaxLength(Dummy.WrappedPropertyWithMaxLengthInfo, 10);
		}

		public void TestMaxLengthFallsBackToDatabaseColumn()
		{
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.MaxLength, Dummy.Z0_VarCharMaxInfo.MaxLength);

			var dummy2 = Factory.New<DummyDependantBusinessObjectWithTablePrefixOverriden>();
			AssertEquals(DummyDependentBizoSchema.ZD1_Code.MaxLength, dummy2.ZD1_CodeInfo.MaxLength);
		}

		public void TestMaxLengthFallsBackToAddInfoColumn()
		{
			var dummy = Factory.New<DummyZZBizo>();
			AssertEquals(ZZDummyBizoSchema.Z0_AddInfoString35.MaxLength, dummy.Z0_AddInfoString35Info.MaxLength);
		}

		#region Test Classes

		class BusinessObjectWithPropertiesOfMaxLength : DummyBusinessObject
		{
			public BusinessObjectWithPropertiesOfMaxLength(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				Z0_Code_MaxLength = -1;
			}

			protected internal int Z0_Code_MaxLength { get; set; }

			#region Property

			public ZString Property { get; set; }

			public ZPropertyInfo PropertyInfo
			{
				get { return GetZPropertyInfo(nameof(Property)); }
			}

			#endregion

			#region PropertyWithMaxLengthAttribute

			[MaxLength(10)]
			public ZString PropertyWithMaxLengthAttribute { get; set; }

			public ZPropertyInfo PropertyWithMaxLengthAttributeInfo
			{
				get { return GetZPropertyInfo(nameof(PropertyWithMaxLengthAttribute)); }
			}

			#endregion

			#region WrappedProperty

			public ZPropertyInfo WrappedProperty { get; set; }

			public ZWrappedPropertyInfo WrappedPropertyInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(WrappedProperty), x => PropertyInfo); }
			}

			#endregion

			#region WrappedPropertyWithMaxLength

			public ZPropertyInfo WrappedPropertyWithMaxLength { get; set; }

			protected int WrappedPropertyWithMaxLength_MaxLength
			{
				get { return 10; }
			}

			public ZWrappedPropertyInfo WrappedPropertyWithMaxLengthInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(WrappedPropertyWithMaxLength), x => PropertyInfo); }
			}

			#endregion
		}

		class DummyDependantBusinessObjectWithTablePrefixOverriden : DummyDependantBusinessObject
		{
			public DummyDependantBusinessObjectWithTablePrefixOverriden(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override string TablePrefix
			{
				get { return base.TablePrefix + "_"; }
			}
		}

		#endregion

		#region Implementation

		void AssertMaxLength(ZPropertyInfo property, int expectedMaxLength)
		{
			AssertMaxLength("", property, expectedMaxLength);
		}

		void AssertMaxLength(string message, ZPropertyInfo property, int expectedMaxLength)
		{
			AssertEquals(message + "; MaxLength from MetaData", expectedMaxLength, MetaData.GetMaxLength(property.BizObj, property.PropertyDescriptor));
			AssertEquals(message + "; MaxLength from ZPropertyInfo must be consistent with meta-data", expectedMaxLength, property.MaxLength);
		}

		BusinessObjectWithPropertiesOfMaxLength Dummy
		{
			get { return dummy ?? (dummy = Factory.New<BusinessObjectWithPropertiesOfMaxLength>()); }
		}
		BusinessObjectWithPropertiesOfMaxLength dummy;

		#endregion
	}
}
