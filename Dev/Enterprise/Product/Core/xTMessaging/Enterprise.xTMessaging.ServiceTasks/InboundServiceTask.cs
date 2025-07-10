using System;
using System.Threading;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ServiceTaskCodeList.Codes.XTI,
	ServiceTaskCodeList.Descriptions.XTI,
	"ESV",
	typeof(InboundServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute")
]

namespace Enterprise.xTMessaging.ServiceTasks
{
	public class InboundServiceTask : DirectxTServiceTask
	{
		[HostedServiceNudged]
		public static bool CheckIsNudged() => ObjectFactory.Get<IProductRegistration>()?.Key.HostedLocation != Core.Constants.LicenceConstants.NotHostedWithCargoWise
			&& ObjectFactory.Get<IProductRegistration>()?.IsWiseTechGlobalInternalSystem() == false
			&& DirectxTMessagingRegistry.Instance.EnableXTINudge.Value
			&& ObjectFactory.Get<IProductRegistration>()?.Key.DatabaseType != DatabaseTypes.Codes.Test
			&& ObjectFactory.Get<IProductRegistration>()?.Key.DatabaseType != DatabaseTypes.Codes.Training;

		protected override void RunTaskCore(CancellationToken token)
		{
			var pilotBranch = GlbBranch.GetFirstActiveBranch();
			using (Environment.DisposableEnvironment.ForBranch(pilotBranch.PK.ToGuid()))
			{
				base.RunTaskCore(token);
			}
		}

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			return new InboundInterchangeProcessor(ServiceLogger);
		}

		protected override DateTime OutageStartTime
		{
			get => DirectxTMessagingRegistry.Instance.XTIOutageStartTime.Value;
			set => DirectxTMessagingRegistry.Instance.XTIOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}
