using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class UserRepository : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<UserRepository>",
				ResString.GetMultilingualString("5cf13e51-b4fa-4e9c-81d3-49083583b45a", "Returns the user repository database name."),
				new List<(string example, object expectedResult)> { ("<UserRepository>", UserRepositoryDBName) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return UserRepositoryDBName;
		}

		string UserRepositoryDBName
		{
			get
			{
				return Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;
			}
		}

		public override System.Text.RegularExpressions.Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex("^" + RegexProvider.UserRepositoryMacroRegex.ToString() + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
