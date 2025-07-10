using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class HasValidGeneralLedgerData : ValueProviderWithLoadControlFactory
	{
		public override Regex Regex => fRegex;

		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)HasValidGeneralLedgerData(?:[\s]*)\((?:[\s]*)(?<CompanyPK>.*)(?:[\s]*),(?:[\s]*)(?<StartPeriod>.*)(?:[\s]*),(?:[\s]*)(?<StartDate>.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<HasValidGeneralLedgerData({CompanyPK},{StartPeriod},{StartDate})>",
				ResString.GetMultilingualString("79F239EF-6889-4B5F-9654-24D8F10710BE", "If you are running report from EDW, you must make sure that backlog accounting transactions have been completely processed. Please check the Journal Entries Last Processed Date registry if you can't run report"),
				new List<(string example, object expectedResult)> { ("<HasValidGeneralLedgerData(36161720-BCCE-45B4-9FEF-F94E23C97E0E, 202304, )>", "Y") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = "N";
			if (report == null || !report.IsEdwDataSource)
			{
				result = "Y";
				return result;
			}

			var pkString = Regex.Match(macro).Groups["CompanyPK"].ToString().Trim();
			var startPeriodString = Regex.Match(macro).Groups["StartPeriod"].ToString().Trim();
			var startDateString = Regex.Match(macro).Groups["StartDate"].ToString().Trim();

			if (!string.IsNullOrEmpty(pkString))
			{
				if (!Guid.TryParse(pkString, out var companyPK))
				{
					ReportMacroError(report, Res.GetString("491CFE78-310A-4E91-B8EE-3E3D9D1FC84C", "Could not parse Guid: {0}", pkString));
					return result;
				}

				var lastProcessedDate =
					AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				if (lastProcessedDate == DateTime.MinValue)
				{
					return result;
				}

				ZInt.TryParse(startPeriodString, out var startPeriod);
				var startDate = new ZDateTime(startDateString);

				var sqlQuery = new ZDBOnlyQuery(typeof(AccPeriodManagement));

				if (startPeriod == 0 && startDate == ZDateTime.Empty)
				{
					sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, companyPK);
					sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate;

					startDate = Factory.LoadTop1<AccPeriodManagement>(sqlQuery)?.AM_StartDate ?? ZDateTime.Empty;
				}
				else if (startPeriod > 0)
				{
					sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, companyPK);
					sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_Period, startPeriod);
					sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate;

					startDate = Factory.LoadTop1<AccPeriodManagement>(sqlQuery)?.AM_StartDate ?? ZDateTime.Empty;
				}
				else
				{
					sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, companyPK);
					sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
					sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualTo, startDate);
					sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_Period + " DESC";

					startDate = Factory.LoadTop1<AccPeriodManagement>(sqlQuery)?.AM_StartDate ?? ZDateTime.Empty;
				}

				if (startDate >= lastProcessedDate)
				{
					result = "Y";
				}
			}
			return result;
		}
	}
}
