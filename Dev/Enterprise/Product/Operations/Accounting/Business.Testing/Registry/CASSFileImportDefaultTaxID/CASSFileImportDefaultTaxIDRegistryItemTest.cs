using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CASSFileImportDefaultTaxIDRegistryItem))]
	public class CASSFileImportDefaultTaxIDRegistryItemTest : StronglyTypedRegistryItemTestCase<CASSFileImportDefaultTaxID>
	{
		protected override StronglyTypedRegistryItem<CASSFileImportDefaultTaxID, CASSFileImportDefaultTaxID> GetNewRegistryItem()
		{
			return new CASSFileImportDefaultTaxIDRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
