using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchCodeMappingRegistryItemEditor))]
	public class BranchCodeMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new BranchCodeMappingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override object[] GetValidRegistryValues()
		{
			BranchCodeMappingRegistryBusinessObjectCollection collection = new BranchCodeMappingRegistryBusinessObjectCollection();
			BranchCodeMappingRegistryBusinessObject element = collection.AddNew();
			element.CodePK = GetChargeCodeToTest().PK;
			element.ExternalCode = "BOB";
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((BranchCodeMappingRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BranchCodeMappingRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BranchCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		GlbBranch GetChargeCodeToTest()
		{
			return Factory.LoadTop1<GlbBranch>(new ZQuery());//new ZGuid("FDD429D2-648C-4895-8F9F-06E90DED2BE5"));  // GB_Code = "SYD"
		}
	}
}
