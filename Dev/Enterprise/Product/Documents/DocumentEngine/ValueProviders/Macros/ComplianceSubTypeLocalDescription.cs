using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ComplianceSubTypeLocalDescription : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ComplianceSubTypeLocalDescription({complianceSubType})>"
				, ResString.GetMultilingualString("52645210-c4d0-4c7d-af64-640576a9c208", "Will return the local description of the compliance sub type."),
				new List<(string example, object expectedResult)> { ("<ComplianceSubTypeLocalDescription(<SubType>)>", "FACTURA") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			// Arg 1: ComplainceSubType
			string complianceSubType = fRegex.Match(macro).Groups[1].Value;

			return AccComplianceSequence.GetDocumentTitle(complianceSubType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<ComplianceSubTypeLocalDescription\(\""*([^\""]+)\""*\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
