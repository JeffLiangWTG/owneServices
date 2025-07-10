using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class RegistryItemTVP : RegistryItem
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RegistryItemTVP({registryobjectidentifier})>"
				, ResString.GetMultilingualString("c95e8a55-0f04-4c1c-b3c6-d9486a462739",
				@"Will return a SQL Server table valued parameter from registry item values from the object in the Old Style registry (i.e: {0}.property name) or the New Style registry (i.e: {1}.property name) based on the registry object identifier that you pass in. This is useful for when you want to access data from a collection in the registry.",
				"Env.Registry", "DocumentsDataRegistry.Instance"),
				new List<(string example, object expectedResult)> { ((NoResString)"<RegistryItemTVP(OrganisationsDataRegistry.Instance.CommissionPeriodList)>", new ReplacementWithParameterType(new DataTable(), "dbo.TVP_CommissionPeriod")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = base.GetReplacementCore(macro, report);
			var collectionToTVP = result as IRegistryCollectionToTVP;
			if (collectionToTVP is null)
			{
				ReportMacroError(report, Res.GetString("e69ebbad-4a86-4c86-9b7e-bd5eb675e55d", "Registry Item indicated by {0} doesn't support passing as a table valued parameter.", macro));
				return string.Empty;
			}

			return new ReplacementWithParameterType(collectionToTVP.CreateDataTable(), $"dbo.{collectionToTVP.TVPType}");
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Registry\s*Item\s*TVP\s*\(.*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
