using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVSubscriberServiceTask))]
	internal class HVLVSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<HVLVSubscriberServiceTask>
	{
		public override HVLVSubscriberServiceTask GenerateServiceTask()
		{
			return new HVLVSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName()
		{
			return HVLVSubscriberServiceTask.Description;
		}

		public void TestIsAuditEnabledFalse()
		{
			AssertNotNullOrEmpty(BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection));
			AssertNullOrEmpty(HVLVSubscriberServiceTask.IsAuditEnabled());

			using (BiServers.TemporarilySetAuditServerToNull())
			{
				AssertNotNullOrEmpty(HVLVSubscriberServiceTask.IsAuditEnabled());
			}
		}
	}
}
