using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionDateTimeFieldSupporterTest : TestCaseWithFactory
	{
		public void TestLongDefaultStrategies()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Long);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedDateFieldDefaultingStrategy), typeof(RelativeDateFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestShortDefaultStrategies()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Short);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedDateFieldDefaultingStrategy), typeof(RelativeDateFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestTimeDefaultStrategies()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("fieldName", false, ZDateTimePickerFormat.Time);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("No supported defaulting strategies", Array.Empty<Type>(), Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestLongAsFilterStringCore()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("FieldName", false, ZDateTimePickerFormat.Long);
			AssertEquals("05-Feb-81 12:18", supporter.AsFilterString(new ZDateTime(1981, 02, 05, 12, 18, 00), Factory));
			AssertEquals("17-May-69 20:00", supporter.AsFilterString(new ZDateTime(1969, 05, 17, 20, 00, 00), Factory));
		}

		public void TestShortAsFilterStringCore()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("FieldName", false, ZDateTimePickerFormat.Short);
			AssertEquals("05-Feb-81", supporter.AsFilterString(new ZDateTime(1981, 02, 05, 12, 18, 00), Factory));
			AssertEquals("17-May-69", supporter.AsFilterString(new ZDateTime(1969, 05, 17, 20, 00, 00), Factory));
		}

		public void TestTimeAsFilterStringCore()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("FieldName", false, ZDateTimePickerFormat.Time);
			AssertEquals("12:18", supporter.AsFilterString(new ZDateTime(1900, 1, 1, 12, 18, 00), Factory));
			AssertEquals("20:00", supporter.AsFilterString(new ZDateTime(1900, 1, 1, 20, 00, 00), Factory));
		}
	}
}
