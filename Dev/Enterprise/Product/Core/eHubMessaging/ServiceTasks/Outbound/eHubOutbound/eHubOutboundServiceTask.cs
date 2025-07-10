using System;
using System.Collections.Generic;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.eHubMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.EHO,
	ServiceTaskNames.EHubOutboundMessages,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.eHubOutboundServiceTask),
	AllowsMultipleInstances = true,
	MinimumPeriod = "1minute",
	IsMandatory = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("EHO", EDIInterchangeSchema.Constants.TableName, new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=TRX", EDIInterchangeSchema.Constants.EI_Status + "=HQU", }, ServiceTaskNames.EHubOutboundMessages)]
namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eHubOutboundServiceTask : eHubServiceTaskWithAdaptor
	{
		public eHubOutboundServiceTask()
			: base()
		{
		}

#if DEBUG
		public
#endif
		eHubOutboundServiceTask(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
			: base(adaptorFactory, diagnosterFactory)
		{
		}

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new eHubOutboundServiceTaskJob(this, Notifier, AdaptorFactory) { ProcessSystemInterchanges = true };
			yield return new eHubOutboundServiceTaskJob(this, Notifier, AdaptorFactory) { ProcessSystemInterchanges = false };
		}

		protected override bool HandleCommunicationException(CommunicationException communicationException)
		{
			if (communicationException.IsExceptionPresentIncludingInner<InvalidOperationException>() && communicationException.InnerException.Source == "System.Data")
			{
				Notifier.Notify(new ErrorNotification(ErrorType.Error, GetExceptionsAndInnerExceptions(communicationException, false)));
				return true;
			}

			return base.HandleCommunicationException(communicationException);
		}

		string GetExceptionsAndInnerExceptions(Exception exception, bool isInner)
		{
			var exceptionMessage = (isInner ? "\t" : string.Empty) +
				Res.GetString("{0EE33408-16A0-1234-1234-86BBD8545B75}", "{0}: {1}",
				exception.GetType(), exception.Message);

			return exceptionMessage + (exception.InnerException != null ? "\r\n" + GetExceptionsAndInnerExceptions(exception.InnerException, true) : string.Empty);
		}

		public override string ServiceTaskName => ServiceTaskNames.EHubOutboundMessages;
		internal override bool ReportKnownExceptionsAsIssues => !SendInterchangesToTestGateway;
		public override string DefaultServerAddress => SendInterchangesToTestGateway ? eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.Value : eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.Value;
		bool SendInterchangesToTestGateway => !IsProduction && eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.Value;

		internal override DateTime OutageStartTime
		{
			get => eHubMessagingRegistry.Instance.EHOOutageStartTime.Value;
			set => eHubMessagingRegistry.Instance.EHOOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}
