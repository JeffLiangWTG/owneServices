using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PropertyComparerTest : TestCaseWithFactory
	{
		public void TestConstructorTranslatesPropertyNameToDescriptor()
		{
			PropertyComparer comparer = new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Constants.Z0_Code, ListSortDirection.Ascending);
			AssertEquals("Z0_Code", comparer.PropertyDescriptor.DisplayName);
		}

		public void TestZGuidPropertyComparerInvalidZGuid()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();

			var comparer = new ZGuidPropertyComparer(dummy1.ZPropertyInfoHash[DummyBizoSchema.Z0_Guid.Name], ListSortDirection.Ascending, "Z0_Date"); //Z0_Date can be any valid property, just need something ignorable

			AssertEquals("", comparer.GetPropertyValueFromObject(dummy1));
		}

		public void TestEquals()
		{
			PropertyComparer comparer1 = new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending);
			PropertyComparer comparer2 = new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending);
			PropertyComparer comparer3 = new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			AssertEquals("When equal", true, comparer1.Equals(comparer1));
			AssertEquals("When not equal", false, comparer2.Equals(comparer3));
			AssertEquals("When not equal to non-comparer type", false, comparer2.Equals("IncompatibleType"));
		}

		public void TestCompareWithDifferentTypes_ShouldReportAndDoStringComparison()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();

			var propertyDescriptor = new PropertyDescriptorForTest(typeof(DummyBusinessObject), DummyBizoSchema.Constants.Z0_Code, typeof(ZString));
			var comparer = new PropertyComparer(propertyDescriptor, ListSortDirection.Ascending);

			propertyDescriptor.SetupGetValueResult(dummy1, new ZString("1"));
			propertyDescriptor.SetupGetValueResult(dummy2, new ZInt(1));
			propertyDescriptor.SetupGetValueResult(dummy3, new ZString("2"));

			Assert(comparer.Compare(dummy1, dummy2) == 0);
			AssertEquals("Comparing valueFromX:ZString=[1] with valueFromY:ZInt=[1] for property CargoWise.EntityFramework.Testing.DummyBusinessObject.Z0_Code (ZString)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Assert(comparer.Compare(dummy2, dummy3) == -1);
			AssertEquals("Comparing valueFromX:ZInt=[1] with valueFromY:ZString=[2] for property CargoWise.EntityFramework.Testing.DummyBusinessObject.Z0_Code (ZString)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Assert(comparer.Compare(dummy3, dummy2) == 1);
			AssertEquals("Comparing valueFromX:ZString=[2] with valueFromY:ZInt=[1] for property CargoWise.EntityFramework.Testing.DummyBusinessObject.Z0_Code (ZString)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		class PropertyDescriptorForTest : ReflectPropertyDescriptor
		{
			internal PropertyDescriptorForTest(Type componentType, string propertyName, Type propertyType)
				: base(componentType, propertyName, propertyType, Array.Empty<Attribute>())
			{
			}

			internal void SetupGetValueResult(object component, object returnValue)
			{
				resultCache[component] = returnValue;
			}

			readonly Dictionary<object, object> resultCache = new Dictionary<object, object>();

			public override object GetValue(object component)
			{
				return resultCache[component];
			}
		}
	}
}
