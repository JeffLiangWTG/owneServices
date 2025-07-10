using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.USS,
	ServiceTaskNames.UsageSubmissionService,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.UsageSubmissionServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskCodes.USS, StmUsageDataSchema.Constants.TableName,
	new[] { StmUsageDataSchema.Constants.SUD_Category + "!=", StmUsageDataSchema.Constants.SUD_Code + "!=",
			StmUsageDataSchema.Constants.SUD_Fail + "=false", StmUsageDataSchema.Constants.SUD_Fixing + "=false" }, "Usage Submission")]

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class UsageSubmissionServiceTask : ScavengingSubmissionServiceTask
	{
		public UsageSubmissionServiceTask()
			: base()
		{
		}

#if DEBUG
		public
#endif
		UsageSubmissionServiceTask(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
			: base(adaptorFactory, diagnosterFactory)
		{
		}

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new BillingJob(this, Notifier, AdaptorFactory);
		}

		public override string ServiceTaskName => ServiceTaskNames.UsageSubmissionService;
		internal override bool ReportKnownExceptionsAsIssues => IsProduction;
		public virtual bool IsWiseCloudHosted => EnvProxy.IsHostedWithCargowise;
		public override string DefaultServerAddress {
			get
			{
				if (IsWiseCloudHosted)
				{
					return SendInterchangesToTestGateway
						? SystemDataRegistry.Instance.UsageBillingTestGateway.Value
						: SystemDataRegistry.Instance.UsageBillingGateway.Value;
				}
				return base.DefaultServerAddress;
			}
		}

		internal override DateTime OutageStartTime
		{
			get => SystemDataRegistry.Instance.USSOutageStartTime.Value;
			set => SystemDataRegistry.Instance.USSOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		protected override bool IsEnabled(out string message)
		{
			message = string.Empty;
			var isDisabledThroughRefDb = new RefSysConfig.Loader(new BusinessObjectFactory()).GetBoolValue("STOPSTLUSS");
			if (isDisabledThroughRefDb)
			{
				message = Res.GetString("00FF3B69-9B11-4032-8384-7FA434CE082A", "Usage submission has been disabled through reference data.");
				return false;
			}

			return base.IsEnabled(out message);
		}
	}
}
