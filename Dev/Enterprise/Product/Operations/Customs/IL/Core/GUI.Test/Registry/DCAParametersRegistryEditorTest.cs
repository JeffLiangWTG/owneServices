using System;
using System.Windows.Forms;
using Enterprise.Customs.IL.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(DCAParametersRegistryEditor))]
	sealed class DCAParametersRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new DCAParametersRegistryEditor(new DCAParametersRegistryDataType(), null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DCAParametersControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DCAParametersControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new DCAParametersRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);

		protected override object[] GetValidRegistryValues() => new[] { new DCAParameters() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
