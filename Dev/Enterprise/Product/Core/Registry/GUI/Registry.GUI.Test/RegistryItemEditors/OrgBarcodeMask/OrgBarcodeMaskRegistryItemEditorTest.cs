using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgBarcodeMaskRegistryItemEditor))]
	sealed class OrgBarcodeMaskRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new OrgBarcodeMaskRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrgBarcodeMaskControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgBarcodeMaskControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OrgBarcodeMaskRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			OrgBarcodeMaskCollection collection = new OrgBarcodeMaskCollection(Factory);

			OrgBarcodeMask orgBarcodeMask = collection.AddNew();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsPackDepot = true;

			orgBarcodeMask.Org = org.PK;
			orgBarcodeMask.Priority = 1;
			orgBarcodeMask.Mask = "(VALID)REGEX";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
