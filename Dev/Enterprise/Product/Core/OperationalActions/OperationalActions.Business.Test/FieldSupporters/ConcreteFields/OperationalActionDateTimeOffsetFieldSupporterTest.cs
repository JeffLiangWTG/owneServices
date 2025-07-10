using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionDateTimeOffsetFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionDateTimeOffsetFieldSupporter supporter = new OperationalActionDateTimeOffsetFieldSupporter("fieldName", false);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedDateTimeOffsetFieldDefaultingStrategy), typeof(RelativeDateTimeOffsetFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			OperationalActionDateTimeOffsetFieldSupporter supporter = new OperationalActionDateTimeOffsetFieldSupporter("FieldName", false);
			AssertEquals("05-Feb-81 12:18", supporter.AsFilterString(new ZDateTime(1981, 02, 05, 12, 18, 00), Factory));
			AssertEquals("17-May-69 20:00", supporter.AsFilterString(new ZDateTime(1969, 05, 17, 20, 00, 00), Factory));
			AssertEquals("05-Feb-81 12:18", supporter.AsFilterString(new ZDateTimeOffset(1981, 02, 05, 12, 18, 30, TimeSpan.FromHours(10)), Factory));
			AssertEquals("17-May-69 20:00", supporter.AsFilterString(new ZDateTimeOffset(1969, 05, 17, 20, 00, 40, TimeSpan.FromHours(-10)), Factory));
		}
	}
}
