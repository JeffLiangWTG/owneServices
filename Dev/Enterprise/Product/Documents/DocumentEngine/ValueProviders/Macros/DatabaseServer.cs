using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DatabaseServer : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DatabaseServer>",
				ResString.GetMultilingualString("33af54be-0870-409e-a17d-51378bec788e", "Returns the name of the SQL Server currently storing your {0} database.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<DatabaseServer>", Db.ServerName) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Db.ServerName;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Database\s*Server\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
