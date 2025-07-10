using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Internal;

namespace Enterprise.Registry.GUI
{
	class DecimalArrayRegistryItemEditor : RegistryItemEditor
	{
		public DecimalArrayRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new DecimalArrayControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((DecimalArrayControl)editorPane).BusinessEntity.ToDecimalArray();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			DecimalLineCollection collection;
			DecimalArrayControl typeCastEditorPane = (DecimalArrayControl)editorPane;

			if (typeCastEditorPane.BusinessEntity == null)
			{
				collection = new DecimalLineCollection((DecimalArrayRegistryDataType)DataType);
				typeCastEditorPane.SetDataBinding(collection, null);
			}
			else
			{
				collection = typeCastEditorPane.BusinessEntity;
				collection.RemoveAndDeleteAll();
			}

			collection.Populate((decimal[])value);
		}
	}
}
