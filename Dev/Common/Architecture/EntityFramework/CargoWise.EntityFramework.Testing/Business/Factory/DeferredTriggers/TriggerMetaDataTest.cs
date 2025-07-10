using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TriggerMetaDataTest : TestCaseWithFactory
	{
		public void TestConstructorWithValidInput()
		{
			var attribute = new DeferTriggerAndRunBeforeCommitAttribute("foo", "bar", "baz", typeof(DeferTriggerOnDeleteConditionStrategy));
			var bizO1 = Factory.New<DummyBusinessObject>();
			var bizO2 = Factory.New<DummyBusinessObject>();
			var bizO3 = Factory.New<DummyBusinessObject>();

			var guids = new List<ZGuid>();
			guids.Add(bizO1.PK);
			guids.Add(bizO2.PK);
			guids.Add(bizO3.PK);

			var triggerMetaData = new TriggerMetaData(attribute, bizO1.GetType(), DummyBizoSchema.PK, guids);

			AssertContainsExactElementsInAnyOrder(guids, triggerMetaData.Values);
			AssertEquals(attribute, triggerMetaData.Attribute);
			AssertEquals(bizO1.GetType(), triggerMetaData.BizOType);
			AssertEquals(DummyBizoSchema.PK, triggerMetaData.Column);
		}

		public void TestConstructorWithInvalidInput()
		{
			var attribute = new DeferTriggerAndRunBeforeCommitAttribute("foo", "bar", "baz", typeof(DeferTriggerOnDeleteConditionStrategy));
			var bizO = Factory.New<DummyBusinessObject>();
			var guids = new List<ZGuid>();

			AssertExceptionThrown<ArgumentNullException>(() => new TriggerMetaData(attribute, null, null, guids));
			AssertExceptionThrown<ArgumentNullException>(() => new TriggerMetaData(null, bizO.GetType(), null, guids));
			AssertExceptionThrown<ArgumentNullException>(() => new TriggerMetaData(attribute, bizO.GetType(), null, null));
		}
	}
}
