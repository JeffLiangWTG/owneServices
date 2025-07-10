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
	[TestedType(typeof(ExportStatusRequestRecipientRegistryItemEditor))]
	class ExportStatusRequestRecipientRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new ExportStatusRequestRecipientRegistryItemEditor(new ExportStatusRequestRecipientDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (ExportStatusRequestRecipientRegistryItemControl)editorPane;
			var messageRecipientGrid = (ZArchitecture.ZGrid)control.Controls.Find("MessageRecipientGrid", true).Single();
			return !messageRecipientGrid.ReadOnly;
		}
		protected override Type GetExpectedEditorPaneType() => typeof(ExportStatusRequestRecipientRegistryItemControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new ExportStatusRequestRecipientRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new ExportStatusRequestRecipientRegistryCollection().DefaultCollection);
		protected override object[] GetValidRegistryValues() => new object[]
		{
			new ExportStatusRequestRecipientRegistryCollection { new ExportStatusRequestRecipientRegistry { SystemCode = ExportStatusRequestRecipientRegistry.AtlasSystemCode, MessageRecipient = "DE001348" } }
		};
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
	}
}
