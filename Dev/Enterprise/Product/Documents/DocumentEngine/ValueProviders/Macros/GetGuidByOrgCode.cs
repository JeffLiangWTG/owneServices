using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class GetGuidByOrgCode : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)GetGuidByOrgCode(?:\s*)\((?:\s*)(?<operands>(.*))\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetGuidByOrgCode({orgCode})>",
				ResString.GetMultilingualString("95E22996-AF8F-42EB-A0E8-45E2239542E6", "Returns the primary key of the given organization code."),
				new List<(string example, object expectedResult)> { ((NoResString)"<GetGuidByOrgCode(\"ABCDE\")>", ZString.Empty) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var match = regex.Match(macro);
			var orgCode = match.Groups["operands"].Value;

			if (String.IsNullOrEmpty(orgCode))
			{
				ReportMacroError(report, Res.GetString("25E45270-39C5-43C6-901A-A6F804AF1425", "Organization Code cannot be empty."));
			}
			else if (!Regex.Match(orgCode, @"^""(.*)""$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
			{
				ReportMacroError(report, Res.GetString("B54BBDFE-F9E1-4B8F-98E2-C7700F316F2D", "Organization Code must use quotation marks"));
			}
			else
			{
				var truncatedOrgCode = orgCode.Substring(1, orgCode.Length - 2);
				var query = new ZQuery(OrgHeaderSchema.OH_Code, truncatedOrgCode);
				var organization = report.Factory.LoadTop1<OrgHeader>(query);

				if (organization == null)
				{
					ReportMacroError(report, Res.GetString("FBAA2598-EF9C-4A7E-977E-25AF95BF005D", "Organization not found: {0}", macro));
				}
				else
				{
					result = organization.PK.ToString();
				}
			}

			return result;
		}
	}
}
