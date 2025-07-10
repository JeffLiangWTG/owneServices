using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class CodeDescriptionListRegistryEditorEx : CodeDescriptionListRegistryItemEditor
	{
		public CodeDescriptionListRegistryEditorEx(IRegistryItem item, IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(item, dataType, editorInfo)
		{ }

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new CodeDescriptionListEditControlForRegistryEx(item, editorInfo.ShowCodeColumn, editorInfo.ShowDescriptionColumn,
				GetCharacterCasing(editorInfo.CodeFieldCasing), GetCharacterCasing(editorInfo.DescriptionFieldCasing),
				editorInfo.CodeColumnCaption, editorInfo.DescriptionColumnCaption, dataType.CodeMaxLength);
		}
	}
}
