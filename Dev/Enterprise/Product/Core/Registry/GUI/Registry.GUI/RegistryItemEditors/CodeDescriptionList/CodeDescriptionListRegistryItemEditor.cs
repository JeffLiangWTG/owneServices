using System;
using System.Data;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionListRegistryItemEditor : RegistryItemEditor
	{
		public CodeDescriptionListRegistryItemEditor(IRegistryItem item, IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
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
				item, editorInfo.ShowCodeColumn, editorInfo.ShowDescriptionColumn,
				GetCharacterCasing(editorInfo.CodeFieldCasing), GetCharacterCasing(editorInfo.DescriptionFieldCasing),
				editorInfo.CodeColumnCaption, editorInfo.DescriptionColumnCaption, dataType.CodeMaxLength);

			if (editorInfo.IsMultilineDescriptionColumn)
			{
				codeDescriptionControl.IsMultilineDescriptionColumn = true;
			}

			return codeDescriptionControl;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected CharacterCasing GetCharacterCasing(CodeDescriptionPairListEditorInfo.CharacterCasing casing)
		{
			switch (casing)
			{
				case CodeDescriptionPairListEditorInfo.CharacterCasing.Lower:
					return CharacterCasing.Lower;
				case CodeDescriptionPairListEditorInfo.CharacterCasing.Normal:
					return CharacterCasing.Normal;
				case CodeDescriptionPairListEditorInfo.CharacterCasing.Upper:
					return CharacterCasing.Upper;

				default:
					throw new ArgumentOutOfRangeException("There is no Windows equivalent of " + casing + ".");
			}
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return new CodeDescriptionPairList(((CodeDescriptionListEditControlForRegistry)editorPane).FieldValue);
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			if (value == null)
			{
				value = new ReadOnlyCodeDescriptionPairList();
			}
			((CodeDescriptionListEditControlForRegistry)editorPane).FieldValue = ((ReadOnlyCodeDescriptionPairList)value).ToXMLByteArray();
		}

		public override string GetCustomValidation(Control editorPane)
		{
			foreach (DataRowView drv in ((CodeDescriptionListEditControlForRegistry)editorPane).CodeDescriptionGrid.List)
			{
				if ((!dataType.AllowEmptyCodes && string.IsNullOrEmpty(drv.Row["Code"].ToString())) ||
					(!dataType.AllowEmptyDescriptions && string.IsNullOrEmpty(drv.Row["Description"].ToString())))
				{
					return Res.GetString("3D3D7842-AF8D-49F8-BBC7-7CFA87204135", "Please enter non empty value.");
				}
			}
			return "";
		}
	}
}
