using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionTextFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionTextFieldSupporter supporter = new OperationalActionTextFieldSupporter("fieldName", false, 15);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedTextFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			OperationalActionTextFieldSupporter supporter = new OperationalActionTextFieldSupporter("FieldName", false, 35);
			AssertEquals("Test String", supporter.AsFilterString((ZString)"Test String", Factory));
		}
	}
}
