using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class DocBuilderThemeRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public DocBuilderThemeRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.fallbackLevel = fallback;
			this.factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new DocBuilderThemeRegistryControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			DocBuilderThemeRegistry result = null;

			var visualThemeRegistryControl = editorPane as DocBuilderThemeRegistryControl;
			if (visualThemeRegistryControl != null)
			{
				result = visualThemeRegistryControl.ThemeRegistry;
			}

			return result;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var themeSelector = value as DocBuilderThemeRegistry;
			var visualThemeRegistryControl = editorPane as DocBuilderThemeRegistryControl;

			if (themeSelector != null && visualThemeRegistryControl != null)
			{
				visualThemeRegistryControl.ThemeRegistry = (DocBuilderThemeRegistry)themeSelector.Clone(fallbackLevel, factory);
			}
		}
	}
}
