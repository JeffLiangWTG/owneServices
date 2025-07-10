using CargoWise.Application;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ContainerDamageCodesCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var agencyRegistry = ObjectFactory.Get<IAgencyRegistry>();
			return (ReadOnlyCodeDescriptionPairList)agencyRegistry.ContainerDamageCodes;
		}
	}
}
