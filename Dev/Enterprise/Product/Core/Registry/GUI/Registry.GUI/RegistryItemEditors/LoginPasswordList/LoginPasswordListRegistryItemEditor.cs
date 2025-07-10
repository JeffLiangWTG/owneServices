using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class LoginPasswordListRegistryItemEditor : CodeDescriptionListRegistryItemEditor
	{
		public LoginPasswordListRegistryItemEditor(IRegistryItem item, IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(item, dataType, editorInfo)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var control = new LoginPasswordListEditControlForRegistry(item, editorInfo.CodeColumnCaption, editorInfo.DescriptionColumnCaption, dataType.CodeMaxLength);
			return control;
		}
	}
}
