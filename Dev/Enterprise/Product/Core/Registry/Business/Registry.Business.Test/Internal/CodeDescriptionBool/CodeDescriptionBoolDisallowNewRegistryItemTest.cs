using System.Linq;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewRegistryItem))]
	sealed class CodeDescriptionBoolDisallowNewRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public void TestConstructor()
		{
			CodeDescriptionBoolDisallowNewRegistryItem item = new CodeDescriptionBoolDisallowNewRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool"), StaffRolesNotificationHelper.GetRoles());

			AssertEquals("Name", "Name", item.Name);
			AssertEquals("Category", "Category", item.Category);
			AssertEquals("Caption", "Caption", item.Caption);
			AssertEquals("Hint", "Hint", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("EditorInfo.BoolColumnCaption", "Bool", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);

			CodeDescriptionBoolDisallowNewCollection expectedDefaultValue = StaffRolesNotificationHelper.GetRoles();

			CodeDescriptionBoolDisallowNewCollection defaultValue = (CodeDescriptionBoolDisallowNewCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", expectedDefaultValue.Count, defaultValue.Count);

			for (int i = 0; i < expectedDefaultValue.Count; i++)
			{
				AssertEquals(string.Format("DefaultValue[{0}].Code", i), expectedDefaultValue[i].Code, defaultValue[i].Code);
				AssertEquals(string.Format("DefaultValue[{0}].Description", i), expectedDefaultValue[i].Description, defaultValue[i].Description);
			}

			AssertEquals("DefaultValue.AddNew().Bool", false, defaultValue.AddNew().Bool);
		}

		public void TestGetCaptionsInEnglish()
		{
			using (IMockResourceStringCache chsMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var codeDescriptionBoolDisallowNew1 = new CodeDescriptionBoolDisallowNew
				{
					Description = ResString._GetMultilingualString(555, "1", "One")
				};
				var codeDescriptionBoolDisallowNew2 = new CodeDescriptionBoolDisallowNew
				{
					Description = ResString._GetMultilingualString(555, "2", "Two")
				};

				chsMockData.Put("1", new ResourceStringData("1", "一"));
				chsMockData.Put("2", new ResourceStringData("2", "二"));

				var list = new CodeDescriptionBoolDisallowNewCollection
				{
					codeDescriptionBoolDisallowNew1,
					codeDescriptionBoolDisallowNew2
				};
				var registryItem = new CodeDescriptionBoolDisallowNewRegistryItem("Name", (NoResString)"Category",
					(NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System,
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Bool"), list);

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var captions = registryItem.GetCaptions(registryItem.DefaultValue).ToList();
					AssertEquals("One", captions[0]);
					AssertEquals("Two", captions[1]);
				}
			}
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionBoolDisallowNewRegistryItem("", null, null, null, RegistryStorageFlags.System, new CodeDescriptionBoolRegistryEditorInfo((NoResString)""), StaffRolesNotificationHelper.GetRoles());
		}
	}
}
