using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using WTG.Rules.Engine;

namespace CargoWise.EntityFramework.Business.Rules.Testing
{
	sealed class BusinessObjectRuleDependancyTests : TestCaseWithFactory
	{
		public void TestCorrectValueForSingleProperty()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_NVarChar = "Hello world";

			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>(nameof(dummy.Z0_NVarChar));

			RuleDependencyResult<ZString> value;
			Assert("When the property can be retrieved the TryGet should return true", dependancy.TryGetValue(dummy, out value));
			AssertEquals("The result should be the value of the property", "Hello world", value.Value);
		}

		public void TestLastElementOfChainIsNull()
		{
			var parent = Factory.NewWithValidTestData<DummyBusinessObject>();
			parent.Z0_Guid = Factory.NewWithValidTestData<DummyBusinessObject>().PK;
			parent.RelatedDummy.Z0_Guid = Guid.Empty;

			AssertNull("PRE: Is null but valid", parent.RelatedDummy.RelatedDummy);

			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>("RelatedDummy.RelatedDummy");

			RuleDependencyResult<DummyBusinessObject> result;
			Assert("Should return true - the value was correctly recieved", dependancy.TryGetValue(parent, out result));
			AssertNull("Should contain the null bizo", result.Value);
		}

		public void TestCorrectValueForMultipleHops()
		{
			var relatedDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			relatedDummy.Z0_NVarChar = "Hello world";

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Guid = relatedDummy.PK;

			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>("RelatedDummy.Z0_NVarChar");

			RuleDependencyResult<ZString> value;
			Assert("When the property can be retrieved the TryGet should return true", dependancy.TryGetValue(dummy, out value));
			AssertEquals("The result should be the value of the property", "Hello world", value.Value);
		}

		public void TestReturnsTheCorrectValueTwice()
		{
			//Making sure there isnt any hyper-aggressive caching
			var relatedDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			relatedDummy.Z0_NVarChar = "Hello world";

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Guid = relatedDummy.PK;

			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>("RelatedDummy.Z0_NVarChar");

			RuleDependencyResult<ZString> value;
			Assert("When the property can be retrieved the TryGet should return true", dependancy.TryGetValue(dummy, out value));
			AssertEquals("The result should be the value of the property", "Hello world", value.Value);

			relatedDummy.Z0_NVarChar = "Goodbye world";

			Assert("When the property can be retrieved the TryGet should return true", dependancy.TryGetValue(dummy, out value));
			AssertEquals("The result should be the value of the property", "Goodbye world", value.Value);
		}

		public void TestCorrectValueForInvalidProperty()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			AssertExceptionThrown<ArgumentException>(() => new BusinessObjectRuleDependancy<DummyBusinessObject>("XX_DoesntExist"));
		}

		public void TestCorrectValueForPartiallyValidProperty_PropertyDoesntExist()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			AssertExceptionThrown<ArgumentException>(() => new BusinessObjectRuleDependancy<DummyBusinessObject>("RelatedDummy.XX_DoesntExist"));
		}

		public void TestCorrectValueForPartiallyValidProperty_PropertyIsNull()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>("RelatedDummy.Z0_NVarChar");

			RuleDependencyResult<ZString> value;
			Assert("If one property in the chain is null, the TryGet should return false", !dependancy.TryGetValue(dummy, out value));
		}

		public void TestGetValueAsyncIsntActuallyAsync()
		{
			var relatedDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			relatedDummy.Z0_NVarChar = "Hello world";

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Guid = relatedDummy.PK;

			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>("RelatedDummy.Z0_NVarChar");
			var task = dependancy.GetValueAsync<ZString>(dummy);

			Assert("Task should be already done since it was never async", task.IsCompleted);
			AssertEquals("Result should be the actual result", "Hello world", task.Result.Value);
		}

		class DummyWithOverridenProperty : DummyBusinessObject
		{
			public DummyWithOverridenProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString Z0_NVarChar => "New Value";
		}

		public void TestCorrectValueUsesStaticType()
		{
			var dummy = Factory.New<DummyWithOverridenProperty>();
			var cast = (DummyBusinessObject)dummy;
			cast.Z0_NVarChar = "Original";

			AssertNotEquals("PRE: Should not be equal", cast.Z0_NVarChar, dummy.Z0_NVarChar);

			var dependancy = new BusinessObjectRuleDependancy<DummyBusinessObject>("Z0_NVarChar");

			RuleDependencyResult<ZString> result;
			dependancy.TryGetValue(dummy, out result);
			AssertEquals("It should use the type of the type param, NOT the type of passed in BusinessObject", cast.Z0_NVarChar, result.Value);
		}
	}
}
