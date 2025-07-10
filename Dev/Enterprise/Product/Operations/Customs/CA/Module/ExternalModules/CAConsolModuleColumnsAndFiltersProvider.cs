using System.Collections.Generic;
using IConsolModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.IConsolModuleColumnsAndFiltersProvider;
using IExternalModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.IExternalModuleColumnsAndFiltersProvider;

namespace Enterprise.Customs.CA.Module
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in EManifestColumnsAndFiltersProvider.")]
	partial class CAConsolModuleColumnsAndFiltersProvider : CAExternalModuleColumnsAndFiltersProvider, IConsolModuleColumnsAndFiltersProvider
	{
		protected override IEnumerable<IExternalModuleColumnsAndFiltersProvider> GetColumnsAndFiltersProviders()
		{
			yield return new EManifestColumnsAndFiltersProvider();
		}
	}
}
