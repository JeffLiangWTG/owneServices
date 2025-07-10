using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DataProviders;

namespace Enterprise.DocumentEngine.Test.DataProviders.BusinessObject
{
	public class CompositeDataSourceFieldValueProviderTest : TestCaseWithFactory
	{
		public void TestGetFieldValue_WithEmptySource_ReturnsNull()
		{
			// Arrange
			var provider = new CompositeDataSourceFieldValueProvider(null, CreateFieldValueSources(), null);

			// Act
			var result = provider.GetFieldValue();

			// Assert
			AssertNull(result);
		}

		public void TestGetFieldValue_WithMultipleSources_ReturnsValue()
		{
			// Arrange
			var expectedResult = 123;

			var dummy1 = Factory.New<DummyBusinessObjectWithField>();

			var dummy2 = Factory.New<DummyBusinessObjectWithField>();
			dummy2.SetFieldValue(expectedResult);

			var provider = CreateProvider(CreateFieldValueSources(dummy1, dummy2));

			// Act
			var result = provider.GetFieldValue();

			// Assert
			AssertEquals(expectedResult, result);
		}

		public void TestGetFieldValue_WithDefaultValueSet_ReturnsDefault()
		{
			// Arrange
			var expectedResult = 0;

			var dummy1 = Factory.New<DummyBusinessObjectWithField>();

			var dummy2 = Factory.New<DummyBusinessObjectWithField>();
			dummy2.SetFieldValue(expectedResult);

			var provider = CreateProvider(CreateFieldValueSources(dummy1, dummy2));

			// Act
			var result = provider.GetFieldValue();

			// Assert
			AssertEquals(expectedResult, result);
		}

		public void TestGetFieldValue_WithSupportedSystemTypes_ReturnsValue()
		{
			ExecuteTest(nameof(DummyBusinessObject));
			ExecuteTest(int.MaxValue);
			ExecuteTest(decimal.MaxValue);
			ExecuteTest(DateTime.MaxValue);
			ExecuteTest(true);

			void ExecuteTest(object expectedResult)
			{
				// Arrange
				var dummy = Factory.New<DummyBusinessObjectWithField>();
				dummy.SetFieldValue(expectedResult);

				var provider = CreateProvider(CreateFieldValueSources(dummy));

				// Act
				var result = provider.GetFieldValue();

				// Assert
				AssertEquals(expectedResult, result);
			}
		}

		class DummyBusinessObjectWithField : DummyBusinessObject
		{
			public DummyBusinessObjectWithField(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetFieldValue(object field)
			{
				this.field = field;
			}

			public object GetFieldValue()
			{
				return field;
			}

			object field;

			public static readonly MethodInfo GetFieldValueMethod = typeof(DummyBusinessObjectWithField).GetMethod(nameof(GetFieldValue));
		}

		static CompositeDataSourceFieldValueProvider CreateProvider(IEnumerable<(MethodInfoChainLink[] methodInfoChain, object dataSource)> fieldSources)
		{
			var provider = new CompositeDataSourceFieldValueProvider(nameof(DummyBusinessObjectWithField.GetFieldValue), fieldSources, (s, o, c) => GetFieldValue(o));

			return provider;
		}

		static object GetFieldValue(object instance)
		{
			return ((DummyBusinessObjectWithField)instance).GetFieldValue();
		}

		static IEnumerable<(MethodInfoChainLink[] methodInfoChain, object dataSource)> CreateFieldValueSources(params DummyBusinessObjectWithField[] instances)
		{
			return instances.Select(o => (new[] { new MethodInfoChainLink(DummyBusinessObjectWithField.GetFieldValueMethod) }, (object)o));
		}
	}
}
