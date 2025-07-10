using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class CWSupportLoginTokenPrivateKeyRegistryEditor : RegistryItemEditor
	{
		public CWSupportLoginTokenPrivateKeyRegistryEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((CWSupportLoginTokenPrivateKeyControl)editorPane).FileDataAsBinary;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new CWSupportLoginTokenPrivateKeyControl();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((CWSupportLoginTokenPrivateKeyControl)editorPane).SetFileData((byte[])value);
		}
	}
}
