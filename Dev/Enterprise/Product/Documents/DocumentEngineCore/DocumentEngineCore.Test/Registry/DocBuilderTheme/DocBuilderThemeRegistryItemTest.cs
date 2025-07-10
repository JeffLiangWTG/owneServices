using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderThemeRegistryItem))]
	public sealed class DocBuilderThemeRegistryItemTest : StronglyTypedRegistryItemTestCase<DocBuilderThemeRegistry>
	{
		public void TestFireThemeChangedEvent()
		{
			var registryItem = new DocBuilderThemeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption",
				(NoResString)"Hint", new DocBuilderThemeRegistry());
			registryItem.ThemeChanged += RegistryItem_ThemeChanged;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DocBuilderThemeRegistry());
			AssertEquals("Should fire theme changed event", 1, handlerCount);

			((IRegistryItemInternals)registryItem).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Should fire theme changed event", 2, handlerCount);

			registryItem.ThemeChanged -= RegistryItem_ThemeChanged;
		}

		int handlerCount;
		void RegistryItem_ThemeChanged(object sender, EventArgs e)
		{
			handlerCount++;
		}

		public void TestTranslatable()
		{
			var registryItem = new DocBuilderThemeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption",
			(NoResString)"Hint", new DocBuilderThemeRegistry());

			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				string key = ((ResourceString)registryItem.DefaultValue.Themes[0].NameMultilingual).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "标准"));

				var actual = registryItem.DefaultValue.Themes[0].NameMultilingual.ToString(Core.SharedConstants.Languages.ChineseSimplified);
				AssertEquals("标准", actual);

				var newValue = new DocBuilderTheme("NEW THEME", ResString.GetMultilingualString("BCC977B0-3C1C-4CCD-8B57-FC552B0421A4", "NEW THEME"), true);
				registryItem.Value.Themes.Add(newValue);
				key = ((ResourceString)newValue.NameMultilingual).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "新模板"));
				AssertEquals("新模板", newValue.NameMultilingual.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		protected override StronglyTypedRegistryItem<DocBuilderThemeRegistry, DocBuilderThemeRegistry> GetNewRegistryItem()
		{
			return new DocBuilderThemeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new DocBuilderThemeRegistry());
		}
	}
}
