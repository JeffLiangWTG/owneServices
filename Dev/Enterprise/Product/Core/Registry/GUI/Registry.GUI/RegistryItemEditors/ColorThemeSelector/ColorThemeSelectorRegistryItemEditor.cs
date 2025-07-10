using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public sealed class ColorThemeSelectorRegistryItemEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public ColorThemeSelectorRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			fallbackLevel = fallback;
			this.factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ColorThemeSelectorControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ColorThemeSelectorControl)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var themeSelector = (ColorThemeSelector)value;
			var clonedData = (ColorThemeSelector)themeSelector.Clone(fallbackLevel, factory);

			((ColorThemeSelectorControl)editorPane).FieldValue = clonedData;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
