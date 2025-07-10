using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgHeaderCodeListEditRegistryItemEditor))]
	sealed class OrgHeaderCodeListEditRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new OrgHeaderCodeListEditRegistryItemEditor(new GuidArrayRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrgHeaderCodeListEditContainer)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgHeaderCodeListEditContainer);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			GuidArrayRegistryItem result = new GuidArrayRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.OrgHeaderCodeListEdit);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new Guid[] { Guid.NewGuid(), Guid.NewGuid() } };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
