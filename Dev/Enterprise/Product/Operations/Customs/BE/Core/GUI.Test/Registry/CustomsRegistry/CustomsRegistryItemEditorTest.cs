using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Registry.Testing;

[TestedType(typeof(CustomsRegistryItemEditor))]
class CustomsRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new CustomsRegistryItemEditor(new MessageVersionDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		var control = (CustomsRegistryItemControl)editorPane;
		var messageVersionGrid = control.CustomsRegistryGrid;
		return !messageVersionGrid.ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType() => typeof(CustomsRegistryItemControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CustomsRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new CustomsRegistryCollection().DefaultCollection);

	protected override object[] GetValidRegistryValues() => new object[] { new CustomsRegistryCollection { new CustomsRegistry { StartingDate = new ZDateTime(ZDateTime.Today.Year, 1, 1), StartingNo = 1, CurrentNo = 0 } } };

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
}
