using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class UnselectedCollectionBatchTypesMacro : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<UnselectedCollectionBatchTypesMacro>",
				ResString.GetMultilingualString("B74392A3-1E28-4A3A-AEC6-4C5B88B907EE", "Returns the List of Active Collection Batch read from the Registry."),
				new List<(string example, object expectedResult)> { ("<UnselectedCollectionBatchTypesMacro>", "ST1,ST3") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var oAMFR = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty);
			var rows = oAMFR.GetCodeDescriptionPairList();
			if (rows != null)
			{
				var notSelectedItems = rows.Cast<CodeDescriptionPair>()
							.Where(x => !oAMFR.GetBoolFromCode(x.Code))
							.Select(x => x.Code.Trim());
				return String.Join(",", notSelectedItems);
			}
			else
			{
				return "";
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Unselected\s*Collection\s*Batch\s*Types\s*Macro\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
