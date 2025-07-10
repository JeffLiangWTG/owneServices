using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ServerVersion : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ServerVersion>",
				ResString.GetMultilingualString("61551217-225d-4e2e-be69-6f12709545df", "Returns the version of the SQL Server the report is running on.  Return values are either 2000 or 2005."),
				new List<(string example, object expectedResult)> { ("<ServerVersion>", GetServerVersion()) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GetServerVersion();
		}

		string GetServerVersion()
		{
			return Db.Connection.ServerVersionNumber.SqlServerGeneration;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Server(?:[\s]*)Version(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
