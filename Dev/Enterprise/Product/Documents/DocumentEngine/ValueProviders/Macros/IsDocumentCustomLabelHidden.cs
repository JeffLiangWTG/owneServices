using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	sealed class IsDocumentCustomLabelHidden : IsCustomLabelHidden
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<IsDocumentCustomLabelHidden({customlabelname},{defaultreturnvalue})>",
				ResString.GetMultilingualString("9ba51090-4271-4bae-af5b-3a40c5d6e271",
				@"Returns a boolean value which is the opposite of the default return value if the Custom Label is not defined on the Organization associated with the Document. The default return value is always either 'Y' or 'N'."),
				new List<(string example, object expectedResult)> { ((NoResString)"<IsDocumentCustomLabelHidden(OrderHeader.CustomDate2, Y)>", (ZBool)false) });
		}

		protected override string GetCustomLabelsType()
		{
			return OrgConstants.CustomLabelType.Document;
		}

		protected override Enterprise.MasterFiles.Business.OrgHeader GetConfigOrganisation(Report report)
		{
			return GetDocumentConfigOrganisation(report);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)IsDocumentCustomLabelHidden(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
