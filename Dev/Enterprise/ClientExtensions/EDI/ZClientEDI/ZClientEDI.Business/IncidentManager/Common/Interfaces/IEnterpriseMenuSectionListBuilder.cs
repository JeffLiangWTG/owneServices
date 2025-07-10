using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IEnterpriseMenuSectionListBuilder
	{
		CodeDescriptionPairList Build(ZString product, ZString productArea, bool excludeExternal = false, bool excludeDisabled = false);
	}
}