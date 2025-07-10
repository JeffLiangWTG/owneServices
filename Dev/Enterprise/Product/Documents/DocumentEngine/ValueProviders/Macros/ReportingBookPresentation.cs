using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ReportingBookPresentation : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ReportingBookPresentation({ReportingBookPK})>",
				ResString.GetMultilingualString("E37024B0-6A83-41F4-A632-9223A08CF743", @"Returns the Presentation journals for Reporting Book."),
				new List<(string example, object expectedResult)> { ((NoResString)"<ReportingBookPresentation(E37024B0-6A83-41F4-A632-9223A08CF7438)>", "") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var reportingbookPK = match.Groups[1].Value;

			ZGuid reportingGuidPK;
			AccReportingBook reportingBook = null;
			if (ZGuid.TryParse(reportingbookPK, out reportingGuidPK))
			{
				reportingBook = report.Factory?.Load<AccReportingBook>(reportingGuidPK);
			}

			if(reportingBook == null)
			{
				return "";
			}

			if (!reportingBook.ARB_IncludeChildPresentation || string.IsNullOrEmpty(reportingBook.ARB_IncludePresentationJournals))
			{
				return reportingBook.ARB_IncludePresentationJournals.ToString();
			}

			return ObjectFactory.Get<IAccounting>().GetCategorisWithChildren(reportingBook.ARB_IncludePresentationJournals);
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<\s*ReportingBookPresentation\((.{1,40})\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
