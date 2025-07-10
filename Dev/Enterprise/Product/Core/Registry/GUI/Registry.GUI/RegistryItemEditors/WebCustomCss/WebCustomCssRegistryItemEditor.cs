using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebCustomCssRegistryItemEditor : RegistryItemEditor
	{
		public WebCustomCssRegistryItemEditor(IRegistryDataType dataType, WebCustomCssRegistryItem registryItem)
			: base(dataType)
		{
			RegistryItem = Argument.NotNull(registryItem, nameof(registryItem));
		}

		public WebCustomCssRegistryItem RegistryItem { get; }

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new WebCustomCssControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((WebCustomCssControl)editorPane).CurrentDataItem.ToWebTrackerCustomCssArray();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((WebCustomCssControl)editorPane).SetDataBinding(new WebCustomsCssCollectionWrapper(RegistryItem.UrlsRegistryItem.Value, (WebTrackerCustomCss[])value), string.Empty);
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
