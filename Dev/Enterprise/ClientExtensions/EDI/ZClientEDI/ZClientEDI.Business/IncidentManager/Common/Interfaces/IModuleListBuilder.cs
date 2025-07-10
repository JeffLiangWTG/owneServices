using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IModuleListBuilder
	{
		CodeDescriptionPairList Build(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal = false, bool excludeDisabled = false);
	}
}