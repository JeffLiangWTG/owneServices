using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class SupportIncidentModuleListBuilderTest : TestCaseWithFactory
	{
		public void TestDoNotIncludeAllInModuleListWhenProductAreaIsFilled()
		{
			var builder = new SupportIncidentModuleListBuilder();
			AssertEquals("Include All in module list when product area is not filled", false, builder.Build(ModuleListType.Unspecified, "ENT", ZString.Empty).ContainsCode("ALL"));
			AssertEquals("Do not include All in module list when product area is filled", false, builder.Build(ModuleListType.Unspecified, "ENT", "ARC").ContainsCode("ALL"));
		}
	}
}