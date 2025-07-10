using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class EnterpriseMenuSectionListBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var builder = new EnterpriseMenuSectionListBuilder();
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddRange(new EnterpriseModuleList());
			expectedList.AddRangeOverwriteIfExists(EDIDataRegistry.Instance.LegacyMenuSectionMappings.Value.GetLegacyModules());

			AssertContainsExactElementsInAnyOrder(expectedList, builder.Build("", ""));
		}
	}
}