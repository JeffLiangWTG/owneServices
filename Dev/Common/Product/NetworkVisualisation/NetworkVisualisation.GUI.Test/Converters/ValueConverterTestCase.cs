using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestsSubclassesOf(typeof(IValueConverter), RestrictedToAssemblies = new[] { "CargoWise.NetworkVisualisation.GUI" })]
	public abstract class ValueConverterTestCase<T> : ConverterTestBase
		where T : IValueConverter, new()
	{
		protected static void AssertConvertResult(object expectedResult, object value, object parameter = null)
		{
			var converter = new T();
			var result = converter.Convert(value, typeof(object), parameter, CultureInfo.CurrentCulture);

			AssertEquals(expectedResult, result);
		}

		protected static void AssertConvertBackResult(object expectedResult, object value, object parameter = null)
		{
			var converter = new T();
			var result = converter.ConvertBack(value, typeof(object), parameter, CultureInfo.CurrentCulture);

			AssertEquals(expectedResult, result);
		}

		protected static void AssertConvertResult(Predicate<object> expectedResultPredicate, object value, object parameter = null)
		{
			var converter = new T();
			var result = converter.Convert(value, typeof(object), parameter, CultureInfo.CurrentCulture);

			ConverterTestBase.AssertConvertResult(expectedResultPredicate, result);
		}

		protected static void AssertBrushConvertResult(Color expectedColor, object value)
		{
			var converter = new T();
			var result = (SolidColorBrush)converter.Convert(value, typeof(object), null, CultureInfo.CurrentCulture);

			AssertEquals(expectedColor, result.Color);
		}
	}

	[TestsSubclassesOf(typeof(IMultiValueConverter), RestrictedToAssemblies = new[] { "CargoWise.NetworkVisualisation.GUI" })]
	public abstract class MultiValueConverterTestCase<T> : ConverterTestBase
		where T : IMultiValueConverter, new()
	{
		protected static void AssertConvertResult(object expectedResult, object[] values, object parameter = null, string message = "")
		{
			var converter = new T();
			var result = converter.Convert(values, typeof(object), parameter, CultureInfo.CurrentCulture);

			AssertEquals(message, expectedResult, result);
		}

		protected static void AssertConvertResult(Predicate<object> expectedResultPredicate, object[] values, object parameter = null)
		{
			var converter = new T();
			var result = converter.Convert(values, typeof(object), parameter, CultureInfo.CurrentCulture);

			AssertConvertResult(expectedResultPredicate, result);
		}

		protected static void AssertBrushConvertResult(Color expectedColor, object[] values, string message = "")
		{
			var converter = new T();
			var result = (SolidColorBrush)converter.Convert(values, typeof(object), null, CultureInfo.CurrentCulture);

			AssertEquals(message, expectedColor, result.Color);
		}
	}

	public abstract class ConverterTestBase : TestCase
	{
		public abstract void TestConvert();

		protected static void AssertConvertResult(Predicate<object> expectedResultPredicate, object result)
		{
			if (expectedResultPredicate == null)
			{
				AssertNull(result);
			}
			else
			{
				Assert(expectedResultPredicate(result));
			}
		}
	}
}
