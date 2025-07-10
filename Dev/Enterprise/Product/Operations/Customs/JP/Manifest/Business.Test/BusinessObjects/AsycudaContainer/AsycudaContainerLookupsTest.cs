using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerLookups))]

	public class AsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVanningLocationCodeCollection()
		{
			AsycudaManifestHeader.AMA_CustomsOffice = "JP";
			AsycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var list = AsycudaContainerLookups.VanningLocationCodeCollection;
			AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);

			var collection = list as ZZRefCusCodeListCombinedCollection;
			CombineAssertions(() =>
			{
				collection.AssertFilterBusinessObjectDefault("Transport Mode:Property", ModuleTextFilter.ComparisonConstants.Exact, Core.Constants.TransportModes.Sea, true);
				collection.AssertFilterBusinessObjectDefault("List Type:Property", string.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, false);
				collection.AssertFilterBusinessObjectDefault("Country/Region or Grouping:Property", string.Empty, Core.Constants.CountryCodes.Japan, false);
			});
		}

		AsycudaContainerLookups AsycudaContainerLookups => asycudaContainerLookups ??= Container.Lookups;
		AsycudaContainerLookups asycudaContainerLookups;

		AsycudaContainer Container => container ??= AsycudaManifestHeader.Containers.AddNew();
		AsycudaContainer container;

		AsycudaManifestHeader AsycudaManifestHeader => asycudaManifestHeader ??= Factory.NewWithValidTestData<AsycudaManifestHeader>();
		AsycudaManifestHeader asycudaManifestHeader;
	}
}
