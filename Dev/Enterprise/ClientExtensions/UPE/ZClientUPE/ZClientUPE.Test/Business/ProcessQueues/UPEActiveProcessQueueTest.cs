using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEActiveProcessQueue))]
	internal class UPEActiveProcessQueueTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusCaption()
		{
			AssertEquals("Reason", UPEActiveProcessQueue.StatusCaption);
		}

		public void TestSubStatusCaption()
		{
			AssertEquals("Status", UPEActiveProcessQueue.SubStatusCaption);
		}

		public void TestReasonCaption()
		{
			AssertEquals("Remarks", UPEActiveProcessQueue.ReasonCaption);
		}

		public void TestIncludeLogsWithEmptyQueueName()
		{
			AssertEquals(false, UPEActiveProcessQueue.IncludeLogsWithEmptyQueueName);
		}

		public void TestOverridenNew()
		{
			AssertEquals(typeof(UPEActiveProcessQueue), ActiveProcessQueue.New(Queue).GetType());
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UPEActiveProcessQueue(Queue);
		}

		UPEActiveProcessQueue UPEActiveProcessQueue
		{
			get
			{
				if (fUPEActiveProcessQueue == null)
				{
					fUPEActiveProcessQueue = new UPEActiveProcessQueue(Queue);
				}

				return fUPEActiveProcessQueue;
			}
		}

		ProcessQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = Factory.New<ProcessQueue>();
				}

				return fQueue;
			}
		}

		UPEActiveProcessQueue fUPEActiveProcessQueue;
		ProcessQueue fQueue;
		#endregion
	}
}
