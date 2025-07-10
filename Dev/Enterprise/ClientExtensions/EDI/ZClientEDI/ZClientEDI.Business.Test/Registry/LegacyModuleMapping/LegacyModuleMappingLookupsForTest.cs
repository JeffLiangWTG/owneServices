using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	public class LegacyModuleMappingLookupsForTest : LegacyModuleMappingLookups
	{
		public LegacyModuleMappingLookupsForTest(LegacyModuleMapping parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ModuleMappingList => new CodeDescriptionPairList();
	}
}
