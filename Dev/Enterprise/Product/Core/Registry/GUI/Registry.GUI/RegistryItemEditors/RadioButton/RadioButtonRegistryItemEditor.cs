using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class RadioButtonRegistryItemEditor : RegistryItemEditor
	{
		public RadioButtonRegistryItemEditor(IRegistryEditorInfo editorInfo, IRegistryDataType dataType)
			: base(dataType)
		{
			this.editorInfo = (BooleanRegistryEditorInfo)editorInfo;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new RadioButtonControl(editorInfo.Caption);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((RadioButtonControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((RadioButtonControl)editorPane).Value = (bool)value;
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly BooleanRegistryEditorInfo editorInfo;
	}
}
