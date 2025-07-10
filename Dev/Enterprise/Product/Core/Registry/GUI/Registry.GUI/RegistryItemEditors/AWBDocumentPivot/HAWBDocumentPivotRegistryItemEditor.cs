using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class HAWBDocumentPivotRegistryItemEditor : RegistryItemEditor
	{
		public HAWBDocumentPivotRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new AWBDocumentPivotControl(false);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((AWBDocumentPivotControl)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((AWBDocumentPivotControl)editorPane).FieldValue = (byte[])value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
