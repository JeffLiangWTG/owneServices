using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(OrgPartRelationRegistryItemEditor))]
	public class OrgPartRelationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override object[] GetValidRegistryValues()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			OrgPartRelationRegistryBusinessObjectCollection collection = new OrgPartRelationRegistryBusinessObjectCollection(Factory);
			OrgPartRelationRegistryBusinessObject orgProdRelation = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			orgProdRelation.OrgHeaderPK = org.PK;
			orgProdRelation.RelationshipType = "OWN";
			return new object[] { collection };
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OrgPartRelationRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrgPartRelationRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgPartRelationRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OrgPartRelationRegistryItem("", "", "", "", RegistryStorageFlags.System);
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
