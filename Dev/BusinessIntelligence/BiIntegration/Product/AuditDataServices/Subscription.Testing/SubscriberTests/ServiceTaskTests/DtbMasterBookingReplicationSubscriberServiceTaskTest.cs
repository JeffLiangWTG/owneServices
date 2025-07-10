using System;
using System.Linq;
using Enterprise.AuditDataServices.TransportBooking;
using Enterprise.AuditDataServices.TransportBooking.Subscribers;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	[TestedType(typeof(DtbMasterBookingReplicationSubscriberServiceTask))]
	class DtbMasterBookingReplicationSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<DtbMasterBookingReplicationSubscriberServiceTask>
	{
		public override DtbMasterBookingReplicationSubscriberServiceTask GenerateServiceTask() => new DtbMasterBookingReplicationSubscriberServiceTask();

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName() => DtbMasterBookingReplicationSubscriberServiceTask.Description;

		public override void TestIsLoaded()
		{
			var serviceTask = GenerateServiceTask();
			var subscribers = serviceTask.GetSubscribersForTest();
			var subscriberTypes = subscribers.Select(s => s.GetType()).ToArray();
			var expectedSubscriberTypes = new Type[]
			{
				typeof(DtbBookingConsolidationMasterBookingReplicationSubscriber),
				typeof(DtbBookingMasterBookingReplicationSubscriber),
				typeof(DtbBookingInstructionMasterBookingReplicationSubscriber),
				typeof(DtbBookingConfirmationMasterBookingReplicationSubscriber),
			};

			AssertContainsExactElementsInExactOrder(
				"Master booking replication subscribers service task should get all DtbMasterBookingReplication subscribers in correct order",
				expectedSubscriberTypes,
				subscriberTypes);
		}

		public void TestTaskShouldBeEnabled()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(string.Empty, DtbMasterBookingReplicationSubscriberServiceTask.CheckMasterBookingsIsEnabled());
			}
		}

		public void TestTaskShouldBeDisabled()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Master bookings feature is not enabled. The Master booking replication subscriber service task will not start.", DtbMasterBookingReplicationSubscriberServiceTask.CheckMasterBookingsIsEnabled());
			}
		}

		protected override string[] GetExpectedHostedServiceRequirements()
		{
			return new[] { "CheckMasterBookingsIsEnabled", "CheckCdcIsEnabled", "IsAuditEnabled", "CheckCdcShouldBeDisabled" };
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			TransportRegistry.Instance.MasterBookingsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			TransportRegistry.Instance.MasterBookingsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
