using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyNameFromPK : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyNameFromPK({companypk})>",
				ResString.GetMultilingualString("b2f3fa9d-d0ab-412b-81d6-6a4c9f45b242", "Returns the Company Name from the given company PK."),
				new List<(string example, object expectedResult)> { ("<CompanyNameFromPK(<CompanyPK>)>", GlbCompany.CurrentCompany.CompanyName) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string pkAsString = match.Groups[1].Value.Trim();
			ZString result = "";

			if (String.IsNullOrEmpty(pkAsString))
			{
				return result;
			}
			ZGuid pk;

			if (!ZGuid.TryParse(pkAsString, out pk))
			{
				ReportMacroError(report, Res.GetString("d644a38b-308d-4956-9d14-2176dfe19a7e", "Parameter [{0}] is not a valid PK.", pkAsString));
				return result;
			}

			var company = Factory.Load<GlbCompany>(pk);
			if (company != null)
			{
				result = company.GC_Name;
			}

			return result;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Name(?:[\s]*)From(?:[\s]*)PK(?:[\s]*)\((?:[""]?)([^,""]*)(?:[""]?)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
