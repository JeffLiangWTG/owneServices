using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	public class EquivalentComplianceSubTypeCode : ValueProvider
	{
		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<(?:\s*)equivalentcompliancesubtypecode(?:\s*)\((?:\s*)(?:')?(?<Compliancesubtype>[^\s\,\']+)(?:')?(?:\s*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<EquivalentComplianceSubTypeCode({Compliancesubtype})>",
				ResString.GetMultilingualString("653C8362-192D-41EB-962B-3E721AA8D845", @"Inserts the Compliance Sub Type value. This function returns the Government Authority equivalent code of the compliance sub type."),
				new List<(string example, object expectedResult)> {
					("<EquivalentComplianceSubTypeCode(TXA)>", "1"),
					((NoResString)"<EquivalentComplianceSubTypeCode('TXA')>", "1") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			try
			{
				var complianceSubType = Regex.Match(macro).Groups["Compliancesubtype"].Value;

				return ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(GlbCompany.CurrentCompany.Country.Code)?.GetEquivalentComplianceSubType(complianceSubType) ?? string.Empty;
			}
			catch (Exception ex)
			{
				ReportMacroError(report, ex.Message);
			}

			return result;
		}
	}
}
