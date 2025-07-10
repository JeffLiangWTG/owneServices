using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPEProcessQueueLookupsHelperTestCase : UPEProcessQueueHelperTestCase
	{
		public void TestGetQueueNameList()
		{
			AssertEquals(ExpectedQueueList.CodesAsString, LookupsHelperForNonPersistentQueue.GetQueueNameList().CodesAsString);
			AssertEquals(ExpectedQueueList.CodesAsString, LookupsHelperForUPEProcessQueue.GetQueueNameList().CodesAsString);
		}

		public void TestGetReasonCodeList()
		{
			foreach (CodeDescriptionPair pair in ExpectedQueueList)
			{
				string expectedCodeListAsString = ReasonCodeDescriptionPairList.GetReasonList(ExpectedQueueList, pair.Code).CodesAsString;
				LookupsHelperForNonPersistentQueue.Queue.QueueName = pair.Code;
				LookupsHelperForUPEProcessQueue.Queue.QueueName = pair.Code;
				AssertEquals(expectedCodeListAsString, LookupsHelperForUPEProcessQueue.GetReasonCodeList(ExpectedQueueList).CodesAsString);
				AssertEquals(expectedCodeListAsString, LookupsHelperForNonPersistentQueue.GetReasonCodeList(ExpectedQueueList).CodesAsString);
			}
		}

		public void TestGetStatusCodeList()
		{
			foreach (CodeDescriptionPair queueCodePair in ExpectedQueueList)
			{
				ReasonCodeDescriptionPairList reasonList = ReasonCodeDescriptionPairList.GetReasonList(ExpectedQueueList, queueCodePair.Code);
				foreach (CodeDescriptionPair reasonCodePair in reasonList)
				{
					string expectedCodeListAsString = StatusCodeDescriptionPairList.GetStatusList(queueCodePair.Code, reasonCodePair.Code).CodesAsString;
					LookupsHelperForNonPersistentQueue.Queue.QueueName = queueCodePair.Code;
					LookupsHelperForNonPersistentQueue.Queue.Status = reasonCodePair.Code;
					LookupsHelperForUPEProcessQueue.Queue.QueueName = queueCodePair.Code;
					LookupsHelperForUPEProcessQueue.Queue.Status = reasonCodePair.Code;
					AssertEquals(expectedCodeListAsString, LookupsHelperForNonPersistentQueue.GetStatusCodeList().CodesAsString);
					AssertEquals(expectedCodeListAsString, LookupsHelperForUPEProcessQueue.GetStatusCodeList().CodesAsString);
				}
			}
		}

		public void TestGetTaskAssignedToList()
		{
			AssertNotNull(LookupsHelperForNonPersistentQueue.GetTaskAssignedToList());
			AssertNotNull(LookupsHelperForUPEProcessQueue.GetTaskAssignedToList());
		}

		#region Implementation
		protected UPEProcessQueueLookupsHelper LookupsHelperForUPEProcessQueue
		{
			get
			{
				return (UPEProcessQueueLookupsHelper)base.HelperForUPEProcessQueue;
			}
		}

		protected UPEProcessQueueLookupsHelper LookupsHelperForNonPersistentQueue
		{
			get
			{
				return (UPEProcessQueueLookupsHelper)base.HelperForNonPersistentProcessQueue;
			}
		}

		protected override sealed UPEProcessQueueHelperBase GetNewProcessQueueHelperBaseForUPEProcessQueue(UPEProcessQueue queue)
		{
			return GetNewProcessQueueLookupsHelperForUPEProcessQueue(queue);
		}

		protected override sealed UPEProcessQueueHelperBase GetNewProcessQueueHelperBaseForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return GetNewProcessQueueLookupsHelperForNonPersistentProcessQueue(queue);
		}

		protected abstract DefaultQueueCodeDescriptionPairList ExpectedQueueList { get; }

		protected abstract UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForUPEProcessQueue(UPEProcessQueue queue);
		protected abstract UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue);
		#endregion
	}
}
