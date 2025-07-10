using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(IndexDurationRegistryItemEditor))]
	sealed class IndexDurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new IndexDurationRegistryItemEditor(new IndexDurationListRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((IndexDurationRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IndexDurationRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new IndexDurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new IndexDurationList() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
