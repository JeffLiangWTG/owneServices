using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderThemeRegistry))]
	class DocBuilderThemeRegistryTest : RegistryBusinessObjectTemplateTestCase<DocBuilderThemeRegistry>
	{
		public void TestSystemThemes()
		{
			var registry = new DocBuilderThemeRegistry();
			Assert("registry.Themes.Length > 3", registry.Themes.Count > 3);
			AssertEquals("registry.Themes[0].Name", DocBuilderThemeRegistry.DefaultThemeName, registry.Themes[0].Name);
			AssertEquals("registry.Themes[1].Name", "Classic", registry.Themes[1].Name);
			AssertEquals("registry.Themes[2].Name", "CargoWise Blues", registry.Themes[2].Name);
		}

		public void TestSelectedTheme()
		{
			var registry = new DocBuilderThemeRegistry();

			AssertEquals("registry.SelectedTheme", registry.Themes[0], registry.SelectedTheme);

			registry.SelectedTheme = null;
			AssertEquals("registry.SelectedTheme", registry.Themes[0], registry.SelectedTheme);

			registry.SelectedTheme = registry.Themes[1];
			AssertEquals("registry.SelectedTheme", registry.Themes[1], registry.SelectedTheme);

			registry.SelectedTheme = null;
			AssertEquals("registry.SelectedTheme", registry.Themes[0], registry.SelectedTheme);
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override void CheckAllPropertiesAreEqual(DocBuilderThemeRegistry originalBusinessObject, DocBuilderThemeRegistry newBusinessObject, bool isClone)
		{
			AssertEquals(originalBusinessObject.Themes.Count, newBusinessObject.Themes.Count);
			Assert(!Object.ReferenceEquals(originalBusinessObject.Themes, newBusinessObject.Themes));

			for (int i = 0; i < originalBusinessObject.Themes.Count; ++i)
			{
				AssertEquals(originalBusinessObject.Themes[i].Name, newBusinessObject.Themes[i].Name);
				if (isClone)
				{
					AssertEquals(originalBusinessObject.Themes[i].NameMultilingual, newBusinessObject.Themes[i].NameMultilingual);
				}
				Assert(!Object.ReferenceEquals(originalBusinessObject.Themes[i], newBusinessObject.Themes[i]));
			}
			AssertEquals(originalBusinessObject.SelectedTheme.Name, newBusinessObject.SelectedTheme.Name);
			Assert(!Object.ReferenceEquals(originalBusinessObject.SelectedTheme, newBusinessObject.SelectedTheme));

			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
		}

		protected override DocBuilderThemeRegistry GetBusinessObjectToClone()
		{
			var registry = new DocBuilderThemeRegistry();
			var theme = new DocBuilderTheme("TEST NEW", ResString.GetMultilingualString("CD35074F-855B-481B-B940-41F23A50ED43", "TEST NEW"), true);
			registry.Themes.Add(theme);
			theme = new DocBuilderTheme("TEST NEW 2", ResString.GetMultilingualString("A146E9B9-B9A4-4EFF-ACBD-BBE9E8251818", "TEST NEW 2"), true);
			registry.Themes.Add(theme);
			registry.SelectedTheme = theme;
			return registry;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocBuilderThemeRegistry();
		}

		protected override DocBuilderThemeRegistry GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
		#endregion
	}
}
