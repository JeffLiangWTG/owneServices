using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionDateFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			var supporter = new OperationalActionDateFieldSupporter("fieldName", false);
			var strategies = supporter.GetDefaultingStrategies();
			var actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedDateOnlyFieldDefaultingStrategy), typeof(RelativeDateOnlyFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			var supporter = new OperationalActionDateFieldSupporter("FieldName", false);
			AssertEquals("05-Feb-81", supporter.AsFilterString(new ZDate(1981, 02, 05), Factory));
			AssertEquals("17-May-69", supporter.AsFilterString(new ZDate(1969, 05, 17), Factory));
		}
	}
}
