using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ContactSalutationRegistryItemEditor))]
	sealed class ContactSalutationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ContactSalutationRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, new ContactSalutationCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ContactSalutationRegistryItemEditor(new ContactSalutationTypesDataType(), null, null);
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(ContactSalutationControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			ContactSalutationCollection collection = new ContactSalutationCollection();

			var contactSalutation1 = collection.AddNew();
			contactSalutation1.EnglishSalutation = "Dear";
			contactSalutation1.Gender = "Male";

			var contactSalutation2 = collection.AddNew();
			contactSalutation2.EnglishSalutation = "Dearest";
			contactSalutation2.Gender = "Female";

			return new[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ContactSalutationControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
