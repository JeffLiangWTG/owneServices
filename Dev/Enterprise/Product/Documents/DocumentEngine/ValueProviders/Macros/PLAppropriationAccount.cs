using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class PLAppropriationAccount : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var registry = GetPLAppropriationAccountRegistryItem();
			return new ValueProviderDocumenter("<PLAppropriationAccount>",
				ResString.GetMultilingualString("3c6e955d-f7ae-41ff-95e3-e9aa4badc7d8", "Returns the {0} defined in the Registry {1}.", registry.CaptionMultilingual, registry.LocationMultilingual),
				new List<(string example, object expectedResult)> { ("<PLAppropriationAccount>", "9999.99.99") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var cmd = Db.Connection.Command("SELECT AG_AccountNum FROM dbo.AccGlHeader WHERE AG_PK = @Guid");
			cmd.AddParameterBasedOnDbColumn("@Guid", ObjectFactory.Get<IAccounting>().PLAppropriationAccount, AccGLHeaderSchema.PK);
			var result = cmd.ExecuteScalar();

			return (result != null) ? result.ToString() : "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		internal RegistryItemImpl GetPLAppropriationAccountRegistryItem()
		{
			var locator = new RegistryItemSetLocator();
			var accRegistryItemSet = (RegistryItemSet)locator.GetRegistryItemSet("AccountingConfigurationRegistry");
			return (RegistryItemImpl)accRegistryItemSet.FindByName("GL_PL_APPROPRIATION_ACCOUNT");
		}

		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)PL(?:[\s]*)Appropriation(?:[\s]*)Account(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
