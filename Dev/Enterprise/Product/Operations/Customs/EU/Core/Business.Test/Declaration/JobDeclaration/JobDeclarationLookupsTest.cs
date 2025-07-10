using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class JobDeclarationLookupsTest<T, TParent> : Customs.Business.Testing.JobDeclarationLookupsAbstractTest<T, TParent>
		where T : JobDeclarationLookups
		where TParent : BaseJobDeclaration
	{
		protected override ZString EntryStatusListForCustomsWareCodes => Factory.GetCachedValue<CustomsWareEntryStatusList>().CodesAsString;

		protected override ZString EntryStatusListForDefaultFallBackCodes => Factory.GetCachedValue<EntryStatusList>().CodesAsString;
	}
}
