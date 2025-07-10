using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ContactEditRegistryItemEditor))]
	sealed class ContactEditRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSelectedContactPK()
		{
			using (ContactEditControl editorPane = (ContactEditControl)Editor.NewWinFormsEditorPane())
			{
				ZGuid newGuid = ZGuid.NewZGuid();
				editorPane.Contact.SelectedContactPK = newGuid;
				AssertEquals("GetValueFromEditorPane()", newGuid.ToGuid(), (Guid)Editor.GetValueFromEditorPane(editorPane));

				editorPane.Contact.SelectedContactPK = ZGuid.Empty;
				AssertEquals("GetValueFromEditorPane()", Guid.Empty, (Guid)Editor.GetValueFromEditorPane(editorPane));

				editorPane.Contact.SelectedContactPK = ZGuid.Invalid;
				AssertEquals("GetValueFromEditorPane()", Guid.Empty, (Guid)Editor.GetValueFromEditorPane(editorPane));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ContactEditRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ContactEditControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ContactEditControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			GuidRegistryItem result = new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new ContactRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Guid.NewGuid(), Guid.Empty };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#endregion
	}
}
