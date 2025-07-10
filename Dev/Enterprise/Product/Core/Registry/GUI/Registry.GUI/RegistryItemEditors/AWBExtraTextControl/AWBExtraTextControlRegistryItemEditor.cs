using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AWBExtraTextControlRegistryItemEditor : RegistryItemEditor
	{
		public AWBExtraTextControlRegistryItemEditor(IRegistryItem item, IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType)
		{
			this.dataType = (CodeDescriptionPairListRegistryDataType)dataType;
			this.editorInfo = (CodeDescriptionPairListEditorInfo)editorInfo;
			this.item = item;
		}

		protected readonly CodeDescriptionPairListRegistryDataType dataType;
		protected readonly CodeDescriptionPairListEditorInfo editorInfo;
		protected readonly IRegistryItem item;

		protected override Control NewWinFormsEditorPaneCore()
		{
			CodeDescriptionListEditControlForRegistry codeDescriptionControl = new CodeDescriptionListEditControlForRegistry(
				item, true, true, CharacterCasing.Normal, CharacterCasing.Normal,
				editorInfo.CodeColumnCaption, editorInfo.DescriptionColumnCaption, dataType.CodeMaxLength);

			if (editorInfo.IsMultilineDescriptionColumn)
			{
				codeDescriptionControl.IsMultilineDescriptionColumn = true;
			}

			return new AWBExtraTextControl(codeDescriptionControl);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((AWBExtraTextControl)editorPane).Data;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((AWBExtraTextControl)editorPane).Data = value != null ? (ReadOnlyCodeDescriptionPairList)value : new ReadOnlyCodeDescriptionPairList();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((AWBExtraTextControl)editorPane).ReadOnly = !enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
