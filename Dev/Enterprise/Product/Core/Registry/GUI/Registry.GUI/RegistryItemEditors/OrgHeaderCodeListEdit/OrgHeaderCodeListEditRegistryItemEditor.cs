using System;
using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class OrgHeaderCodeListEditRegistryItemEditor : RegistryItemEditor
	{
		public OrgHeaderCodeListEditRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new OrgHeaderCodeListEditContainer();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((OrgHeaderCodeListEditContainer)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((OrgHeaderCodeListEditContainer)editorPane).FieldValue = (value is Guid[]) ? (Guid[])value : Array.Empty<Guid>();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
