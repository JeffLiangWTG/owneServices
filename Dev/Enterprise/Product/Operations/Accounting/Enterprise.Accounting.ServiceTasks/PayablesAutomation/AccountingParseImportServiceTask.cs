using System.Threading;
using CargoWise.Application;
using Enterprise.Accounting.APAutomation.AccountingParseImportServiceTask;
using Enterprise.Accounting.ServiceTasks.PayablesAutomation;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Services;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AccountingParseImportServiceTask.Code,
	"Accounting Parse Import Service Task",
	"ACC",
	typeof(AccountingParseImportServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: HostedServiceBusinessObjectBinding(
	AccountingParseImportServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	[
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DashAccountingImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.API,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal,
	],
	"Parsed AP Invoices")
]

namespace Enterprise.Accounting.ServiceTasks.PayablesAutomation
{
	public class AccountingParseImportServiceTask : ServiceProviderImpl
	{
		public const string Code = "API";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var shipamaxService = ObjectFactory.Get<IShipamaxService>();
			var dashErrorReporter = ObjectFactory.Get<IDashErrorReporter>();
			var dashEntitiesService = new DashEntitiesService(shipamaxService, dashErrorReporter);
			var processor = new ParsedDraftInvoiceProcessor(ServiceLogger, dashEntitiesService);
			processor.Run(youMustReactToThisToken);
		}
	}
}
