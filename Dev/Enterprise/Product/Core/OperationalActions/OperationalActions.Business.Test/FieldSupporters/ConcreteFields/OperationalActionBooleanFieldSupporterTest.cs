using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionBooleanFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionBooleanFieldSupporter supporter = new OperationalActionBooleanFieldSupporter("fieldName", false);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedBooleanFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			OperationalActionBooleanFieldSupporter supporter = new OperationalActionBooleanFieldSupporter("FieldName", false);
			AssertEquals("Y", supporter.AsFilterString(ZBool.True, Factory));
			AssertEquals("N", supporter.AsFilterString(ZBool.False, Factory));
		}
	}
}
