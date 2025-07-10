using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class DirectoryBrowserRegistryItemEditor : RegistryItemEditor
	{
		public DirectoryBrowserRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new DirectoryBrowserControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((DirectoryBrowserControl)editorPane).GetValue();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((DirectoryBrowserControl)editorPane).SetValue((string)value);
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
