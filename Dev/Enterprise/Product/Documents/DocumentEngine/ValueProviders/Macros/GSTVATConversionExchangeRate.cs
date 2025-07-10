using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.Integration.Accounting;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class GSTVATConversionExchangeRate : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GSTVATConversionExchangeRate({TransactionHeaderPK},{CompanyPK})>",
				ResString.GetMultilingualString("FA8A28B8-DB36-43F0-A390-150BDD5831CB", "Returns the GST VAT Conversion Exchange Rate for the current Company's login country/region."),
				new List<(string example, object expectedResult)> { (ResString.GetMultilingualString("FB30FFF6-72F1-4D01-B573-2A180BABB0C3", "<GSTVATConversionExchangeRate(B802BEBC-7C84-4FD1-9378-64F361C0AB27, 878D7ACA-FFC3-49FC-9710-969CA0C0F2AC)>"), 1m) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var matchedGroups = Regex.Match(macro).Groups;
			var transactionHeaderPK = ZGuid.ParseSafe(matchedGroups["TransactionHeaderPK"].Value);
			var companyPk = ZGuid.ParseSafe(matchedGroups["CompanyPK"].Value);

			return ObjectFactory.Get<IAccounting>().GetGSTVATConversionExchangeRate(transactionHeaderPK, companyPk);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)GSTVATConversionExchangeRate(?:[\s]*)\((?:[\s]*)(?<TransactionHeaderPK>.*)(?:[\s]*),(?:[\s]*)(?<CompanyPK>.*)(?:[\s]*)\)(?:[\s]*)>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
