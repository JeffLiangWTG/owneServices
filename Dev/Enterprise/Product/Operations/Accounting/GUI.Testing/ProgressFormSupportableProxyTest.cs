using Enterprise.Accounting.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ProgressFormSupportableProxyTest : TestCase
	{
		public void TestInitialization()
		{
			var progressProxy = new ProgressFormSupportableProxy();
			AssertEquals("TotaItemsToComplete should be zero, by default", 0, progressProxy.TotaItemsToComplete);

			progressProxy = new ProgressFormSupportableProxy(2);
			AssertEquals("TotaItemsToComplete can be set in constructor", 2, progressProxy.TotaItemsToComplete);

			ResetEventStatus();
			progressProxy = new ProgressFormSupportableProxy(EventHandler);
			Assert("EventHandler should not be called in constructor", !LastIsComplete.HasValue);

			progressProxy.ResetTotalItemsToComplete(1);
			AssertEquals("TotalItemsToComplete should be one after reset", 1, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be zero after reset", 0, progressProxy.CompletedItems);
			AssertNullOrEmpty("CurrentStatusText should be null/empty after reset", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after reset", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);
		}

		public void TestUpdateProgress()
		{
			ResetEventStatus();
			var progressProxy = new ProgressFormSupportableProxy(EventHandler);

			progressProxy.ResetTotalItemsToComplete(2);
			AssertEquals("TotalItemsToComplete should be two after reset", 2, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be zero after reset", 0, progressProxy.CompletedItems);
			AssertNullOrEmpty("CurrentStatusText should be null/empty after reset", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after reset", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.UpdateProgressStatus("First Message");
			AssertEquals("TotalItemsToComplete should still be two after update", 2, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be one after update", 1, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after update", "First Message", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after update", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.UpdateProgressStatus("Second Message", "Detailed log");
			AssertEquals("TotalItemsToComplete should still be two after update", 2, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be two after update", 2, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after update", "Second Message", progressProxy.CurrentStatusText);
			AssertEquals("Log should be set after update", "Detailed log", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, true);
		}

		public void TestSampleUsageForExtendingProgress()
		{
			ResetEventStatus();
			var progressProxy = new ProgressFormSupportableProxy(EventHandler);

			progressProxy.ResetTotalItemsToComplete(2);
			AssertEquals("TotalItemsToComplete should be two after reset", 2, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be zero after reset", 0, progressProxy.CompletedItems);
			AssertNullOrEmpty("CurrentStatusText should be null/empty after reset", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after reset", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.UpdateProgressStatus("First Message");
			AssertEquals("TotalItemsToComplete should still be two after update", 2, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be one after update", 1, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after update", "First Message", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after update", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.SetTotalItemsToComplete(3);
			AssertEquals("TotalItemsToComplete should be three after extend", 3, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be one after extend", 1, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after extend", "First Message", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after extend", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.UpdateProgressStatus("Second Message", "Detailed log");
			AssertEquals("TotalItemsToComplete should still be three after update", 3, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be two after update", 2, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after update", "Second Message", progressProxy.CurrentStatusText);
			AssertEquals("Log should be set after update", "Detailed log", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.UpdateProgressStatus("Third Message");
			AssertEquals("TotalItemsToComplete should still be three after update", 3, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be three after update", 3, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after update", "Third Message", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after update", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, true);
		}

		public void TestSampleUsageForShrinkingProgress()
		{
			ResetEventStatus();
			var progressProxy = new ProgressFormSupportableProxy(EventHandler);

			progressProxy.ResetTotalItemsToComplete(2);
			AssertEquals("TotalItemsToComplete should be two after reset", 2, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be zero after reset", 0, progressProxy.CompletedItems);
			AssertNullOrEmpty("CurrentStatusText should be null/empty after reset", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after reset", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.SetTotalItemsToComplete(1);
			AssertEquals("TotalItemsToComplete should be one after shrink", 1, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should still be zero after shrink", 0, progressProxy.CompletedItems);
			AssertNullOrEmpty("CurrentStatusText should be null/empty after shrink", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after shrink", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, false);

			progressProxy.UpdateProgressStatus("The Message");
			AssertEquals("TotalItemsToComplete should still be one after update", 1, progressProxy.TotaItemsToComplete);
			AssertEquals("CompletedItems should be one after update", 1, progressProxy.CompletedItems);
			AssertEquals("CurrentStatusText should be set after update", "The Message", progressProxy.CurrentStatusText);
			AssertNullOrEmpty("Log should be null/empty after update", progressProxy.Log);
			AssertEventStatusAndReset(progressProxy, true);
		}

		public void TestNoEventHandlerShouldNotThrow()
		{
			ResetEventStatus();
			var progressProxy = new ProgressFormSupportableProxy();
			Assert("EventHandler should not be called when not set", !LastIsComplete.HasValue);

			progressProxy.ResetTotalItemsToComplete(1);
			progressProxy.SetTotalItemsToComplete(2);
			progressProxy.UpdateProgressStatus("Message");
		}

		#region EventHandler Helpers
		void EventHandler(IProgressFormSupportable sender, bool isComplete)
		{
			LastSender = sender;
			LastIsComplete = isComplete;
		}
		IProgressFormSupportable LastSender;
		bool? LastIsComplete;
		void AssertEventStatusAndReset(IProgressFormSupportable expectedSender, bool expectedIsComplete)
		{
			Assert("Event should be raised", LastIsComplete.HasValue);
			AssertEquals(expectedSender, LastSender);
			AssertEquals(expectedIsComplete, LastIsComplete.Value);
			ResetEventStatus();
		}
		void ResetEventStatus()
		{
			LastSender = null;
			LastIsComplete = null;
		}
		#endregion
	}
}
