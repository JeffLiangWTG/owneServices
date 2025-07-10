using System;
using System.Collections.Generic;
using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataRefreshSubscriptionManagerTestCase : TestCaseWithFactory
	{
		public void TestSubscriptionEnabledFiresEvent()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizOCopy = factory2.Load<DummyBusinessObject>(bizO.PK);

			DataRefreshSubscriptionManager manager = new DataRefreshSubscriptionManager(new EventHandler(OnEventFired));
			manager.BusinessObject = bizOCopy;

			Factory.Save();

			AssertEquals("PreCondition: EventFired", false, EventFired);
			Assert("PreCondition : BizO.HasChanges = false", !bizO.HasChanges);
			bizO.Z0_Code = "XX";
			Assert("PreCondition : BizO.HasChanges = true", bizO.HasChanges);

			AssertEquals("BizO RowState", DataRowState.Modified, bizO.Row.RowState);
			AssertEquals("BizOCopy RowState", DataRowState.Unchanged, bizOCopy.Row.RowState);

			Factory.Save();

			AssertEquals("Factory RefreshEnabled", true, Factory.RefreshEnabled);
			AssertEquals("Factory 2RefreshEnabled", true, factory2.RefreshEnabled);
			AssertNotNull("Bus is not null", DataRefreshManager.Bus);

			List<DataRefreshBus.Subscription> subscriptions = new List<DataRefreshBus.Subscription>(DataRefreshManager.Bus.Subscriptions.GetSubscriptions(Factory, new DataRefreshBus.ZGuidSubscriptionSubject(bizO.PK), null));
			AssertEquals("Subscriptions count", 1, subscriptions.Count);
			AssertNotEquals(Factory, subscriptions[0].Factory);
			AssertNotNull("MethodToCall", manager.MethodToCall);

			AssertEquals("TableName", bizO.TableName, bizOCopy.TableName);
			AssertEquals("PK", bizO.PK, bizOCopy.PK);

			AssertEquals("BizO RowState", DataRowState.Unchanged, bizO.Row.RowState);
			AssertEquals("BizOCopy RowState", DataRowState.Unchanged, bizOCopy.Row.RowState);

			AssertEquals("DataRefreshSubscriptionManager : EventFired", true, EventFired);
		}

		public void TestSubscriptionDisabled()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizOCopy = factory2.Load<DummyBusinessObject>(bizO.PK);

			DataRefreshSubscriptionManager manager = new DataRefreshSubscriptionManager(new EventHandler(OnEventFired));
			manager.BusinessObject = bizOCopy;
			manager.Enabled = false;

			Factory.Save();

			Assert(!EventFired);
			bizO.Z0_Code = "XX";
			Factory.Save();

			Assert(!EventFired);
		}

		public void OnEventFired(object sender, EventArgs e)
		{
			EventFired = true;
		}

		bool EventFired;
	}
}
