using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class ContactEditRegistryItemEditor : RegistryItemEditor
	{
		public ContactEditRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			ZContactBusinessObject contact = new ZContactBusinessObject();
			ContactEditControl result = new ContactEditControl(contact);
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			ZGuid value = ((ContactEditControl)editorPane).Contact.SelectedContactPK;
			Guid result = (value.IsEmpty || !value.IsValid) ? Guid.Empty : value.ToGuid();
			return result;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((ContactEditControl)editorPane).Contact.SelectedContactPK = (Guid)value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
