using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TaxRecognitionDefaultingRulesRegistryItem))]
	public class TaxRecognitionDefaultingRulesRegistryItemTest : StronglyTypedRegistryItemTestCase<TaxRecognitionDefaultingRules>
	{
		protected override StronglyTypedRegistryItem<TaxRecognitionDefaultingRules, TaxRecognitionDefaultingRules> GetNewRegistryItem()
		{
			return new TaxRecognitionDefaultingRulesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company);
		}
	}
}
