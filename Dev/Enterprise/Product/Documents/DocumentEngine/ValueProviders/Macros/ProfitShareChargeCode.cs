using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ProfitShareChargeCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var registry = (IMultilingualRegistryItem)GetProfitShareChargeCodeRegistryItem().Inner;
			return new ValueProviderDocumenter("<ProfitShareChargeCode>",
				ResString.GetMultilingualString("93977e26-622d-429e-ab51-1d71dc6956b3", "Returns the {0} from the Registry {1}.", registry.CaptionMultilingual, registry.LocationMultilingual),
				new List<(string example, object expectedResult)> { ("<ProfitShareChargeCode>", new Guid("65BBEAF1-B62C-4837-B45B-B069FB0A5ECB")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = ObjectFactory.Get<IAccounting>().ProfitShareChargeCode;
			if (ZGuid.Empty.Equals(result))
			{
				return DBNull.Value;
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		internal RegistryItemWrapper GetProfitShareChargeCodeRegistryItem()
		{
			var locator = new RegistryItemSetLocator();
			var accRegistryItemSet = (RegistryItemSet)locator.GetRegistryItemSet("AccountingConfigurationRegistry");
			return (RegistryItemWrapper)accRegistryItemSet.FindByName("ProfitShareChargeCode");
		}

		static readonly Regex fRegex = new Regex(@"^<\s*Profit\s*Share\s*Charge\s*Code\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
