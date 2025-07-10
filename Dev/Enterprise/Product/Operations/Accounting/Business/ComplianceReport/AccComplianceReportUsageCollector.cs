using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public enum AccComplianceReportUsageCollectorAction
	{
		Queueing = 1,
		ReQueueing = 2,
		Generating = 3,
		GeneratingStatusChanged = 4,
		GeneratingFiles = 5,
	}

	public enum AccComplianceReportUsageCollectorContext
	{
		CRQServiceTask = 1,
		Cargowise = 2,
	}

	[CodeAlive("Provides a comprehensive list of context names used in the compliance report.")]
	public enum AccComplianceReportUsageCollectorContextNames
	{
		ACR = 0,
		OldReportStatus = 1,
		NewReportStatus = 2,
		Action = 3,
		Context = 4,
		SessionId = 5,
		LogonUserName = 6,
		FullUserName = 7,
		DurationSeconds = 8,
		ExecutionDate = 9,
		ACRUsageCollectorVersion = 10,
		ReportType = 11,
		CRCompanyPK = 12,
		CRCompanyCode = 13,
		CountryCode = 14,
		ReportCreationDate = 15,
		ReportDescription = 16,
		DateFrom = 17,
		DateTo = 18,
		Country = 19,
		ReportCode = 20,
		Title = 21,
		TaxRegistrationType = 22,
		Periodicity = 23,
		TablePrefix = 24,
		CountryRegistrationCode = 25,
		ReportLineGrouping = 26,
		ReportLineOrdering = 27,
		GoodsServiceType = 28,
		AmountsRoundingType = 29,
		AmountsRoundingTruncating = 30,
		AmountThresholdLevel = 31,
		ExTaxAmountThreshold = 32,
		TaxAmountThreshold = 33,
		RecipientPK = 34,
		IncludeQueuedForPreviousPeriod = 35,
		NoOfFiles = 36,
		TotalFileSizeInByte = 37,
		ReportConfiguration = 38,
		eDocs = 39,
		StatusMessage = 40
	}

	/// <summary>
	/// The using scope of an instance of this class determines both - the duration of an action and reporting the collected data.
	/// </summary>
	public class AccComplianceReportUsageCollector : IDisposable, IAccComplianceReportUsageCollector
	{
		public AccComplianceReport ComplianceReport;
		public IStopwatch StopWatch;

		public AccComplianceReportUsageCollector()
		{
		}

		public AccComplianceReportUsageCollector(AccComplianceReport complianceReport)
		{
			ComplianceReport = complianceReport;
			ReportProperties = new List<(string name, object value)>();
			AddReportProperties();
			AddReportConfiguration();
			StopWatch = ObjectFactory.Get<IStopwatch>();
			StopWatch.Start();
		}

		public void Dispose()
		{
			StopTimerAndAddDurationAndExecutionDate();
			ReportComplianceReportUsage();
		}

		public void AddChangedStatus(string oldStatus, string newStatus)
		{
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.OldReportStatus), oldStatus));
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.NewReportStatus), newStatus));
		}

		public void AddStatusMessage(string statusMessage)
		{
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.StatusMessage), statusMessage));
		}

		public void AddAction(AccComplianceReportUsageCollectorAction action)
		{
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.Action), action.ToString()));
		}

		public void AddContext(AccComplianceReportUsageCollectorContext context)
		{
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.Context), context.ToString()));
		}

		/// <summary>
		/// In the 1st version of the usage collector I created a new GUID for each run of the CRQ service task
		/// ToDo AWU: Do we need this method in the 1st step?
		/// </summary>
		/// <param name="sessionId"></param>
		public void AddSessionId(Guid sessionId)
		{
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.SessionId), sessionId));
		}

		public void AddLogonUser(IUser user)
		{
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.LogonUserName), user.LoginName));
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.FullUserName), user.FullName));
		}

		public IEnumerable<(string name, object value)> ReturnReportProperties()
		{
			return ReportProperties;
		}

		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		// !! Increment this version number on each change/enhancement of this usage collector.                  !!
		// !! Decide together with your team lead and/or product team if this is a big or medium / small change. !!
		// !! For big enhancements increment the 1st number and reset the second to 0, e.g. 1.5 ==> 2.0          !!
		// !! For medium/small changes increment the 2nd number, e.g. 1.5.23 ==> 1.6.0                           !!
		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		readonly ZString VersionNumber = "1.0";
		readonly List<(string name, object value)> ReportProperties;

		void ReportComplianceReportUsage()
		{
			AddReporteDocsProperties();
			var crNameSpace = new List<(string name, object value)>
			{
				(nameof(AccComplianceReportUsageCollectorContextNames.ACR), ReportProperties.ToArray())
			};
			UsageCollector.Report(ComplianceReport.Factory, UsageFeatures.Codes.AccComplianceReport, crNameSpace.ToArray());
		}

		void StopTimerAndAddDurationAndExecutionDate()
		{
			StopWatch.Stop();
			var elapsedSeconds = Utilities.Round((decimal)StopWatch.ElapsedMilliseconds / 1000, 3);
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.DurationSeconds), elapsedSeconds));
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.ExecutionDate), ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
		}

		void AddReportProperties()
		{
			var result = new List<(string name, object value)>
			{
				(nameof(AccComplianceReportUsageCollectorContextNames.ACRUsageCollectorVersion), VersionNumber),
				(nameof(AccComplianceReportUsageCollectorContextNames.ReportType), ComplianceReport.ACR_ReportType),
				(nameof(AccComplianceReportUsageCollectorContextNames.CRCompanyPK), ComplianceReport.ACR_GC_Company),
				(nameof(AccComplianceReportUsageCollectorContextNames.CRCompanyCode), ComplianceReport.Company.GC_Code),
				(nameof(AccComplianceReportUsageCollectorContextNames.CountryCode), ComplianceReport.Company.Country.Code),
				(nameof(AccComplianceReportUsageCollectorContextNames.ReportCreationDate), ComplianceReport.ACR_SystemCreateTimeUtc.ToString("yyyy-MM-dd")),
				(nameof(AccComplianceReportUsageCollectorContextNames.ReportDescription), ComplianceReport.ACR_Description),
				(nameof(AccComplianceReportUsageCollectorContextNames.DateFrom), ComplianceReport.ACR_DateFrom.ToString("yyyy-MM-dd")),
				(nameof(AccComplianceReportUsageCollectorContextNames.DateTo), ComplianceReport.ACR_DateTo.ToString("yyyy-MM-dd"))
			};

			ReportProperties.AddRange(result);
		}

		void AddReportConfiguration()
		{
			var resultConfig =  new List<(string name, object value)>
			{
				(nameof(AccComplianceReportUsageCollectorContextNames.Country), ComplianceReport.ReportCountryCode),
				(nameof(AccComplianceReportUsageCollectorContextNames.ReportCode), ComplianceReport.ReportCode),
				(nameof(AccComplianceReportUsageCollectorContextNames.Title), ComplianceReport.ReportTypeDescription),
				(nameof(AccComplianceReportUsageCollectorContextNames.TaxRegistrationType), ComplianceReport.TaxRegistrationType),
				(nameof(AccComplianceReportUsageCollectorContextNames.Periodicity), ComplianceReport.ReportPeriodicity),
				(nameof(AccComplianceReportUsageCollectorContextNames.TablePrefix), ComplianceReport.ReportBaseTablePrefix),
				(nameof(AccComplianceReportUsageCollectorContextNames.CountryRegistrationCode), ComplianceReport.ReportCountryRegistrationCode),
				(nameof(AccComplianceReportUsageCollectorContextNames.ReportLineGrouping), ComplianceReport.ReportLineGrouping),
				(nameof(AccComplianceReportUsageCollectorContextNames.ReportLineOrdering), ComplianceReport.ReportLineOrdering),
				(nameof(AccComplianceReportUsageCollectorContextNames.GoodsServiceType), ComplianceReport.GoodsServiceType),
				(nameof(AccComplianceReportUsageCollectorContextNames.AmountsRoundingType), ComplianceReport.ReportAmountsRoundingType),
				(nameof(AccComplianceReportUsageCollectorContextNames.AmountsRoundingTruncating), ComplianceReport.ReportAmountsRoundingTruncating),
				(nameof(AccComplianceReportUsageCollectorContextNames.AmountThresholdLevel), ComplianceReport.ReportAmountThresholdLevel),
				(nameof(AccComplianceReportUsageCollectorContextNames.ExTaxAmountThreshold), ComplianceReport.ReportExTaxAmountThreshold),
				(nameof(AccComplianceReportUsageCollectorContextNames.TaxAmountThreshold), ComplianceReport.ReportTaxAmountThreshold),
				(nameof(AccComplianceReportUsageCollectorContextNames.RecipientPK), ComplianceReport.ReportRecipientOrgPK),
				(nameof(AccComplianceReportUsageCollectorContextNames.IncludeQueuedForPreviousPeriod), ComplianceReport.IncludeQueuedForPreviousPeriod)
			};
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.ReportConfiguration), resultConfig));
		}

		void AddReporteDocsProperties()
		{
			var result = new List<(string name, object value)>();

			var docManager = ComplianceReport.DocManagerInfo();
			var noOfDocs = docManager.AllEDocs.Count;
			var sizeOfAllFiles = docManager.AllEDocs.Cast<IeDoc>().Sum(x => x.FileSizeInMB);
			var resulteDocs = new List<(string name, object value)>
			{
				(nameof(AccComplianceReportUsageCollectorContextNames.NoOfFiles), noOfDocs),
				(nameof(AccComplianceReportUsageCollectorContextNames.TotalFileSizeInByte), (int)Utilities.Round(sizeOfAllFiles * 1024 * 1024, 0))
			};
			ReportProperties.Add((nameof(AccComplianceReportUsageCollectorContextNames.eDocs), resulteDocs));
		}
	}
}

