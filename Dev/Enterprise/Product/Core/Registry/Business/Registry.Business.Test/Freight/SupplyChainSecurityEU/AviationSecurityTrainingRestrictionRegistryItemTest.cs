using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.SupplyChainSecurityEU
{
	[TestedType(typeof(AviationSecurityTrainingRestrictionRegistryItem))]
	public class AviationSecurityTrainingRestrictionRegistryItemTest : StronglyTypedRegistryItemTestCase<AviationSecurityTrainingRestriction>
	{
		protected override StronglyTypedRegistryItem<AviationSecurityTrainingRestriction, AviationSecurityTrainingRestriction> GetNewRegistryItem()
		{
			return new AviationSecurityTrainingRestrictionRegistryItem("", null, null, null, RegistryStorageFlags.System, new AviationSecurityTrainingRestriction());
		}
	}
}
