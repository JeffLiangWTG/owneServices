using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionNKModuleFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionNKModuleFieldSupporter supporter = new OperationalActionNKModuleFieldSupporter("fieldName", false, 5, (f) => new RefUNLOCOCollection(f));
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedNKModuleFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			OperationalActionNKModuleFieldSupporter supporter = new OperationalActionNKModuleFieldSupporter("FieldName", false, 5, (f) => new RefUNLOCOCollection(f));
			AssertEquals("AUBNE", supporter.AsFilterString((ZString)"AUBNE", Factory));
			AssertEquals("AUBNE", supporter.AsFilterString((ZString)"AUBNE", Factory));
			AssertEquals("", supporter.AsFilterString((ZString)"", Factory));
		}
	}
}
