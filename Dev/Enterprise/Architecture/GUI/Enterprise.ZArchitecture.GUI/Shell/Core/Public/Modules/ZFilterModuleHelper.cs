using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules
{
	public class ZFilterModuleHelper : IZFilterModuleHelper
	{
		public FilterStripBusinessObject FilterBusinessObject => throw new NotImplementedException();

		public IBusinessObjectCollection GridCollection => throw new NotImplementedException();

		public Action<ZQuery> AddAdditionalDisplayFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool DoNotCheckOrSaveChanges { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool ShouldLoadFilterBizOIndexSearchFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void OverrideModuleDecisionProvider(ISQLFilterOnlyModuleDecisionProvider moduleDecisionProvider)
		{
			throw new NotImplementedException();
		}

		public IZFilterModuleHelper GetZFilterModule(ModuleIdentifier id, string countryOverride) => ZFilterModule.GetZFilterModule(id, countryOverride);

		public void Dispose()
		{
			Dispose();
		}
	}
}
