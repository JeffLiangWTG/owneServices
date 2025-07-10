using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class BinaryKeyRegistryItemEditor : RegistryItemEditor
	{
		public BinaryKeyRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var dataType = (BinaryKeyRegistryDataType)DataType;
			return new BinaryKeyControl(dataType.KeySize);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((BinaryKeyControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((BinaryKeyControl)editorPane).Value = (string)value;
		}
	}
}
