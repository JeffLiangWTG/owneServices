using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module
{
	[TestedType(typeof(ResourceStringsModule))]
	class ResourceStringsModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestModuleOverrides()
		{
			using (ResourceStringsModule module = (ResourceStringsModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert(module.GetNewFilterBusinessObjectInternal() is ResourceStringsFilterBusinessObject);
				Assert(module.GetNewGridCollectionInternal() is HelpDataStringCollection);
				Assert(module.GetNewControllerInternal(module.GetNewGridCollectionInternal().AddNew()) is ResourceStringsController);

				using (var filterControl = module.GetNewFilterControlInternal())
				{
					Assert(filterControl is ResourceStringsFilterControl);
				}
			}
		}

		public void TestNoDuplicateRowsAreLoaded()
		{
			using (ResourceStringsFactory.MockSources())
			{
				var en = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
				en.Put("code", new ResourceStringData("code", "caption"));

				var item = new HelpDataString();
				item.HD_Language = Core.SharedConstants.Languages.French;
				item.HD_Code = "code";
				item.HD_Caption = "sous-titre";
				ResourceStringsFactory.Save(EditReasons.Codes.CustomizableDataTranslation, item);

				using (var module = (ResourceStringsModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == "Full Caption");
					filter.IsActive = true;
					filter.Property = "sous-titre";

					module.PerformSearch_ForTest();
					AssertEquals(1, module.GridCollection.Count);

					module.PerformSearch_ForTest();
					AssertEquals(1, module.GridCollection.Count);
				}
			}
		}

		public void TestMaxRowsToLoad()
		{
			using (ResourceStringsModule module = (ResourceStringsModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(100000, module.MaxRowsToLoadInternal);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ResourceStrings;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var dataString = new HelpDataString();

			dataString.HD_Caption = "Caption";
			dataString.HD_FullDescription = "FooDescription";

			collection.Add(dataString);
		}
	}
}
