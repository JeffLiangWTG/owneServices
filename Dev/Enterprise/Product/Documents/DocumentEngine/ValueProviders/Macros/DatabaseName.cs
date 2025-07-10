using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DatabaseName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DatabaseName>",
				ResString.GetMultilingualString("577a1a76-08f0-48a2-b33b-3e5b7ffaeeb7", "Returns the name of the database containing the {0} data on your SQL Server.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<DatabaseName>", Db.DatabaseName) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Db.DatabaseName;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Database\s*Name\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
