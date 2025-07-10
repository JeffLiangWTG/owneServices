using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AccountingRatingService : IAccountingRatingService
	{
		public RatingResults AutoRateAndCreateJobHeader(
			Guid operationsJobPk,
			string operationsJobTableCode,
			Guid staffPk,
			Guid branchPk,
			Guid departmentPk,
			Guid localClientPk,
			bool autorateRevenue = true,
			bool autorateCosts = true,
			bool isConsolLevelChargeExcluded = false,
			LogType maxLogLevel = LogType.Information)
		{
			var options = new AutoRateOptions
			(
				autoRateCost: autorateCosts,
				autoRateRevenue: autorateRevenue,
				excludeConsolLevelChargesOnCosting: isConsolLevelChargeExcluded,
				enableAutoRateExplorer: true
			);

			return GetRatingResults(
				operationsJobPk,
				operationsJobTableCode,
				staffPk,
				branchPk,
				departmentPk,
				localClientPk,
				options,
				maxLogLevel,
				searchForRatesMode: false
				);
		}

		public RatingResults SearchForRates(
			Guid operationsJobPk,
			string operationsJobTableCode,
			Guid staffPk,
			Guid branchPk,
			Guid departmentPk,
			Guid localClientPk,
			bool autorateRevenue = true,
			bool autorateCosts = true,
			bool isConsolLevelChargeExcluded = false,
			LogType maxLogLevel = LogType.Information)
		{
			var options = new AutoRateOptions
			(
				autoRateCost: autorateCosts,
				autoRateRevenue: autorateRevenue,
				excludeConsolLevelChargesOnCosting: isConsolLevelChargeExcluded,
				enableAutoRateExplorer: true,
				triggerSource: AutoRateTriggerSource.Api
			);

			return GetRatingResults(
				operationsJobPk,
				operationsJobTableCode,
				staffPk,
				branchPk,
				departmentPk,
				localClientPk,
				options,
				maxLogLevel,
				searchForRatesMode: true
				);
		}

		public string GetAutoRatingExplorer(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			using (Env.SetTemporaryUserContext(staffPk, branchPk, departmentPk))
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Accounting Rating WebService RatingExplorer" };
				var businessObject = factory.Load(operationsJobTableCode, operationsJobPk);

				var serializer = new RatingObjectSerializer();
				var json = serializer.GetJSON(businessObject);

				return json;
			}
		}

		#region SuppressResourceStringsCheckRegion

		RatingResults GetRatingResults(
			Guid operationsJobPk,
			string operationsJobTableCode,
			Guid staffPk,
			Guid branchPk,
			Guid departmentPk,
			Guid localClientPk,
			AutoRateOptions options,
			LogType maxLogLevel = LogType.Information,
			bool searchForRatesMode = false)
		{
			var logger = new Logger(maxLogLevel);
			var result = new RatingResults();

			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(operationsJobTableCode ?? string.Empty, false);
			if (type == null)
			{
				logger.Warning("Operation job table code is invalid, autorating didn't run");
				result.Logs = logger.GetLogs();
				return result;
			}

			using (Env.SetTemporaryUserContext(staffPk, branchPk, departmentPk))
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Accounting Rating WebService Auto Rate" };
				var businessObject = factory.Load(operationsJobTableCode, operationsJobPk);
				Job jobHeader = null;
				if (businessObject == null)
				{
					logger.Warning("operation job not found, autorating didn't run");
				}
				else if (businessObject as IJobHeaderParent == null || (jobHeader = JobHeaderHelper.LoadOrCreateJobWithClient(branchPk, localClientPk, businessObject)) != null) //GLOW doesn't create JobHeaders so we have to load or create it here
				{
					using (jobHeader)
					{
						var additionalJobsAction = GetAdditionalJobsAction(businessObject);
						var autoratingStarter = new AutoRatingStarter(businessObject, logger, additionalJobsAction: additionalJobsAction);
						result = searchForRatesMode
							? autoratingStarter.SearchForRates(options)
							: autoratingStarter.ExecuteAutorating(options);
						try
						{
							var shouldSave = !TryDeleteQuotesWithoutRates(businessObject, operationsJobTableCode);

							if (shouldSave)
							{
								factory.Save();
								logger.Information("Results Saved");
							}
						}
						catch (ZSaveException ex)
						{
							logger.Error(ex.Message, ex);
							ErrorReporter.ReportOnce("AutoRatingService", ex);
						}
					}
				}
				else
				{
					logger.Warning("failed to load or create job");
				}

				result.Logs = logger.GetLogs();

				return result;
			}
		}

		#endregion

		//Cleanup for when no rates are generated for a Quote and registry value of SaveQuotesWithoutRates is false
		bool TryDeleteQuotesWithoutRates(BusinessObject operationsJob, string operationsTableCode)
		{
			if (operationsTableCode == ViewQuotedBookingSchema.Constants.Prefix &&
				Env.CurrentUser.IsWebUser &&
				operationsJob is QuotedBooking quotedBooking &&
				quotedBooking.IsOneOffQuote &&
				!quotedBooking.Quote.HasNonZeroSellAmt &&
				!WebDataRegistry.Instance.SaveQuotesWithoutRates.Value)
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedQuotedBooking = newFactory.Load<QuotedBooking>(quotedBooking.PK);

				reloadedQuotedBooking?.Quote?.Delete();
				newFactory.Save();

				return true;
			}

			return false;
		}

		static AdditionalJobsAction GetAdditionalJobsAction(object job)
			=> job is IDtbConsignmentRunSheet
				? AdditionalJobsAction.LoadOrCreateInvoicingJobs
				: AdditionalJobsAction.AutoRateAdditionalInvoicingJobs;

		class Logger : ILogger
		{
			public Logger(LogType maxLogLevel)
			{
				_maxLogLevel = maxLogLevel;
			}

			readonly LogType _maxLogLevel;
			readonly List<string> logs = new List<string>();

			public void Log(LogType type, string message)
			{
				Log(type, message, null);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				if (type <= _maxLogLevel)
				{
					logs.Add(string.Concat(type.ToString(), ": ", message));
				}
			}

			public string[] GetLogs() => logs.ToArray();
		}
	}
}

#region Test

// see Enterprise\Product\Operations\Accounting\Business.Testing\Rating\AccountingRatingServiceTest.cs

#endregion
