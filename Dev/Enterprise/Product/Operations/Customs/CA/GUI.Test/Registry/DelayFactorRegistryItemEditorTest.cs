using System;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(DelayFactorRegistryItemEditor))]
	sealed class DelayFactorRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new DelayFactorRegistryItem("", null, null, null, new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 7, DelayIntervalTypeCodes.Codes.Default));

		protected override RegistryItemEditor GetEditor() => new DelayFactorRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override Type GetExpectedEditorPaneType() => typeof(DelayFactorControl);

		protected override object[] GetValidRegistryValues() => new object[] { new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 7, DelayIntervalTypeCodes.Codes.Default) };

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((DelayFactorControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
