using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class RegistryAPPaymentMethod : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var registry = (IMultilingualRegistryItem)OrganisationsDataRegistry.Instance.APPaymentMethod.Inner;
			return new ValueProviderDocumenter("<RegistryAPPaymentMethod>",
				ResString.GetMultilingualString("76e262cc-bf3b-411e-96f4-b4ee5405c534", "Returns the {0} from the Registry {1}.", registry.CaptionMultilingual, registry.LocationMultilingual),
				new List<(string example, object expectedResult)> { ("<RegistryAPPaymentMethod>", ZArchitecture.Core.ReceiptTypes.Cheque) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return OrganisationsDataRegistry.Instance.APPaymentMethod.Value;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)RegistryAPPaymentMethod(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
