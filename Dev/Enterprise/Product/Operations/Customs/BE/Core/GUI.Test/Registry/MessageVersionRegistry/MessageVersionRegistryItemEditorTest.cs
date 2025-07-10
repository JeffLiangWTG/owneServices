using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Registry.Testing;

[TestedType(typeof(MessageVersionRegistryItemEditor))]
class MessageVersionRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new MessageVersionRegistryItemEditor(new MessageVersionDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		var control = (MessageVersionRegistryItemControl)editorPane;
		var messageVersionGrid = control.FindSingle<ZArchitecture.ZGrid>("MessageVersionGrid");
		return !messageVersionGrid.ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType() => typeof(MessageVersionRegistryItemControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new MessageVersionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new MessageVersionRegistryCollection().DefaultCollection);

	protected override object[] GetValidRegistryValues() => new object[] { new MessageVersionRegistryCollection { new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget } } };

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
}
