using CargoWise.EntityFramework.Testing;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class ProjectModuleListBuilderTest : TestCaseWithFactory
	{
		public void TestAlwaysIncludeAllInModuleList()
		{
			var builder = new ProjectModuleListBuilder();
			var products = new string[] { "", "ENT" };
			var moduleListTypes = new ModuleListType[] { ModuleListType.Unspecified, ModuleListType.MenuSection, ModuleListType.Cr8, ModuleListType.Cr9 };
			var productAreas = new string[] { "", "ARC" };

			foreach (var product in products)
			{
				foreach (var moduleListType in moduleListTypes)
				{
					foreach (var productArea in productAreas)
					{
						Assert(builder.Build(moduleListType, product, productArea).ContainsCode("ALL"));
					}
				}
			}
		}
	}
}