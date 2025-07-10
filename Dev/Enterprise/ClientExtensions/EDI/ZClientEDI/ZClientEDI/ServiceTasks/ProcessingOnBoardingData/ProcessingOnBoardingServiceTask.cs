using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ProcessingOnBoardingServiceTask.Code,
	ProcessingOnBoardingServiceTask.Description,
	"CSP",
	typeof(ProcessingOnBoardingServiceTask),
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ProcessingOnBoardingServiceTask.Code,
	EdiTokenAuthOnBoardingDataSchema.Constants.TableName,
	new[]
	{
		EdiTokenAuthOnBoardingDataSchema.Constants.TOD_Status + "=" + OnBoardingStatuses.Codes.Queued
	},
	"Queued Onboarding Data",
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding(
	ProcessingOnBoardingServiceTask.Code,
	EdiTokenAuthOnBoardingDataSchema.Constants.TableName,
	new[]
	{
		EdiTokenAuthOnBoardingDataSchema.Constants.TOD_Status + "=" + OnBoardingStatuses.Codes.StagingMergedAndVerified
	},
	"Staging Merged And Verified Onboarding Data",
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding(
	ProcessingOnBoardingServiceTask.Code,
	EdiTokenAuthOnBoardingDataSchema.Constants.TableName,
	new[]
	{
		EdiTokenAuthOnBoardingDataSchema.Constants.TOD_Status + "=" + OnBoardingStatuses.Codes.Revert
	},
	"Reverted Onboarding Data",
	ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client.EDI
{
	public class ProcessingOnBoardingServiceTask : ServiceProviderImpl, IServiceTaskConfigurationUser
	{
		public ProcessingOnBoardingServiceTask() { }

		internal ProcessingOnBoardingServiceTask(BusinessObjectFactory businessObjectFactory)
		{
			this.businessObjectFactory = businessObjectFactory;
		}

		public const string Code = "POB";
		public const string Description = "Processing On Boarding Data Service Task";

		public ZQuery OnBoardingDataQuery
		{
			get
			{
				return new ZDBOnlyQuery(typeof(EdiTokenAuthOnBoardingData))
				.AddToFilter(EdiTokenAuthOnBoardingDataSchema.TOD_Retry, SQLComparisonOperator.LessThan, 3)
				.AddToFilter(EdiTokenAuthOnBoardingDataSchema.TOD_Status, SQLComparisonOperator.Equal, new string[] { "QUE", "SMV", "REV" });
			}
		}

		readonly BusinessObjectFactory businessObjectFactory;

		string IServiceTaskConfigurationUser.ConfigString { get; set; }

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, Description + " started");
			Run(youMustReactToThisToken);
			ServiceLogger.Log(LogType.Information, Description + " finished");
		}

		void Run(CancellationToken token)
		{
			var factory = businessObjectFactory ?? new BusinessObjectFactory() { NameForDebugging = nameof(EdiTokenAuthOnBoardingData) };
			var ediTokenAuthOnBoardingDatas = factory.Load<EdiTokenAuthOnBoardingData>(OnBoardingDataQuery).ToList();

			foreach (var ediTokenAuthOnBoardingData in ediTokenAuthOnBoardingDatas)
			{
				token.ThrowIfCancellationRequested();

				try
				{
					StatusHandlerFactory.GetHandler(ediTokenAuthOnBoardingData).Handle();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ediTokenAuthOnBoardingData.TOD_Retry++;
					if (ediTokenAuthOnBoardingData.TOD_Retry > 2)
					{
						ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Error;
						ServiceTaskEmailNotification.SendEmailToGroup($"{Description} Error", ex.Message, EDIDataRegistry.Instance.OnboardingNotificationGroup, ServiceLogger);
					}
					var errorMessage = FormattableString.Invariant($"Unable to process on boarding data '{ediTokenAuthOnBoardingData.LicenceEnterpriseCode}', current status is '{ediTokenAuthOnBoardingData.TOD_Status}'");
					ServiceLogger.Error(errorMessage, ex);
				}

				ediTokenAuthOnBoardingData.Factory.Save();
			}
		}
	}
}
