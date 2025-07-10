using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.DataTransfer.eNett_Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	eNettInboundTransactionProcessorTask.Code,
	"ComPay Messaging Service",
	"ACC",
	typeof(eNettInboundTransactionProcessorTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia
)]

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.eNett_Integration
{
	internal class eNettInboundTransactionProcessorTask : ServiceProviderImpl
	{
		public const string Code = "CPM";

		[HostedServiceRequirement]
		public static string AtLeastOneActiveAustralianCompanyHasENettRegistration()
		{
			var auCompanies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Australia);
			if (!auCompanies.Any())
			{
				return (NoResString)"System should have at least one active Australian Company.";
			}
			else if (auCompanies.Any(x => AccountingConfigurationRegistry.Instance.ENettRegistration.CheckRegistrationCodeIsConfigured(x.PK.ToGuid())))
			{
				return string.Empty;
			}
			else
			{
				var auCompanyCodes = new ZStringBuilder(auCompanies.Select(x => $"'{x.GC_Code}'")).ToStringWithDelimiterBetweenAppends(", ");
				return $"At least one of these Australian Companies needs to have the ComPay Client Number value configured under the registry setting '{AccountingConfigurationRegistry.Instance.ENettRegistration.GetLocationInEnglish()}' configured ({auCompanyCodes}).";
			}
		}

		public override void RunTask(CancellationToken token)
		{
			GetProcessor().Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		protected virtual eNettInboundTransactionProcessor GetProcessor()
		{
			return new eNettInboundTransactionProcessor();
		}
	}
}
