using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	sealed class IsReportCustomLabelHidden : IsCustomLabelHidden
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<IsReportCustomLabelHidden({customlabelname},{defaultreturnvalue})>",
				ResString.GetMultilingualString("792cab00-8386-409b-937b-6eaa2a2bafc8",
				@"Returns a boolean value which is the opposite of the default return value if the Custom Label is not defined on the Organization associated with the Report. The default return value is always either 'Y' or 'N'."),
				new List<(string example, object expectedResult)> { ((NoResString)"<IsReportCustomLabelHidden(OrderHeader.CustomDate2, Y)>", (ZBool)false) });
		}

		protected override string GetCustomLabelsType()
		{
			return OrgConstants.CustomLabelType.Report;
		}

		protected override Enterprise.MasterFiles.Business.OrgHeader GetConfigOrganisation(Report report)
		{
			return GetReportConfigOrganisation(report);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)IsReportCustomLabelHidden(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
