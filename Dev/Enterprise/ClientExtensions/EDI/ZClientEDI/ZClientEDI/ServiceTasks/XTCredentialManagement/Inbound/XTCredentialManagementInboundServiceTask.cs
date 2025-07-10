using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ServiceTasks.XTCredentialManagement.Inbound;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(XTCredentialManagementInboundServiceTask.ServiceTaskCode,
	XTCredentialManagementInboundServiceTask.ServiceTaskDescription,
	"ESV",
	typeof(XTCredentialManagementInboundServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes")
]

[assembly: HostedServiceBusinessObjectBinding(XTCredentialManagementInboundServiceTask.ServiceTaskCode,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.XMS
	},
	XTCredentialManagementInboundServiceTask.ServiceTaskDescription
)]

namespace Enterprise.Client.EDI.ServiceTasks.XTCredentialManagement.Inbound
{
	public class XTCredentialManagementInboundServiceTask : ServiceProviderImpl
	{
		public ILogger Logger
		{
			get { return logger ?? (logger = ServiceLogger); }
			set { logger = value; }
		}
		ILogger logger;

		public const string ServiceTaskCode = "XTC";
		public const string ServiceTaskDescription = "XT Credential Change Response Service Task";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			try
			{
				Logger.Log(LogType.Information, ServiceTaskDescription + " Started");
				new XTCredentialManagementInboundProcessor(Factory).Process(youMustReactToThisToken);
				Logger.Log(LogType.Information, ServiceTaskDescription + " Finished");
			}
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, ServiceTaskDescription + " Failed", ex);
			}
		}

		public BusinessObjectFactory Factory { get; set; } = new BusinessObjectFactory() { NameForDebugging = "CredentialChange Response" };
	}
}
