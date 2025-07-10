
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public interface IFilterModuleLayoutFinder
	{
		CodeDescriptionPairList GetListOfModuleFilterLayouts(string moduleName);
	}
}
