using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem))]
	internal sealed class PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<PrintChargesBilledToLocalClientAtDestAsCollectCollection>
	{
		protected override StronglyTypedRegistryItem<PrintChargesBilledToLocalClientAtDestAsCollectCollection, PrintChargesBilledToLocalClientAtDestAsCollectCollection> GetNewRegistryItem()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System);
		}
	}
}
