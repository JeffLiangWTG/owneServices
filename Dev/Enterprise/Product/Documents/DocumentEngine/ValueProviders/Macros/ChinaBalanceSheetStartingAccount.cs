using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ChinaBalanceSheetStartingAccount : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ChinaBalanceSheetStartingAccount>",
				ResString.GetMultilingualString("f85717b2-0187-4402-a0d5-f7d313be3666", "Returns the Balance Sheet Starting Account Number for China."),
				new List<(string example, object expectedResult)> { ("<ChinaBalanceSheetStartingAccount>", "1020.30.40") });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			return GetBalanceSheetStartingAccount();
		}

#if DEBUG
		internal
#endif
 string GetBalanceSheetStartingAccount()
		{
			object result = "";
			var gLAccountSecondReportStartsFrom = ObjectFactory.Get<IAccounting>().ReportOrder_GLAccountSecondReportStartsFrom(Core.SharedConstants.Languages.ChineseSimplified, Core.Constants.CountryCodes.China);

			if (!gLAccountSecondReportStartsFrom.IsEmpty)
			{
				var sql = string.Format("SELECT {0} FROM {1} WHERE {2} = @Guid",
					AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.Name,
					AccGLAccountDescriptorSchema.Constants.TableName,
					AccGLAccountDescriptorSchema.PK.Name);

				var cmd = Db.Connection.Command(sql);
				cmd.AddParameterBasedOnDbColumn("@Guid", gLAccountSecondReportStartsFrom.ToGuid(), AccGLHeaderSchema.PK);
				result = cmd.ExecuteScalar();
			}
			return (result != null) ? result.ToString() : "";
		}
		#endregion

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)China(?:[\s]*)Balance(?:[\s]*)Sheet(?:[\s]*)Starting(?:[\s]*)Account(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
