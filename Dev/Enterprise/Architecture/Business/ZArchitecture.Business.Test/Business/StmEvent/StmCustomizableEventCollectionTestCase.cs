using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmCustomizableEventCollectionTestCase : TestCaseWithFactory
	{
		public void TestStmCustomizableEventCodeDescriptionPairList()
		{
			TestCaseHelper.ClearTable("StmEvent");

			var event1 = GetNewEvent("EV1", false);
			var event2 = GetNewEvent("EV2", false);
			var customizableEvent1 = GetNewEvent("EX1", true);
			var customizableEvent2 = GetNewEvent("EX2", true);

			Factory.Save();

			AssertEquals(4, Factory.GetDatabaseCount(typeof(StmEvent)));

			var customizableEventsCodeDescriptionPairList = new StmCustomizableEventCodeDescriptionPairList(Factory);
			AssertEquals(2, customizableEventsCodeDescriptionPairList.Count);
			AssertEquals("EX1", customizableEventsCodeDescriptionPairList[0].Code);
			AssertEquals("EX2", customizableEventsCodeDescriptionPairList[1].Code);
		}

		public void TestStmCustomizableEventCodeDescriptionPairList_DoesntCreateFactories()
		{
			GC.Collect();

			var initialFactoriesCount = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;
			//Let's create a few StmCustomizableEventCodeDescriptionPairList and check no new BOF are created
			new StmCustomizableEventCodeDescriptionPairList(Factory);
			new StmCustomizableEventCodeDescriptionPairList(Factory);
			new StmCustomizableEventCodeDescriptionPairList(Factory);
			AssertEquals(initialFactoriesCount, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length);
		}

		StmEvent GetNewEvent(string eventCode, bool isCustomizable)
		{
			var evt = Factory.New<StmEvent>();
			evt.SE_Code = eventCode;
			evt.SE_IsCustomizable = isCustomizable;
			return evt;
		}

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
		}
	}
}
