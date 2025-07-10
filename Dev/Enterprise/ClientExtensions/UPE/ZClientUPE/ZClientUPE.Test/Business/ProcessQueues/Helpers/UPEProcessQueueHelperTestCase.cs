using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPEProcessQueueHelperTestCase : TestCaseWithFactory
	{
		public void TestConstructor_ProcessQueue()
		{
			AssertRequiredPropertiesNotNull(HelperForUPEProcessQueue);
			ActiveProcessQueue activeProcessQueue = (ActiveProcessQueue)HelperForUPEProcessQueue.Queue;
			AssertEquals(ExpectedQueueType, activeProcessQueue.QueueType);
			AssertEquals(UPEProcessQueue, activeProcessQueue.ProcessQueue);
			AssertEquals(UPEProcessQueue.Factory, HelperForUPEProcessQueue.Factory);
		}

		public void TestConstructor_NonPersistentProcessQueue()
		{
			AssertRequiredPropertiesNotNull(HelperForNonPersistentProcessQueue);
			AssertEquals(NonPersistentQueue, HelperForNonPersistentProcessQueue.Queue);
			AssertEquals(NonPersistentQueue.Factory, HelperForNonPersistentProcessQueue.Factory);
		}

		#region Implementation
		void AssertRequiredPropertiesNotNull(UPEProcessQueueHelperBase helper)
		{
			AssertNotNull("Factory should not be null", helper.Factory);
			AssertNotNull("IActiveProcessQueue object should not be null", helper.Queue);
		}

		protected UPEProcessQueueHelperBase HelperForUPEProcessQueue
		{
			get
			{
				if (fHelperForUPEProcessQueue == null)
				{
					fHelperForUPEProcessQueue = GetNewProcessQueueHelperBaseForUPEProcessQueue(UPEProcessQueue);
				}

				return fHelperForUPEProcessQueue;
			}
		}

		protected UPEProcessQueueHelperBase HelperForNonPersistentProcessQueue
		{
			get
			{
				if (fHelperForNonPersistentProcessQueue == null)
				{
					fHelperForNonPersistentProcessQueue = GetNewProcessQueueHelperBaseForNonPersistentProcessQueue(NonPersistentQueue);
				}

				return fHelperForNonPersistentProcessQueue;
			}
		}

		protected UPEProcessQueue UPEProcessQueue
		{
			get
			{
				if (fUPEProcessQueue == null)
				{
					fUPEProcessQueue = GetNewUPEProcessQueue();
					SuspendedUPEProcessQueueValidationTesting = fUPEProcessQueue.SuspendValidationTesting();
				}

				return fUPEProcessQueue;
			}
		}

		protected NonPersistentProcessQueue NonPersistentQueue
		{
			get
			{
				if (fNonPersistentQueue == null)
				{
					fNonPersistentQueue = GetNewNonPersistentProcessQueue();
					SuspendedNonPersistentQueueValidationTesting = fNonPersistentQueue.SuspendValidationTesting();
				}

				return fNonPersistentQueue;
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (SuspendedUPEProcessQueueValidationTesting != null)
			{
				SuspendedUPEProcessQueueValidationTesting.Dispose();
			}

			if (SuspendedNonPersistentQueueValidationTesting != null)
			{
				SuspendedNonPersistentQueueValidationTesting.Dispose();
			}
		}

		protected abstract ProcessQueueType.Enum ExpectedQueueType { get; }

		protected abstract UPEProcessQueue GetNewUPEProcessQueue();
		protected abstract NonPersistentProcessQueue GetNewNonPersistentProcessQueue();
		protected abstract UPEProcessQueueHelperBase GetNewProcessQueueHelperBaseForUPEProcessQueue(UPEProcessQueue queue);
		protected abstract UPEProcessQueueHelperBase GetNewProcessQueueHelperBaseForNonPersistentProcessQueue(NonPersistentProcessQueue queue);
		UPEProcessQueueHelperBase fHelperForUPEProcessQueue;
		UPEProcessQueueHelperBase fHelperForNonPersistentProcessQueue;
		UPEProcessQueue fUPEProcessQueue;
		NonPersistentProcessQueue fNonPersistentQueue;
		IDisposable SuspendedUPEProcessQueueValidationTesting;
		IDisposable SuspendedNonPersistentQueueValidationTesting;
		#endregion
	}
}
