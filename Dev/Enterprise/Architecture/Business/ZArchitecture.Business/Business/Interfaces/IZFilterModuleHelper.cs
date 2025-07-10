using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public interface IZFilterModuleHelper : IDisposable
	{
		IZFilterModuleHelper GetZFilterModule(ModuleIdentifier id, string countryOverride = null);
		void OverrideModuleDecisionProvider(ISQLFilterOnlyModuleDecisionProvider moduleDecisionProvider);

		FilterStripBusinessObject FilterBusinessObject { get; }
		IBusinessObjectCollection GridCollection { get; }
		Action<ZQuery> AddAdditionalDisplayFilter { get; set; }
		bool DoNotCheckOrSaveChanges { get; set; }
		bool ShouldLoadFilterBizOIndexSearchFilter { get; set; }
	}
}
