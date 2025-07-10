using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Registry.Testing
{
	[TestedType(typeof(MessageVersionRegistryItemEditor))]
	class MessageVersionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new MessageVersionRegistryItemEditor(new MessageVersionDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (MessageVersionRegistryItemControl)editorPane;
			var messageVersionGrid = (ZArchitecture.ZGrid)control.Controls.Find("MessageVersionGrid", true).Single();
			return !messageVersionGrid.ReadOnly;
		}
		protected override Type GetExpectedEditorPaneType() => typeof(MessageVersionRegistryItemControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new MessageVersionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new MessageVersionRegistryCollection().DefaultCollection);
		protected override object[] GetValidRegistryValues() => new object[] { new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = MessageVersionRegistry.AtlasDefaultVersionNumber } } };
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
	}
}
