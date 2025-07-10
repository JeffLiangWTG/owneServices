using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrentPeriod : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrentPeriod>",
				ResString.GetMultilingualString("448c5655-4ccc-4eb9-8ddd-21b159b851d4", "Returns the Current Period for the Current Company ({0}) the current user is logged into.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CurrentPeriod>", 200301) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GetPeriod();
		}

		internal ZInt GetPeriod()
		{
			return new AccountingPeriodCalculator(new BusinessObjectFactory()).GetPeriodFromDate(ZDateTime.Now);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Current\s*Period\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
