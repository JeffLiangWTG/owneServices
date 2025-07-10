using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Registry.Testing
{
	[TestedType(typeof(FallbackRegistryItemEditor))]
	public class FallbackRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new FallbackRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((FallbackControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FallbackControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FallbackSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			FallbackSettings fallbackSettings1 = new FallbackSettings();
			fallbackSettings1.Start = new ZDateTime(2020, 03, 24);
			fallbackSettings1.InvocationReason = "Invocation blabla";

			FallbackSettings fallbackSettings2 = new FallbackSettings();
			fallbackSettings2.Start = new ZDateTime(2020, 03, 25);
			fallbackSettings2.InvocationReason = "Invocation blabla";

			return new object[] { fallbackSettings1, fallbackSettings2 };
		}
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
