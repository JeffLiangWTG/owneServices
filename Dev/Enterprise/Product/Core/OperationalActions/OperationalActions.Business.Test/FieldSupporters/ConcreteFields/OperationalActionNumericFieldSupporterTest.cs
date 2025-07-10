using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionNumericFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionNumericFieldSupporter supporter = new OperationalActionNumericFieldSupporter("fieldName", false, 0m, 0m, 3, 0);
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", Array.Empty<Type>(), Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			OperationalActionNumericFieldSupporter supporter = new OperationalActionNumericFieldSupporter("FieldName", false, decimal.MinValue, decimal.MaxValue, 9, 3);
			AssertEquals("5.556", supporter.AsFilterString((ZDecimal)5.5555m, Factory));
			AssertEquals("5.25", supporter.AsFilterString((ZDecimal)5.25m, Factory));
			AssertEquals("5", supporter.AsFilterString((ZDecimal)5m, Factory));
			AssertEquals("4", supporter.AsFilterString((ZInt)4, Factory));
			AssertEquals("3", supporter.AsFilterString((ZShort)3, Factory));
			AssertEquals("2", supporter.AsFilterString((ZByte)2, Factory));
		}

		public void TestIgnoreDecimalPrecisionCheckDefaultValue()
		{
			OperationalActionNumericFieldSupporter supporter = new OperationalActionNumericFieldSupporter("FieldName", false, decimal.MinValue, decimal.MaxValue, 9, 3);
			OperationalActionNumericFieldSupporter supporterSetIgnoreDecimalPrecisionCheck = new OperationalActionNumericFieldSupporter("FieldName", false, decimal.MinValue, decimal.MaxValue, 9, 3)
			{ IgnoreDecimalPrecisionCheck = true };
			AssertEquals(false, supporter.IgnoreDecimalPrecisionCheck);
			AssertEquals(true, supporterSetIgnoreDecimalPrecisionCheck.IgnoreDecimalPrecisionCheck);
		}
	}
}
