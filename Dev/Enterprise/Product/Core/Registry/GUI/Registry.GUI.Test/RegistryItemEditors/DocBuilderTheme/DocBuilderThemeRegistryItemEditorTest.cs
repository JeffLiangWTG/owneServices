using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DocBuilderThemeRegistryItemEditor))]
	sealed class DocBuilderThemeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return DocumentsDataRegistry.Instance.DocBuilderTheme;
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DocBuilderThemeRegistryItemEditor(null, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocBuilderThemeRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new DocBuilderThemeRegistry() };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocBuilderThemeRegistryControl)editorPane).ReadOnly;
		}
	}
}
