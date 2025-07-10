using CargoWise.Application;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ContainerCleanCodesCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var agencyRegistry = ObjectFactory.Get<IAgencyRegistry>();
			return (ReadOnlyCodeDescriptionPairList)agencyRegistry.ContainerCleanCodes;
		}
	}
}
