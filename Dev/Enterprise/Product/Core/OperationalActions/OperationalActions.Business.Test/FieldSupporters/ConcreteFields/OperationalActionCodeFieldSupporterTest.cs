using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionCodeFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionCodeFieldSupporter supporter = new OperationalActionCodeFieldSupporter("fieldName", false, new CodeDescriptionPairList());
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedCodeFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ALP", "Alpha");
			list.AddPair("BET", "Beta");
			OperationalActionCodeFieldSupporter supporter = new OperationalActionCodeFieldSupporter("FieldName", false, list);
			AssertEquals("ALP", supporter.AsFilterString((ZString)"ALP", Factory));
			AssertEquals("BET", supporter.AsFilterString((ZString)"BET", Factory));
			AssertEquals("", supporter.AsFilterString((ZString)"", Factory));
		}
	}
}
