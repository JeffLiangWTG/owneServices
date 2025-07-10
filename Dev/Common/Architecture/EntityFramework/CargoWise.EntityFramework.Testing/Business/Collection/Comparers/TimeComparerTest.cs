using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TimeComparerTest : TestCaseWithFactory
	{
		public void TestCompareFromDifferentYears()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();

			var propertyDescriptor = new PropertyDescriptorForTest(typeof(DummyBusinessObject), DummyBizoSchema.Constants.Z0_SmallDateTime, typeof(ZDateTime));
			var comparer = new TimeComparer(propertyDescriptor, ListSortDirection.Ascending);

			propertyDescriptor.SetupGetValueResult(dummy1, new ZDateTime(2022, 1, 1, 0, 1, 0));
			propertyDescriptor.SetupGetValueResult(dummy2, new ZDateTime(2020, 1, 1, 0, 2, 0));
			propertyDescriptor.SetupGetValueResult(dummy3, new ZDateTime(2021, 1, 1, 0, 3, 0));

			AssertEquals(-1, comparer.Compare(dummy1, dummy2));
			AssertEquals(-1, comparer.Compare(dummy2, dummy3));
			AssertEquals(-1, comparer.Compare(dummy1, dummy3));
			AssertEquals(1, comparer.Compare(dummy2, dummy1));
			AssertEquals(1, comparer.Compare(dummy3, dummy2));
			AssertEquals(1, comparer.Compare(dummy3, dummy1));
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
