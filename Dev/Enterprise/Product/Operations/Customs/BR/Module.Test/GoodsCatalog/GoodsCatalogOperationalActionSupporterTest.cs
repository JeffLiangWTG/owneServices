using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(GoodsCatalogOperationalActionSupporter))]
	class GoodsCatalogOperationalActionSupporterTest : OperationalActionSupporterTest<GoodsCatalogOperationalActionSupporter>
	{
		public override bool ShouldSupportDocuments => false;

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.GoodsCatalog;
	}
}
