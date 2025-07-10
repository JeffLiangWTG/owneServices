using System;
using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class OrgDebtorGroupCodeListEditRegistryItemEditor : RegistryItemEditor
	{
		public OrgDebtorGroupCodeListEditRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new OrgDebtorGroupCodeListEditContainer();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((OrgDebtorGroupCodeListEditContainer)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((OrgDebtorGroupCodeListEditContainer)editorPane).FieldValue = (value is Guid[]) ? (Guid[])value : Array.Empty<Guid>();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
