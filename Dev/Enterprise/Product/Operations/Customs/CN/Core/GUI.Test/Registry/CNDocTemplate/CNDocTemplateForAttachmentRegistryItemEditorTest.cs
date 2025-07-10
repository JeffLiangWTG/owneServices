using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CNDocTemplateForAttachmentRegistryItemEditor))]
	class CNDocTemplateForAttachmentRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CNDocTemplateForAttachmentRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CNDocTemplateForAttachmentRegistryItemUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CNDocTemplateForAttachmentRegistryItemUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CNDocTemplateForAttachmentRegistryItem("", null, null, null, RegistryStorageFlags.System, CNDocTemplateForAttachmentCollection.GetDefault());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CNDocTemplateForAttachmentCollection();
			var docTemplate = collection.AddNew();
			docTemplate.OrganizationPK = ZGuid.Empty;
			docTemplate.DataContext = ".CustomsDeclarationDocument";
			docTemplate.DocumentTemplate = "CN Customs Invoice(System)";
			docTemplate.DocumentType = "INV";
			docTemplate.DocumentDescription = "Invoice";
			docTemplate.AttachmentType = "00000003";
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
