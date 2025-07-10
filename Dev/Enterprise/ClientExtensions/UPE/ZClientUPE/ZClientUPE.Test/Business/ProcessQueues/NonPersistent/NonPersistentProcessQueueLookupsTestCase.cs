using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class NonPersistentProcessQueueLookupsTestCase : BusinessObjectLookupsTestCase
	{
		public abstract void TestQueueList();

		public abstract void TestStatusList();

		public abstract void TestSubStatusList();

		public abstract void TestTaskAssignedToList();

		public void TestLookupsHelper()
		{
			NonPersistentProcessQueueLookups lookups = GetNewNonPersistentProcessQueueLookups();
			AssertEquals(ExpectedLookupsHelperType, lookups.LookupsHelper.GetType());
		}

		#region Implementation
		protected NonPersistentProcessQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = GetNewNonPersistentQueue();
				}

				return fQueue;
			}
		}

		protected abstract Type ExpectedLookupsHelperType { get; }

		protected abstract Type NonPersistentProcessQueueLookupsTypeToTest { get; }

		protected abstract NonPersistentProcessQueueLookups GetNewNonPersistentProcessQueueLookups();
		protected abstract NonPersistentProcessQueue GetNewNonPersistentQueue();
		NonPersistentProcessQueue fQueue;
		#endregion
	}
}
