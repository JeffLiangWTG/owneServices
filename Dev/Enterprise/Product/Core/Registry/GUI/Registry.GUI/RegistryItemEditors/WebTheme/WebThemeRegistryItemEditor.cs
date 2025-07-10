using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebThemeRegistryItemEditor : RegistryItemEditor
	{
		public WebThemeRegistryItemEditor(IRegistryDataType dataType, IRegistryItem registryItem)
			: base(dataType)
		{
			RegistryItem = registryItem;
		}

		public IRegistryItem RegistryItem
		{
			get { return registryItem; }
			set { registryItem = value; }
		}
		IRegistryItem registryItem;

		protected override Control NewWinFormsEditorPaneCore() => new WebThemeControl(RegistryItem);

		protected override object GetValueFromEditorPaneCore(Control editorPane) => ((WebThemeControl)editorPane).Value;

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((WebThemeControl)editorPane).Value = ((WebTrackerTheme[])value).Select(x => new WebTrackerTheme(x.Url, x.Code)).ToArray();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeft;
	}
}
