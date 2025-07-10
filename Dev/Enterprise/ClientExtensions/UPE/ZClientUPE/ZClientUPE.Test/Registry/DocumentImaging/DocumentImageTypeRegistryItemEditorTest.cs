using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(DocumentImageTypeRegistryItemEditor))]
	internal class DocumentImageTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DocumentImageTypeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocumentImageTypeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocumentImageTypeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DocumentImageTypeRegistryItem("", "", "", "");
		}

		protected override object[] GetValidRegistryValues()
		{
			DocumentImageTypeCollection collection = new DocumentImageTypeCollection();
			DocumentImageType imageType = collection.AddNew();
			imageType.UPSCode = "UPS";
			imageType.DocTypeCode = "MSC";
			imageType.Description = "DESC";
			imageType.MoveJobToClassOnImport = true;
			imageType.NotifyOnImport = true;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
