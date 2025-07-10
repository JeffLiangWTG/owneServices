using System;
using System.Threading;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicPayment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(GlobalElectronicPaymentProcessingServiceTask.Code,
	"Global Electronic Payment Processing Service Task",
	"ACC",
	typeof(GlobalElectronicPaymentProcessingServiceTask),
	IsMandatory = true,
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(GlobalElectronicPaymentProcessingServiceTask.Code,
	AccEPaymentQuoteSchema.Constants.TableName,
	new[]
	{
		AccEPaymentQuoteSchema.Constants.QU_Status + "=" + EPaymentStatusCodes.Quote.Queued
	},
	"Global Electronic Payment Quotes Queuing")]
[assembly: HostedServiceBusinessObjectBinding(GlobalElectronicPaymentProcessingServiceTask.Code,
	AccEPaymentDealSchema.Constants.TableName,
	new[]
	{
		AccEPaymentDealSchema.Constants.AED_Status + "=" + EPaymentStatusCodes.Deal.Queued
	},
	"Global Electronic Payment Deals Queuing")]
[assembly: HostedServiceBusinessObjectBinding(GlobalElectronicPaymentProcessingServiceTask.Code,
	AccEPaymentBeneficiaryRequestSchema.Constants.TableName,
	new[]
	{
		AccEPaymentBeneficiaryRequestSchema.Constants.ABR_Status + "=" + EPaymentStatusCodes.BeneficiaryRequest.Queued
	},
	"Global Electronic Payment Beneficiary Request Queuing")]
namespace Enterprise.Accounting.ElectronicPayment
{
	public class GlobalElectronicPaymentProcessingServiceTask : ServiceProviderImpl
	{
		public const string Code = "GEP";

		ZString TaskName => (NoResString)"Global Electronic Payment Processing"; // Service task name does not need to be translated.

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"{TaskName} service task started."));
			RunTaskCore(youMustReactToThisToken);
			ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"{TaskName} service task completed."));
		}

		void RunTaskCore(CancellationToken token)
		{
			new GlobalElectronicPaymentProcessor().ProcessEPayments(ServiceLogger, token);
		}
	}
}
