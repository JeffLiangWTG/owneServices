using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebCustomImagesRegistryItemEditor : RegistryItemEditor
	{
		public WebCustomImagesRegistryItemEditor(IRegistryDataType dataType, IRegistryItem registryItem)
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

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new WebCustomImagesControl(RegistryItem);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((WebCustomImagesControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((WebCustomImagesControl)editorPane).Value = (WebTrackerCustomImage[])value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
