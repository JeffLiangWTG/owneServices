using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataBoundBusinessObjectTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DataBoundBusinessObject(businessObject: null));
		}

		public void TestHasProperty()
		{
			var bo = Factory.New<DummyBusinessObject>();
			IDataBoundBusinessObject valuesProvider = new DataBoundBusinessObject(bo);
			AssertEquals("Invalid Property", false, valuesProvider.HasProperty("TestProperty"));
			AssertEquals("Z0_Calculated", true, valuesProvider.HasProperty("Z0_Calculated"));
		}

		public void TestTryGetValue()
		{
			var bo = Factory.New<DummyBusinessObject>();
			IDataBoundBusinessObject valuesProvider = new DataBoundBusinessObject(bo);

			var result = valuesProvider.TryGetValue<ZInt>("TestProperty", out var testPropertyValue);
			AssertEquals("TryGetValue Result", false, result);
			AssertEquals("Default when invalid property name", 0, testPropertyValue);

			bo.Z0_Number = 100;
			result = valuesProvider.TryGetValue<ZInt>("Z0_Number", out var zNumberPropertyValue);
			AssertEquals("TryGetValue Result", true, result);
			AssertEquals("When Value is 100", 100, zNumberPropertyValue);

			result = valuesProvider.TryGetValue<ZDateTime>("Z0_Number", out var _);
			AssertEquals("TryGetValue Result", false, result);
		}
	}
}
