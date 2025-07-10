using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LandedCostingPreference : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LandedCostingPreference({lcgroupid})>",
				ResString.GetMultilingualString("805f9cfe-b678-4040-acd3-482b346004f9",
					@"Returns the Landed Costing Group name from the {0} registry settings. The {1} can be a numeric value from 1 to 6 relating to the 6 configurable groups in the Landed Costing registry settings.",
					Core.Constants.ProductName,
					"lcgroupid"),
				new List<(string example, object expectedResult)> { ("<LandedCostingPreference(1)>", (NoResString)"Intl Freight Charges") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string lCGroupIDString = fRegex.Match(macro).Groups[1].Value;
			var lCGroupID = (ZByte)0;
			ZByte.TryParse(lCGroupIDString, out lCGroupID);

			string result = "";
			if (lCGroupID > 0 && lCGroupID < 7)
			{
				result = FreightDataRegistry.Instance.LandedCostingPreferences.Value.GetLandedCostingGroupNameFromID(lCGroupID);
			}
			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)LandedCostingPreference(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
