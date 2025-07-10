using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class RatesServiceUrlRegistryItemEditor : RegistryItemEditor
	{
		public RatesServiceUrlRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new RatesServiceUrlControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((RatesServiceUrlControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((RatesServiceUrlControl)editorPane).Value = (string)value;
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
