using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module
{
	public abstract class ModuleFilterGenerator
	{
		public ModuleFilter Generate(ZString description, FilterCategory category, MultilingualString multilingualDescription)
		{
			var filter = GenerateInternal(description);
			filter.Category = category;
			filter.MultilingualDescription = multilingualDescription;
			return filter;
		}

		protected abstract ModuleFilter GenerateInternal(ZString description);
	}
}
