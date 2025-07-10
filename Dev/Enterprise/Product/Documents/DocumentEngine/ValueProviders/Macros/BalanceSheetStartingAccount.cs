using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BalanceSheetStartingAccount : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BalanceSheetStartingAccount>",
				ResString.GetMultilingualString("57bb6814-f888-472a-9aae-f330ac47106e", "Returns the Account Number for the Balance Sheet Starting Account."),
				new List<(string example, object expectedResult)> { ("<BalanceSheetStartingAccount>", "9999.99.99") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var sql = string.Format("SELECT {0} FROM {1} WHERE {2} = @Guid",
				AccGLHeaderSchema.AG_AccountNum.Name,
				AccGLHeaderSchema.Constants.TableName,
				AccGLHeaderSchema.PK.Name);

			var cmd = Db.Connection.Command(sql);
			cmd.AddParameterBasedOnDbColumn("@Guid", ObjectFactory.Get<IAccounting>().BSAccountStartAccount, AccGLHeaderSchema.PK);
			var result = cmd.ExecuteScalar();

			return (result != null) ? result.ToString() : "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Balance(?:[\s]*)Sheet(?:[\s]*)Starting(?:[\s]*)Account(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
