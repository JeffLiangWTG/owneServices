using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAExternalModuleColumnsAndFiltersProvider.EqualModuleStatusFilter))]
	sealed class EqualAndBlankModuleStatusFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CAExternalModuleColumnsAndFiltersProvider.EqualModuleStatusFilter(
				CAConsolModuleColumnsAndFiltersProvider.EManifestColumnsAndFiltersProvider.Captions.CloseJobStatusFilterId,
					CAConsolModuleColumnsAndFiltersProvider.EManifestColumnsAndFiltersProvider.Captions.CloseJobStatusMultilingualDescription,
					(c, v) => new ZQuery(),
					() => new EManifestForwarderJobStatusList());
		}
	}
}
