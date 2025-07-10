using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AWBLabelCustomisationRegistryItem))]
	sealed class AWBLabelCustomisationRegistryItemTest : StronglyTypedRegistryItemTestCase<AWBLabelCustomisation>
	{
		protected override StronglyTypedRegistryItem<AWBLabelCustomisation, AWBLabelCustomisation> GetNewRegistryItem()
		{
			return new AWBLabelCustomisationRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new AWBLabelCustomisation());
		}
	}
}
