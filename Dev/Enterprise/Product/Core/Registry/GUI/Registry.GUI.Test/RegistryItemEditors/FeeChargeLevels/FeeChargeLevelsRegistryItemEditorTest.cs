using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FeeChargeLevelsRegistryItemEditor))]
	sealed class FeeChargeLevelsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new FeeChargeLevelsRegistryItemEditor(new FeeChargeLevelsRegistryItem.FeeChargeLevelsRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((FeeChargeLevelsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FeeChargeLevelsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FeeChargeLevelsRegistryItem("", null, null, null, RegistryStorageFlags.System, new FeeChargeLevelsSection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new FeeChargeLevelsSection() };
		}
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
