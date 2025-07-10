using System.Collections.Generic;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.TransportBooking.Subscribers;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]
namespace Enterprise.AuditDataServices.TransportBooking
{
	public class DtbMasterBookingReplicationSubscriberServiceTask : AuditSubscriberTask
	{
		[HostedServiceRequirement]
		public static string CheckMasterBookingsIsEnabled() => TransportRegistry.Instance.MasterBookingsEnabled.Value ? string.Empty : (NoResString)"Master bookings feature is not enabled. The Master booking replication subscriber service task will not start.";

		public const string Code = "MBR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "Master booking replication subscriber service task";

		public override string ServiceTaskCode => Code;

		public override string ServiceTaskDescription => Description;

		public override string AssemblyName => base.AssemblyName + "TransportBooking";

		protected override IEnumerable<IAuditSubscriber> GetSubscribers()
		{
			yield return new DtbBookingConsolidationMasterBookingReplicationSubscriber();
			yield return new DtbBookingMasterBookingReplicationSubscriber();
			yield return new DtbBookingInstructionMasterBookingReplicationSubscriber();
			yield return new DtbBookingConfirmationMasterBookingReplicationSubscriber();
		}

#if DEBUG
		internal IEnumerable<IAuditSubscriber> GetSubscribersForTest() => GetSubscribers();
#endif
	}
}
