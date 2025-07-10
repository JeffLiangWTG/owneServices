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
	[TestedType(typeof(BranchDepartmentCodeMappingRegistryItemEditor))]
	public class BranchDepartmentCodeMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new BranchDepartmentCodeMappingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override object[] GetValidRegistryValues()
		{
			BranchDepartmentCodeMappingRegistryBusinessObjectCollection collection = new BranchDepartmentCodeMappingRegistryBusinessObjectCollection();
			BranchDepartmentCodeMappingRegistryBusinessObject element = collection.AddNew();
			element.BranchCodePK = GetChargeCodeToTest().PK;
			element.ProfitCentre = "BOB";
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((BranchDepartmentCodeMappingRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BranchDepartmentCodeMappingRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BranchDepartmentCodeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System);
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
