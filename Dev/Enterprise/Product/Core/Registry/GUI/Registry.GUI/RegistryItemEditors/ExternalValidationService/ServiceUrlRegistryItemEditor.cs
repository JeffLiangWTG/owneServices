using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class ServiceUrlRegistryItemEditor : RegistryItemEditor
	{
		public ServiceUrlRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ServiceUrlControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ServiceUrlControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((ServiceUrlControl)editorPane).Value = (string)value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get
			{
				return EditorPaneAnchor.TopLeftRight;
			}
		}
	}
}
