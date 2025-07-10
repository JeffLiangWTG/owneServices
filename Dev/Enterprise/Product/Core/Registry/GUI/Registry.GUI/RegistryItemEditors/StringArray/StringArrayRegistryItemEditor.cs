using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class StringArrayRegistryItemEditor : RegistryItemEditor
	{
		public StringArrayRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new StringArrayControl(CharacterCasing);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((StringArrayControl)editorPane).BusinessEntity.ToStringArray();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			StringLineCollection collection;
			StringArrayControl typeCastEditorPane = (StringArrayControl)editorPane;

			if (typeCastEditorPane.BusinessEntity == null)
			{
				collection = new StringLineCollection((StringArrayRegistryDataType)DataType);
				typeCastEditorPane.SetDataBinding(collection, null);
			}
			else
			{
				collection = typeCastEditorPane.BusinessEntity;
				collection.RemoveAndDeleteAll();
			}

			collection.Populate((string[])value);
		}

		CharacterCasing CharacterCasing
		{
			get
			{
				switch (((StringArrayRegistryDataType)DataType).CharacterCase)
				{
					case CharacterCase.Lower:
						return CharacterCasing.Lower;
					case CharacterCase.Upper:
						return CharacterCasing.Upper;
					default:
						return CharacterCasing.Normal;
				}
			}
		}
	}
}
