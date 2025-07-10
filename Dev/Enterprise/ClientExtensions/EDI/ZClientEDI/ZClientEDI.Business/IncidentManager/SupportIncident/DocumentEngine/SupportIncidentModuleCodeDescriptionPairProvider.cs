using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentModuleCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new SupportIncidentModuleListBuilder().Build(ModuleListType.Unspecified, ZString.Empty, ZString.Empty);
		}
	}
}

