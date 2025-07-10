using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardSectionChannelLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChannelTypes_ShouldNotIncludeNotChanneled()
		{
			var list = Factory.New<BMBoardSection>().SectionConfiguration.PrimaryAxisChannels.AddNew().Lookups.ChannelTypes;
			AssertEquals(false, list.ContainsCode(ChannelTypeList.Codes.NotChanneled));
		}
	}
}
