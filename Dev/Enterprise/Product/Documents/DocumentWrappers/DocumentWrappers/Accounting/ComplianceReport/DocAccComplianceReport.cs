using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("used in Compliance Tax Report documents")]
	public class DocAccComplianceReport : DocBaseWrapper
	{
		#region Constructor

		protected DocAccComplianceReport(AccComplianceReport report, BusinessObjectFactory factory)
			: base(report, factory)
		{
			// Make sure we have cached related periods
			Factory.ClearCachedValue<AccPeriodManagement[]>(PeriodCacheKeyPrefix + Report.UniqueReportID);
			Factory.GetCachedValue(PeriodCacheKeyPrefix + Report.UniqueReportID, () =>
			{
				var query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, Report.ACR_GC_Company);
				if (Report.ACR_DateFrom.IsValid)
				{
					query.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThan, Report.ACR_DateFrom);
				}
				if (Report.ACR_DateTo.IsValid)
				{
					query.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualTo, Report.ACR_DateTo);
				}

				return Factory.Load<AccPeriodManagement>(query);
			});
		}

		public const string PeriodCacheKeyPrefix = "ComplianceReportPeriods";

		public static DocAccComplianceReport New(AccComplianceReport report, BusinessObjectFactory factory)
		{
			if (report == null)
			{
				return null;
			}
			return new DocAccComplianceReport(report, factory);
		}

		#endregion

		AccComplianceReport Report
		{
			get { return (AccComplianceReport)WrappedObject; }
		}

		#region Properties

		public ZString ReportType
		{
			get { return Report.ACR_ReportType; }
		}

		public ZString Description
		{
			get { return Report.ACR_Description; }
		}

		public ZString Periodicity
		{
			get { return Report.ACR_Periodicity; }
		}

		public ZDateTime DateFrom
		{
			get { return Report.ACR_DateFrom; }
		}

		public ZDateTime DateTo
		{
			get { return Report.ACR_DateTo; }
		}

		public ZString AccountingPeriodYear
		{
			get
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
				var period = periodCalculator.GetPeriodFromDate(DateFrom, GlbCompany.CurrentCompany.PK);
				return period.IsValid ? period.ToString().Substring(0, 4) : "0";
			}
		}

		public ZInt MaxPageNumber
		{
			get
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
				var periodManagement = periodCalculator.GetPeriodManagementFromDate(DateFrom, Report.ACR_GC_Company);
				var lastPeriod = periodCalculator.GetLastPeriodForYear(periodManagement.AM_Year);
				var lastDay = periodCalculator.GetLastDayForPeriod(lastPeriod);

				ZQuery query = new ZQuery(AccComplianceReportSchema.ACR_ReportType, SQLComparisonOperator.Equal, Report.ACR_ReportType);
				query.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, SQLComparisonOperator.Equal, Report.ACR_GC_Company);
				query.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.GreaterThanOrEqualTo, DateFrom);
				query.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.LessThanOrEqualTo, lastDay.IsValid ? lastDay : ZDateTime.MaxSmallDateTime);

				var reports = Factory.Load<AccComplianceReport>(query);
				return reports.Max(x => x.ACR_PageNumberTo);
			}
		}

		public ZString UniqueReportID
		{
			get { return Report.UniqueReportID; }
		}

		public ZString OrgITIVARegNo
		{
			get
			{
				var iTIVA = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == "IT" && x.OK_CodeType == "IVA");
				return iTIVA != null ? iTIVA.OK_CustomsRegNo : ZString.Empty;
			}
		}

		public ZString OrgITCODRegNo
		{
			get
			{
				var iTCOD = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == "IT" && x.OK_CodeType == "COD");
				return iTCOD != null ? iTCOD.OK_CustomsRegNo : ZString.Empty;
			}
		}

		public ZString CompanyAddress
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_Address1 + " " + GlbCompany.CurrentCompany.GC_Address2;
			}
		}

		public ZInt PageNumberFrom
		{
			get { return Report.ACR_PageNumberFrom; }
		}

		public ZInt PageNumberTo
		{
			get { return Report.ACR_PageNumberTo; }
		}

		public ZInt APVatSummaryPageNumberFrom
		{
			get { return Report.APVatSummaryPageNumberFrom; }
		}

		public ZInt APVatSummaryPageNumberTo
		{
			get { return Report.APVatSummaryPageNumberTo; }
		}

		public ZInt ARVatSummaryPageNumberFrom
		{
			get { return Report.ARVatSummaryPageNumberFrom; }
		}

		public ZInt ARVatSummaryPageNumberTo
		{
			get { return Report.ARVatSummaryPageNumberTo; }
		}

		public ZInt LiquidazioneIvaPageNumberFrom
		{
			get { return Report.LiquidazioneIvaPageNumberFrom; }
		}

		public ZInt LiquidazioneIvaPageNumberTo
		{
			get { return Report.LiquidazioneIvaPageNumberTo; }
		}

		public ZString ReportBaseTablePrefix
		{
			get { return Report.ReportBaseTablePrefix; }
		}

		public ZString ReportLineGrouping
		{
			get { return Report.ReportLineGrouping; }
		}

		public ZDecimal GLOpeningBalanceDR
		{
			get { return Report.GLOpeningBalanceDR; }
		}

		public ZDecimal GLOpeningBalanceCR
		{
			get { return Report.GLOpeningBalanceCR; }
		}

		#endregion

		#region ReportLines

		public IComplianceReportAdditionalDataCollector AdditionalDataCollector
		{
			get { return additionalDataCollector ?? (additionalDataCollector = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(Report, ComplianceReportDataCollectionMode.Document)); }
		}
		IComplianceReportAdditionalDataCollector additionalDataCollector;

		public DocAccComplianceReportLineCollection ReportLines
		{
			get
			{
				return reportLines ?? (reportLines = GetReportLines());
			}
		}
		DocAccComplianceReportLineCollection reportLines;

		DocAccComplianceReportLineCollection GetReportLines()
		{
			var result = new DocAccComplianceReportLineCollection(Factory);

			foreach (AccComplianceReportLine line in Report.ReportLines)
			{
				var docLine = DocAccComplianceReportLine.New(line, Report.ReportLines, this);
				docLine.ReportUniqueID = UniqueReportID;
				result.Add(docLine);
			}

			return result;
		}

		#endregion

		#region TransactionHeaderDescriptions

		internal Dictionary<ZGuid, ZString> TransactionHeaderDescriptions
		{
			get { return transactionHeaderDescriptions ?? (transactionHeaderDescriptions = GetTransactionHeaderDescriptions()); }
		}
		Dictionary<ZGuid, ZString> transactionHeaderDescriptions;

		Dictionary<ZGuid, ZString> GetTransactionHeaderDescriptions()
		{
			var result = new Dictionary<ZGuid, ZString>();

			return result;
		}

		#endregion
	}
}
