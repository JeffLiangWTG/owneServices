using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class TranslatableRegistryBusinessItemCollectionRegistryItemTest : TranslatableRegistryItemTest
	{
		public void TestGetCaptionsInEnglish()
		{
			using (var chsMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				chsMockData.Put("e4da0bbf-5fa1-499f-befb-e0985b20528d", new ResourceStringData("e4da0bbf-5fa1-499f-befb-e0985b20528d", "一"));

				var registryItem = new TranslatableRegistryBusinessItemCollectionRegistryItemForTest(new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
					new TranslatableRegistryBusinessItemCollectionRegistryDataTypeForTest(), RegistryStorageFlags.System, RegistryOptions.PreserveTestValue,
					new OpportunityStatusCollection { new OpportunityStatus { Description = ResString._GetMultilingualString(ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, "e4da0bbf-5fa1-499f-befb-e0985b20528d", "One") } }));

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("One", registryItem.GetCaptions(registryItem.DefaultValue).First());
				}
			}
		}
	}
}
