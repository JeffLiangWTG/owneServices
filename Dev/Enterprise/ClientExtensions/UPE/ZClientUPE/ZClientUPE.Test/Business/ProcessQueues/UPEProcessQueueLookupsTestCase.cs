using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPEProcessQueueLookupsTestCase : BusinessObjectLookupsTestCase
	{
		public abstract void TestCustomsQueueList();

		public abstract void TestCustomsStatusList();

		public abstract void TestCustomsSubStatusList();

		public abstract void TestCustomsTaskAssignedTos();

		public abstract void TestCustomsQueueLookupsHelper();

		public abstract void TestStatusAndSubStatusListNotCached();

		#region Implementation
		protected UPEProcessQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = GetNewUPEProcessQueue();
				}

				return fQueue;
			}
		}

		protected abstract UPEProcessQueueLookups GetNewUPEProcessQueueLookups();
		protected abstract UPEProcessQueue GetNewUPEProcessQueue();
		UPEProcessQueue fQueue;
		#endregion
	}
}
